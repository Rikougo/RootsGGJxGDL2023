using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Roots
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private GameObject m_menu;
        [SerializeField] private GameObject m_credits;
        [SerializeField] private CinemachineVirtualCamera m_menuCamera;
        
        void Start()
        {
            HideShowMenu(false);
        }

        public void PlayGame()
        {
            HideShowMenu(true);
        }

        public void PauseGame()
        {
            HideShowMenu(false);
        }

        private void HideShowMenu(bool p_hide)
        {
            m_menu.SetActive(!p_hide);
            m_menuCamera.Priority = p_hide ? 0 : 999;
            m_playerController.enabled = p_hide;
        }

        public void HideShowCredit(bool p_hide)
        {
            m_credits.SetActive(!p_hide);
            m_menu.SetActive(p_hide);
        }
    }
}
