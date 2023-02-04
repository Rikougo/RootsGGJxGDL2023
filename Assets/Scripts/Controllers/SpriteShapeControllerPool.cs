using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.U2D;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Roots
{
    public class SpriteShapeControllerPool : MonoBehaviour
    {
        [SerializeField] private GameObject m_rootParent;
        [SerializeField] private SpriteShapeController m_newRootPrefab;
        
        private int m_currentIdx = 0;
        private List<SpriteShapeController> m_registeredSpriteShapes;

        public SpriteShapeController CurrentSpriteShape
        {
            get
            {
                if (m_registeredSpriteShapes.Count == 0 || m_currentIdx > m_registeredSpriteShapes.Count)
                {
                    return null;
                }
                
                return m_registeredSpriteShapes[m_currentIdx];
            }
        }

        public Vector3[] LeafPositions
        {
            get
            {
                Vector3[] l_res = new Vector3[m_registeredSpriteShapes.Count];
                
                for(int l_index = 0; l_index < m_registeredSpriteShapes.Count; l_index++)
                {
                    Spline l_spline = m_registeredSpriteShapes[l_index].spline;
                    l_res[l_index] = l_spline.GetPosition(l_spline.GetPointCount() - 1);
                }

                return l_res;
            }
        }

        public void Start()
        {
            m_registeredSpriteShapes = new List<SpriteShapeController>(m_rootParent.GetComponentsInChildren<SpriteShapeController>());
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) return;
            
            foreach (Vector3 l_position in LeafPositions)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(l_position, 0.1f);
            }
#endif
        }

        public int Select(int p_idx)
        {
            if (p_idx >= m_registeredSpriteShapes.Count) return -1;

            m_currentIdx = p_idx;
            return CurrentSpriteShape.spline.GetPointCount() - 1;
        }

        public void SplitCurrent(Vector3 p_currentDirection)
        {
            float l_dot = Vector3.Dot(p_currentDirection.normalized, Vector3.right);
            bool l_isRight = l_dot > 0.0f;

            SpriteShapeController l_shapeController = CurrentSpriteShape;
            Spline l_spline = l_shapeController.spline;
            Vector3 l_currentPosition = l_spline.GetPosition(l_spline.GetPointCount() - 1);
            
            m_registeredSpriteShapes.RemoveAt(m_currentIdx);

            Vector3 l_leftDirection = (Vector3.down + Vector3.left).normalized;
            Vector3 l_rightDirection = (Vector3.down + Vector3.right).normalized;

            SpriteShapeController l_leftShape = Instantiate(m_newRootPrefab, Vector3.zero, Quaternion.identity, m_rootParent.transform);
            l_leftShape.gameObject.name = $"{l_shapeController.gameObject.name}_L";
            l_leftShape.spline.SetPosition(0, l_currentPosition);
            
            l_leftShape.spline.SetLeftTangent(1, l_leftDirection * 0.1f);
            l_leftShape.spline.SetRightTangent(1,l_leftDirection * 0.1f);
            l_leftShape.spline.SetPosition(1, l_currentPosition + l_leftDirection * 0.25f);

            SpriteShapeController l_rightShape = Instantiate(m_newRootPrefab, Vector3.zero, Quaternion.identity, m_rootParent.transform);
            l_rightShape.gameObject.name = $"{l_shapeController.gameObject.name}_R";
            l_rightShape.spline.SetPosition(0, l_currentPosition);
            
            l_rightShape.spline.SetLeftTangent(1,  l_rightDirection * 0.1f);
            l_rightShape.spline.SetRightTangent(1, l_rightDirection * 0.1f);
            l_rightShape.spline.SetPosition(1, l_currentPosition + l_rightDirection * 0.25f);

            m_registeredSpriteShapes.Add(l_leftShape);
            m_registeredSpriteShapes.Add(l_rightShape);

            l_leftShape.enabled = true;
            l_rightShape.enabled = true;

            m_currentIdx = m_registeredSpriteShapes.Count - (l_isRight ? 1 : 2);
        }
    }
}