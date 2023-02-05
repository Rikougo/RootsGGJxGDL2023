using UnityEngine;

namespace Roots
{
    public class UIBorder : MonoBehaviour
    {
        [SerializeField] private float m_shownY;
        [SerializeField] private float m_hiddenY;

        private bool m_animated;
        private bool m_show;
        private float m_timer;
        private float m_duration;

        public void UpdatePosition(float p_progress)
        {
            Vector2 l_position = ((RectTransform)transform).anchoredPosition;
            l_position.y = m_hiddenY + m_shownY * p_progress;
            ((RectTransform)transform).anchoredPosition = l_position;
        }

        public void Show(float p_time)
        {
            if (m_animated) return;

            if (p_time <= 0.0f)
            {
                Vector2 l_position = ((RectTransform)transform).anchoredPosition;
                l_position.y = m_shownY;
                ((RectTransform)transform).anchoredPosition = l_position;
                return;
            }

            m_animated = true;
            m_show = true;
            m_duration = p_time;
            m_timer = 0.0f;
        }

        public void Hide(float p_time)
        {
            if (m_animated) return;
            
            if (p_time <= 0.0f)
            {
                Vector2 l_position = ((RectTransform)transform).anchoredPosition;
                l_position.y = m_hiddenY;
                ((RectTransform)transform).anchoredPosition = l_position;
                return;
            }
            
            m_animated = true;
            m_show = false;
            m_duration = p_time;
            m_timer = 0.0f;
        }

        private void Update()
        {
            if (!m_animated) return;

            float l_progress = (m_show ? (m_timer / m_duration) : (1.0f - (m_timer / m_duration)));
            UpdatePosition(l_progress);
            m_timer += Time.deltaTime;

            if (m_timer > m_duration) m_animated = false;
        }
    }
}