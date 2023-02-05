using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roots
{
    public class Collectible : MonoBehaviour
    {
        public enum CollectibleType
        {
            REFILL = 0,
            ITEM   = 1
        }

        [SerializeField] private CollectibleType m_type = CollectibleType.REFILL;
        [SerializeField] private BoundariesController.ZoneUnlock m_unlockedZone = BoundariesController.ZoneUnlock.WATER;
        [SerializeField] private GameObject m_relatedTree;

        public CollectibleType Type => m_type;
        public BoundariesController.ZoneUnlock Zone => m_unlockedZone;
        public GameObject TreeGO => m_relatedTree;
    }
}
