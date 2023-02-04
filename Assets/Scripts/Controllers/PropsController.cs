using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Roots
{
    public class PropsController : MonoBehaviour
    {
        [Serializable]
        struct Prop
        {
            public Sprite sprite;
            public float weight;
        }
        
        [SerializeField] private BoxCollider2D m_waterArea;

        [Header("WaterProps")] 
        [SerializeField] private SpriteRenderer m_waterPropPrefab;
        [SerializeField] private Transform m_waterPropsHolder;
        [SerializeField] private Prop[] m_waterProps;
        [SerializeField] private int m_waterPropsAmount;
        [SerializeField] private bool m_debugGenerateWaterProps;
        [SerializeField] private bool m_debugCleanupWaterProps;

        private Transform[] m_waterPropsTransform;
        private Vector3[] m_waterPropsPositions;
        
        private void OnEnable()
        {
            CleanUpWaterProps();
            
            GenerateWaterProps();
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            UpdateWaterProps();
#else
            UpdateWaterProps();
#endif
        }

        private void OnDisable()
        {
            CleanUpWaterProps();
        }

        private void OnDestroy()
        {
            CleanUpWaterProps();
        }

        #region WaterZone

        private void GenerateWaterProps()
        {
            m_waterPropsTransform = new Transform[m_waterPropsAmount];
            m_waterPropsPositions = new Vector3[m_waterPropsAmount];

            Vector2 l_halfAreaSize = m_waterArea.size / 2.0f;
            Vector3 l_areaPosition = m_waterArea.transform.position;
            for (int l_index = 0; l_index < m_waterPropsAmount; l_index++)
            {
                // TODO Improve random position distribution to avoid collision
                float l_x = Random.Range(l_areaPosition.x - l_halfAreaSize.x, l_areaPosition.x + l_halfAreaSize.x);
                float l_y = Random.Range(l_areaPosition.y - l_halfAreaSize.y, l_areaPosition.y + l_halfAreaSize.y);

                SpriteRenderer l_renderer = Instantiate(
                    m_waterPropPrefab,
                    new Vector3(l_x, l_y, 0.0f),
                    Quaternion.identity,
                    m_waterPropsHolder);

                l_renderer.sprite = m_waterProps[Random.Range(0, m_waterProps.Length)].sprite;
                
                Transform l_transform = l_renderer.transform;
                float l_scale = Random.Range(0.8f, 1.2f);
                l_transform.localScale = new Vector3(l_scale, l_scale, l_scale);
                m_waterPropsTransform[l_index] = l_transform;
                m_waterPropsPositions[l_index] = l_transform.position;
            }
        }

        private void UpdateWaterProps()
        {
            if (m_waterPropsTransform == null || m_waterPropsPositions == null) return;

            for (int l_index = 0; l_index < m_waterPropsTransform.Length; l_index++)
            {
                Vector3 l_position = m_waterPropsPositions[l_index];
                Vector3 l_delta = Vector3.up * ((float)Math.Sin(Time.time + l_position.x * 0.25f) * 0.2f);
                m_waterPropsTransform[l_index].position = l_position + l_delta;
            }
        }

        private void CleanUpWaterProps()
        {
            if (m_waterPropsTransform == null)
            {
                m_waterPropsTransform = null;
                m_waterPropsPositions = null;
                return;
            }

            foreach (Transform l_child in m_waterPropsTransform)
            {
                if (l_child == null) 
                {
                    m_waterPropsTransform = null;
                    m_waterPropsPositions = null;
                    return;
                }
                
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying) Destroy(l_child.gameObject);
                else DestroyImmediate(l_child.gameObject);
#else
                Destroy(l_child.gameObject);
#endif
            }

            m_waterPropsTransform = null;
            m_waterPropsPositions = null;
        }
        
        #endregion
    }
}
