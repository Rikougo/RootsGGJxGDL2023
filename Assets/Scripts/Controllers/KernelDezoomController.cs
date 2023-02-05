using System;
using Cinemachine;
using UnityEngine;

namespace Roots
{
    public class KernelDezoomController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera m_rootCamera;
        [SerializeField] private Transform m_trackingPoint;
        [SerializeField] private PlayerController m_playerController;
        
        [SerializeField] private float m_startY;
        [SerializeField] private float m_endY;

        [SerializeField] private float m_baseLensSize;
        [SerializeField] private float m_targetLensSize;

        private bool m_processing = false;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(
                new Vector3(0.0f, m_startY + ((m_endY - m_startY) / 2.0f), 0.0f),
                new Vector3(100.0f, m_endY - m_startY, 100.0f));
        }

        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (p_collider.gameObject.CompareTag("Player"))
            {
                m_processing = true;
            }
        }
        
        private void OnTriggerExit2D(Collider2D p_collider)
        {
            if (p_collider.gameObject.CompareTag("Player"))
            {
                m_processing = false;
            }
        }

        private void Update()
        {
            if (!m_processing) return;
            if (m_playerController.ZoomMode) return;

            float l_depth = m_trackingPoint.position.y;

            l_depth -= m_startY;
            float l_progress = l_depth / (m_endY - m_startY);

            m_rootCamera.m_Lens.OrthographicSize = Mathf.Lerp(m_baseLensSize, m_targetLensSize, l_progress);
        }
    }
}
