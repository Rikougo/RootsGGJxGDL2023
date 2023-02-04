using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Roots
{
    public class RootHead : MonoBehaviour
    {
        [SerializeField] private float m_radius = 0.5f;
        [SerializeField] private LayerMask m_boundaryLayerMask;
        [SerializeField] private LayerMask m_itemLayerMask;
        
        public Action OnCollide;
        public Action<Collectible> OnPickup;
        
        public void FixedUpdate()
        {
            if (Physics2D.OverlapCircle(transform.position, m_radius, m_boundaryLayerMask))
            {
                OnCollide?.Invoke();
            }

            Collider2D l_itemCollider = Physics2D.OverlapCircle(transform.position, m_radius, m_itemLayerMask);
            if (l_itemCollider)
            {
                OnPickup?.Invoke(l_itemCollider.GetComponent<Collectible>());
                
                Destroy(l_itemCollider.gameObject);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_radius);
        }
    }
}