using System;
using UnityEngine;

namespace Roots
{
    public class BoundariesController : MonoBehaviour
    {
        [Flags]
        public enum ZoneUnlock
        {
            NONE  = 0,
            WATER = 1,
            ROCK  = 2,
            LAVA  = 3,
            END   = 4,
        }
        
        [SerializeField] private GameObject[] m_waterBoundary;
        [SerializeField] private GameObject[] m_rockBoundary;
        [SerializeField] private GameObject[] m_lavaBoundary;
        [SerializeField] private GameObject[] m_endBoundary;

        private ZoneUnlock m_unlocked;

        private void Awake()
        {
            m_unlocked = ZoneUnlock.NONE;
        }

        public void UnlockZone(ZoneUnlock p_zone)
        {
            if (m_unlocked + 1 != p_zone)
            {
                Debug.LogWarning("Trying to unlock zone that is not the next zone.");
                return;
            }

            m_unlocked = p_zone;

            switch (m_unlocked)
            {
                case ZoneUnlock.WATER:
                    foreach (GameObject l_go in m_waterBoundary) 
                        l_go.layer = 0;
                    break;
                case ZoneUnlock.ROCK:
                    foreach (GameObject l_go in m_rockBoundary) 
                        l_go.layer = 0;
                    break;
                case ZoneUnlock.LAVA:
                    foreach (GameObject l_go in m_lavaBoundary) 
                        l_go.layer = 0;
                    break;
                case ZoneUnlock.END:
                    foreach (GameObject l_go in m_endBoundary) 
                        l_go.layer = 0;
                    break;
            }
        }
    }
}
