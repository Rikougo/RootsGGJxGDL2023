using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Roots
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private PlayerController m_playerController;
        [SerializeField] private GameObject m_menu;
        [SerializeField] private GameObject m_credits;
        [SerializeField] private CinemachineVirtualCamera m_menuCamera;
        [SerializeField] private PlayerInput m_input;
        
        void Start()
        {
            m_input.actions["Escape"].started += OnEscape;
            
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

        private void OnEscape(InputAction.CallbackContext p_ctx)
        {
            if (p_ctx.started)
            {
                if (!m_menu.activeSelf)
                {
                    HideShowMenu(false);
                }
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
