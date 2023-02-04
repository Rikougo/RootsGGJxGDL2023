using System;
using System.Collections;
using System.Collections.Generic;
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
        
        [SerializeField] private GameObject m_waterBoundary;
        [SerializeField] private GameObject m_rockBoundary;
        [SerializeField] private GameObject m_lavaBoundary;
        [SerializeField] private GameObject m_endBoundary;

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
                    m_waterBoundary.layer = 0;
                    break;
                case ZoneUnlock.ROCK:
                    m_rockBoundary.layer = 0;
                    break;
                case ZoneUnlock.LAVA:
                    m_lavaBoundary.layer = 0;
                    break;
                case ZoneUnlock.END:
                    m_endBoundary.layer = 0;
                    break;
            }
        }
    }
}
