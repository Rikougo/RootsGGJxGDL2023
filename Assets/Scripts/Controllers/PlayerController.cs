
using Cinemachine;
using Roots.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

namespace Roots
{
    [RequireComponent(typeof(SpriteShapeControllerPool), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player stats")]
        [SerializeField] private float m_squareSelectThreshold = 0.25f;
        [SerializeField] private float m_rootSpeed = 2.0f;
        [SerializeField] private float m_cutSize = 1.0f;
        [SerializeField] private float m_cameraSpeed = 2.0f;
        [SerializeField] private float m_maxCapacity = 10.0f;
        [SerializeField] private float m_recoverFactor = 0.5f;
            
        [Header("Components")]
        [SerializeField] private Transform m_trackingPoint;
        [SerializeField] private Transform m_hoverSelectTransform;
        [SerializeField] private CinemachineVirtualCamera m_virtualCamera;
        [SerializeField] private Volume m_globalVolume;
        [SerializeField] private AudioClip m_startMovingSound;

        private VolumeProfile m_globalProfile;
        private SpriteShapeControllerPool m_shapePool;
        private PlayerInput m_input;

        private bool m_zoomMode;
        private bool m_clicking;
        private Vector3 m_cursorPosition;
        private Vector2 m_cursorDelta;
        
        private Vector3 m_lastDirection;
        private float m_currentCapacity;
        private float m_currentSize;
        private float m_deepestGone;
        
        // current spline point working on
        private int m_currentPointIdx = 2;
        
        private float m_disabledTimer = 0.0f;

        public Vector3 CurrentDirection => m_lastDirection;
        public Vector3 CursorPosition => m_cursorPosition;

        public Vector3 CurrentTrackedPosition
        {
            get => m_shapePool.CurrentSpriteShape.spline.GetPosition(m_currentPointIdx);
            set => m_shapePool.CurrentSpriteShape.spline.SetPosition(m_currentPointIdx, value);
        }
        public bool InCinematic { get; set; }
        public Transform TrackingPoint => m_trackingPoint;
        public bool ZoomMode
        {
            get => m_zoomMode;
            private set
            {
                if (m_zoomMode == value) return;
                
                m_zoomMode = value;

                m_trackingPoint.GetComponent<RootHead>().enabled = !m_zoomMode;
                m_virtualCamera.m_Lens.OrthographicSize = m_zoomMode ? 15.0f : 5.0f;
                if (!m_zoomMode)
                {
                    m_trackingPoint.position = CurrentTrackedPosition;
                }
            }
        }

        public float Capacity
        {
            get => m_currentCapacity;
            set => m_currentCapacity = Mathf.Min(value, m_maxCapacity);
        }

        private void Awake()
        {
            m_shapePool = GetComponent<SpriteShapeControllerPool>();
            m_input = GetComponent<PlayerInput>();

            m_clicking = false;
            m_zoomMode = false;

            m_lastDirection = Vector3.zero;
            m_currentCapacity = m_maxCapacity;
            m_currentSize = 0.0f;
        }
        
        private void OnEnable()
        {
            m_globalProfile = m_globalVolume.profile;
            
            if (m_trackingPoint != null)
            {
                m_trackingPoint.GetComponent<RootHead>().OnCollide += KnockBack;
                m_trackingPoint.GetComponent<RootHead>().OnPickup += Pickup;
            }

            m_input.actions["Click"].started += OnClick;
            m_input.actions["Click"].canceled += OnClick;

            m_input.actions["Cursor"].performed += OnCursor;
            m_input.actions["CursorDelta"].performed += OnCursorDelta;
            
            m_input.actions["SplitDbg"].performed += OnSplit;

            m_input.actions["Switch"].performed += OnSwitch;
        }

        private void OnDisable()
        {
            if (m_trackingPoint != null)
            {
                m_trackingPoint.GetComponent<RootHead>().OnCollide -= KnockBack;
                m_trackingPoint.GetComponent<RootHead>().OnPickup -= Pickup;
            }
            
            
            m_input.actions["Click"].started -= OnClick;
            m_input.actions["Click"].canceled -= OnClick;
            
            m_input.actions["Cursor"].performed -= OnCursor;
            m_input.actions["CursorDelta"].performed -= OnCursorDelta;
            
            m_input.actions["SplitDbg"].performed -= OnSplit;
            
            m_input.actions["Switch"].performed -= OnSwitch;
        }

        private void Update()
        {
            if (InCinematic) return;
            
            if (ZoomMode) UpdateCamera();
            else UpdateRoot();

            m_deepestGone = Mathf.Min(m_deepestGone, CurrentTrackedPosition.y);
            
            if (ZoomMode || !m_clicking)
            {
                // PASSIVE CAPACITY INCREASE
                Capacity += Time.deltaTime * m_recoverFactor;
            }

            if (m_globalProfile.TryGet<ColorAdjustments>(out var l_adjustments))
            {
                l_adjustments.saturation.value = Mathf.Min(0.0f, (1.0f - (m_currentCapacity / m_maxCapacity)) * -80.0f);
            }
        }

        private void UpdateCamera()
        {
            if (m_clicking)
            {
                Vector3 l_position = m_trackingPoint.position;
                l_position -= new Vector3(m_cursorDelta.x, m_cursorDelta.y, 0.0f) * 
                                            (Time.deltaTime * m_cameraSpeed);
                l_position.y = Mathf.Max(l_position.y, m_deepestGone);
                m_trackingPoint.position = l_position;

            }
        }
        
        private void UpdateRoot()
        {
            if (m_disabledTimer > 0.0f)
            {
                Vector3 l_currentSelectedPoint = CurrentTrackedPosition;
                l_currentSelectedPoint.z = 0;

                l_currentSelectedPoint -= CurrentDirection * Time.deltaTime;

                float l_previousPointDist = (l_currentSelectedPoint -
                                             m_shapePool.CurrentSpriteShape.spline.GetPosition(m_currentPointIdx - 1)).magnitude;
                if (l_previousPointDist < 0.1f)
                {
                    m_shapePool.CurrentSpriteShape.spline.RemovePointAt(m_currentPointIdx);
                    m_currentPointIdx--;
                }

                CurrentTrackedPosition = l_currentSelectedPoint;
                m_trackingPoint.position = l_currentSelectedPoint;
                
                m_disabledTimer -= Time.deltaTime;
                return;
            }

            if (m_currentPointIdx == -1) return;
            
            if (m_clicking)
            {
                if (m_hoverSelectTransform.gameObject.activeSelf)
                    m_hoverSelectTransform.gameObject.SetActive(false);
                
                if (m_currentCapacity <= 0.0f)
                {
                    return;
                }

                Vector3 l_currentSelectedPoint = CurrentTrackedPosition;
                l_currentSelectedPoint.z = 0;

                // if cursor is too close, don't move
                Vector3 l_delta = (m_cursorPosition - l_currentSelectedPoint);
                if (l_delta.sqrMagnitude < 0.2f) return;
                
                Vector3 l_direction = l_delta.normalized;
                float l_deltaPosition = m_rootSpeed * Time.deltaTime;
                l_currentSelectedPoint += l_direction * l_deltaPosition;
                
                CurrentTrackedPosition = l_currentSelectedPoint;
                
                m_currentSize += l_deltaPosition;

                if (m_currentSize > m_cutSize)
                {
                    m_shapePool.CurrentSpriteShape.spline.InsertPointAt(
                        m_currentPointIdx + 1, l_currentSelectedPoint + l_direction * 0.05f);
                    m_shapePool.CurrentSpriteShape.spline.SetTangentMode(
                        m_currentPointIdx + 1, ShapeTangentMode.Continuous);
                    m_shapePool.CurrentSpriteShape.spline.SetLeftTangent(m_currentPointIdx + 1, -l_direction * 0.1f);
                    m_shapePool.CurrentSpriteShape.spline.SetRightTangent(m_currentPointIdx + 1, l_direction * 0.1f);
                    
                    m_currentPointIdx++;
                    m_currentSize = 0.0f;

                    m_lastDirection = l_direction;
                }

                m_trackingPoint.position = l_currentSelectedPoint;
                m_lastDirection = l_direction;
                m_currentCapacity -= Time.deltaTime;
            }
            else
            {
                Vector3[] l_leafPosition = m_shapePool.LeafPositions;
                
                for (int l_index = 0; l_index < l_leafPosition.Length; l_index++)
                {
                    if ((m_cursorPosition.XY() - l_leafPosition[l_index].XY()).magnitude < m_squareSelectThreshold)
                    {
                        m_hoverSelectTransform.position = l_leafPosition[l_index];
                        m_hoverSelectTransform.gameObject.SetActive(true);
                        return;
                    }
                }
                
                m_hoverSelectTransform.gameObject.SetActive(false);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_cursorPosition, 0.2f);
        }

        private void KnockBack(Collider2D p_groundCollider)
        {
            m_disabledTimer = 0.5f;
        }

        private void Pickup(Collectible p_collectible)
        {
            if (InCinematic) return;
            
            if (p_collectible.Type == Collectible.CollectibleType.ITEM)
            {
                m_clicking = false;
                m_currentCapacity = m_maxCapacity;

                FindObjectOfType<BoundariesController>().UnlockZone(p_collectible.Zone);
                FindObjectOfType<UnlockController>().ItemCollected(this, p_collectible);
            }
            else
            {
                m_shapePool.SplitCurrent(CurrentDirection);

                m_currentPointIdx = 1;
                m_currentSize = 0.0f;

                m_currentCapacity = m_maxCapacity;
                
                Destroy(p_collectible.gameObject);
            }
        }
        
        #region EventListeners

        private void OnClick(InputAction.CallbackContext p_ctx)
        {
            if (InCinematic) return;
            
            m_clicking = p_ctx.started && m_currentCapacity > 1.0f;
            
            if (!ZoomMode && p_ctx.started) FindObjectOfType<SoundController>().PlaySound(m_startMovingSound);

            // On click, select closest leaf
            // TODO IMPROVE ACCURACY (USING COLLIDER ?)
            if (p_ctx.started)
            {
                Vector3[] l_leafPosition = m_shapePool.LeafPositions;

                for (int l_index = 0; l_index < l_leafPosition.Length; l_index++)
                {
                    if ((m_cursorPosition.XY() - l_leafPosition[l_index].XY()).magnitude < m_squareSelectThreshold)
                    {
                        m_currentPointIdx = m_shapePool.Select(l_index);
                        ZoomMode = false;
                        m_trackingPoint.position = CurrentTrackedPosition;
                        break;
                    }
                }
                
                Vector3 l_currentSelectedPoint = CurrentTrackedPosition;
                l_currentSelectedPoint.z = 0;
                
                Vector3 l_delta = (m_cursorPosition - l_currentSelectedPoint);
                m_lastDirection = l_delta.normalized;
            }
        }

        private void OnCursor(InputAction.CallbackContext p_ctx)
        {
            if (InCinematic) return;
            
            m_cursorPosition = Camera.main.ScreenToWorldPoint(p_ctx.ReadValue<Vector2>());
            m_cursorPosition.z = 0;
        }

        private void OnCursorDelta(InputAction.CallbackContext p_ctx)
        {
            if (InCinematic) return;
            
            m_cursorDelta = p_ctx.ReadValue<Vector2>();
        }

        private void OnSplit(InputAction.CallbackContext p_ctx)
        {
            if (InCinematic) return;
            
            m_shapePool.SplitCurrent(CurrentDirection);

            m_currentPointIdx = 1;
            m_currentSize = 0.0f;
        }

        private void OnSwitch(InputAction.CallbackContext p_ctx)
        {
            if (InCinematic) return;
            
            ZoomMode = !ZoomMode;
        }
        
        #endregion
    }
}
