using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Roots
{
    public class UnlockController : MonoBehaviour
    {
        [SerializeField] private float m_wanderingDuration = 4.0f;
        [SerializeField] private float m_moveTowardDuration = 0.2f;
        [SerializeField] private float m_stayOnTreeDuration = 0.2f;
        
        [SerializeField] private CinemachineVirtualCamera m_rootVirtualCamera;
        [SerializeField] private CinemachineVirtualCamera m_treeVirtualCamera;
        [SerializeField] private Volume m_globalBloomVolume;
        [SerializeField] private UIBorder m_topBorder, m_bottomBorder;
        [SerializeField] private AudioClip m_itemFoundSound;

        private VolumeProfile m_globalBloomProfile;
        private Bloom m_bloomOverride;
        private ColorAdjustments m_colorAdjustmentsOverride;

        private bool m_playing;
        private float m_wanderingTimer;
        private float m_moveTowardTimer;
        private float m_stayOnTreeTimer;

        private Vector3 m_initialRootPos;
        private PlayerController m_playerController;
        private Collectible m_collectible;

        private void Start()
        {
            m_globalBloomProfile = m_globalBloomVolume.profile;
            m_globalBloomProfile.TryGet<Bloom>(out m_bloomOverride);
            m_globalBloomProfile.TryGet<ColorAdjustments>(out m_colorAdjustmentsOverride);
        }
        
        public void ItemCollected(PlayerController p_playerController, Collectible p_collectible)
        {
            if (p_collectible.Type != Collectible.CollectibleType.ITEM)
            {
                Debug.LogWarning("Wrong collectible type submitted.");
                return;
            }

            m_playerController = p_playerController;
            m_playerController.InCinematic = true;
            m_initialRootPos = m_playerController.CurrentTrackedPosition;
            m_collectible = p_collectible;
            
            m_playing = true;
            m_wanderingTimer = 0.0f;
            m_moveTowardTimer = 0.0f;
            m_stayOnTreeTimer = 0.0f;

            FindObjectOfType<RootHead>().enabled = false;
            
            var l_soundController = FindObjectOfType<SoundController>();
            l_soundController.SetMusicVolume(0.0f);
            l_soundController.PlaySound(m_itemFoundSound);
            m_colorAdjustmentsOverride.active = false;
            
            m_topBorder.Show(0.5f);
            m_bottomBorder.Show(0.5f);
        }

        private void Update()
        {
            if (!m_playing) return;

            if (m_wanderingTimer < m_wanderingDuration)
            {
                m_wanderingTimer += Time.deltaTime;
            }
            else if (m_moveTowardTimer < m_moveTowardDuration)
            {
                m_moveTowardTimer += Time.deltaTime;
                float l_progress = m_moveTowardTimer / m_moveTowardDuration;
                m_playerController.CurrentTrackedPosition = Vector3.Lerp(
                    m_initialRootPos,
                    m_collectible.transform.position,
                    l_progress);

                m_bloomOverride.intensity.value = l_progress * 7.0f;
                m_bloomOverride.threshold.value = 1.0f - l_progress;

                if (m_moveTowardTimer >= m_moveTowardDuration)
                {
                    m_treeVirtualCamera.Priority = 15;
                    m_rootVirtualCamera.Priority = 5;
                    
                    FindObjectOfType<TreeController>().UpdateTree(m_collectible.TreeGO);
                    Destroy(m_collectible.gameObject);
                }
            } else if (m_stayOnTreeTimer < m_stayOnTreeDuration)
            {
                m_stayOnTreeTimer += Time.deltaTime;
                float l_progress = Mathf.Clamp((m_stayOnTreeTimer / m_stayOnTreeDuration) * 1.2f, 0.0f, 1.0f);
                
                m_bloomOverride.intensity.value = (1.0f - l_progress) * 7.0f;
                m_bloomOverride.threshold.value = l_progress;
            }
            else
            {
                m_topBorder.Hide(0.5f);
                m_bottomBorder.Hide(0.5f);
                OnEnd();
            }
        }

        private void OnEnd()
        {
            m_colorAdjustmentsOverride.active = true;
            m_rootVirtualCamera.Priority = 15;
            m_treeVirtualCamera.Priority = 5;
            
            m_playing = false;
            
            FindObjectOfType<SoundController>().SetMusicVolume(1.0f);
            FindObjectOfType<RootHead>().enabled = true;
            m_playerController.InCinematic = false;
        }
    }
}