using System;
using UnityEngine;

namespace Roots
{
    public class RootHead : MonoBehaviour
    {
        [SerializeField] private float m_radius = 0.5f;
        [SerializeField] private LayerMask m_boundaryLayerMask;
        [SerializeField] private LayerMask m_itemLayerMask;
        
        public Action<Collider2D> OnCollide;
        public Action<Collectible> OnPickup;
        
        public void FixedUpdate()
        {
            Collider2D l_groundCollider = Physics2D.OverlapCircle(transform.position, m_radius, m_boundaryLayerMask);
            if (l_groundCollider)
            {
                OnCollide?.Invoke(l_groundCollider);
            }

            Collider2D l_itemCollider = Physics2D.OverlapCircle(transform.position, m_radius, m_itemLayerMask);
            if (l_itemCollider)
            {
                OnPickup?.Invoke(l_itemCollider.GetComponent<Collectible>());
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_radius);
        }
    }
}