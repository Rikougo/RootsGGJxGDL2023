using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roots
{
    public class PassiveRegenArea : MonoBehaviour
    {
        [SerializeField] private float m_regenFactor;
        private PlayerController m_playerController;
        private bool m_regen = false;

        private void Start()
        {
            m_playerController = FindObjectOfType<PlayerController>();
        }
        
        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (p_collider.gameObject.CompareTag("Player"))
            {
                m_regen = true;
            }
        }

        private void OnTriggerExit2D(Collider2D p_collider)
        {
            if (p_collider.gameObject.CompareTag("Player"))
            {
                m_regen = false;
            }
        }

        private void Update()
        {
            if (!m_regen) return;

            m_playerController.Capacity += Time.deltaTime * m_regenFactor;
        }
    }
}
