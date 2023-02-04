
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
            
        [Header("Components")]
        [SerializeField] private Transform m_trackingPoint;
        [SerializeField] private Transform m_hoverSelectTransform;
        [SerializeField] private CinemachineVirtualCamera m_virtualCamera;
        [SerializeField] private Volume m_globalVolume;

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
        
        // current spline point working on
        private int m_currentPointIdx = 2;
        
        private float m_disabledTimer = 0.0f;

        public Vector3 CurrentDirection => m_lastDirection;
        public Vector3 CursorPosition => m_cursorPosition;

        public Vector3 CurrentTrackedPosition => m_shapePool.CurrentSpriteShape.spline.GetPosition(m_currentPointIdx);

        private bool ZoomMode
        {
            get => m_zoomMode;
            set
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

            m_input.actions["Click"].performed += OnClick;
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
            
            
            m_input.actions["Click"].performed -= OnClick;
            m_input.actions["Click"].canceled -= OnClick;
            
            m_input.actions["Cursor"].performed -= OnCursor;
            m_input.actions["CursorDelta"].performed -= OnCursorDelta;
            
            m_input.actions["SplitDbg"].performed -= OnSplit;
            
            m_input.actions["Switch"].performed -= OnSwitch;
        }

        private void Update()
        {
            if (ZoomMode) UpdateCamera();
            else UpdateRoot();

            if (ZoomMode || !m_clicking)
            {
                // PASSIVE CAPACITY INCREASE
                m_currentCapacity += Time.deltaTime;
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
                m_trackingPoint.position -= new Vector3(m_cursorDelta.x, m_cursorDelta.y, 0.0f) * 
                    (Time.deltaTime * m_cameraSpeed);
            }
        }
        
        private void UpdateRoot()
        {
            if (m_disabledTimer > 0.0f)
            {
                Vector3 l_currentSelectedPoint = CurrentTrackedPosition;
                l_currentSelectedPoint.z = 0;

                l_currentSelectedPoint += Vector3.up * Time.deltaTime;
                m_shapePool.CurrentSpriteShape.spline.SetPosition(m_currentPointIdx, l_currentSelectedPoint);
                m_trackingPoint.position = l_currentSelectedPoint;
                
                m_disabledTimer -= Time.deltaTime;
                return;
            }

            if (m_currentPointIdx == -1) return;
            
            if (m_clicking)
            {
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
                
                m_shapePool.CurrentSpriteShape.spline.SetPosition(m_currentPointIdx, l_currentSelectedPoint);
                
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

        private void KnockBack()
        {
            m_disabledTimer = 0.5f;
        }

        private void Pickup(Collectible p_collectible)
        {
            if (p_collectible.Type == Collectible.CollectibleType.ITEM)
            {
                FindObjectOfType<BoundariesController>().UnlockZone(p_collectible.Zone);
            }
            else
            {
                m_shapePool.SplitCurrent(CurrentDirection);

                m_currentPointIdx = 1;
                m_currentSize = 0.0f;

                m_currentCapacity = 10.0f;
            }
        }
        
        #region EventListeners

        private void OnClick(InputAction.CallbackContext p_ctx)
        {
            m_clicking = p_ctx.performed && m_currentCapacity > 1.0f;

            // On click, select closest leaf
            // TODO IMPROVE ACCURACY (USING COLLIDER ?)
            if (p_ctx.performed)
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
            m_cursorPosition = Camera.main.ScreenToWorldPoint(p_ctx.ReadValue<Vector2>());
            m_cursorPosition.z = 0;
        }

        private void OnCursorDelta(InputAction.CallbackContext p_ctx)
        {
            m_cursorDelta = p_ctx.ReadValue<Vector2>();
        }

        private void OnSplit(InputAction.CallbackContext p_ctx)
        {
            m_shapePool.SplitCurrent(CurrentDirection);

            m_currentPointIdx = 1;
            m_currentSize = 0.0f;
        }

        private void OnSwitch(InputAction.CallbackContext p_ctx)
        {
            ZoomMode = !ZoomMode;
        }
        
        #endregion
    }
}
