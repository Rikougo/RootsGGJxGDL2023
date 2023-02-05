using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Roots
{
    public class TreeController : MonoBehaviour
    {
        [SerializeField] private GameObject m_currentTree;

        public void UpdateTree(GameObject p_newGameObject)
        {
            m_currentTree.SetActive(false);
            m_currentTree = p_newGameObject;
            p_newGameObject.SetActive(true);
        }
    }
}
