using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roots
{
    public class ScalePulse : MonoBehaviour
    {
        [SerializeField] private AnimationCurve m_curve;

        // Time is second of one tick
        [SerializeField] private float m_frequency;

        // Value of added scale at max
        [SerializeField] private float m_addScale;

        private float m_baseScale;
        private float m_timer;

        private void Awake()
        {
            m_baseScale = transform.localScale.x;

            m_timer = 0.0f;
        }
        
        void Update()
        {
            if (m_frequency <= 0.0f) return;

            float l_progress = m_timer / m_frequency;
            l_progress = m_curve.Evaluate(l_progress);

            /*if (l_progress < 0.5f)
            {
                l_progress = l_progress * 2.0f;
            }
            else
            {
                l_progress = 1.0f - ((l_progress - 0.5f) * 2.0f);
            }*/

            float l_scale = m_baseScale + l_progress * m_addScale;

            transform.localScale = new Vector3(l_scale, l_scale, l_scale);

            m_timer += Time.deltaTime;

            if (m_timer >= m_frequency) m_timer = 0.0f;
        }
    }
}
