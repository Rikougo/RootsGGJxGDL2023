using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roots
{
    public class TrackingDebug : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}
