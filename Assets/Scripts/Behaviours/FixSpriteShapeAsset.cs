using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Roots
{
    public class FixSpriteShapeAsset : MonoBehaviour
    {
        [SerializeField] private Sprite m_baseRoot;
        [SerializeField] private SpriteShape m_shape;

        public void Start()
        {
            m_shape.angleRanges[0].sprites[0] = m_baseRoot;
        }
    }
}
