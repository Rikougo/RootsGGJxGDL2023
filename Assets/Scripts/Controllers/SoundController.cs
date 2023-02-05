using UnityEngine;

namespace Roots
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource m_musicPlayer;
        [SerializeField] private AudioSource[] m_pool;
        private bool[] m_used;
        private float[] m_timers;

        private void Start()
        {
            m_used = new bool[m_pool.Length];
            m_timers = new float[m_pool.Length];

            for (int l_idx = 0; l_idx < m_pool.Length; l_idx++)
            {
                m_used[l_idx] = false;
                m_timers[l_idx] = 0.0f;
            }
        }

        public bool PlaySound(AudioClip p_clip)
        {
            int l_index = GetFirstUnused();

            if (l_index == -1)
            {
                Debug.LogWarning("Couldn't play sound, no player available.");
                return false;
            }
            
            m_pool[l_index].Stop();
            m_pool[l_index].clip = p_clip;
            m_pool[l_index].Play();

            m_used[l_index] = true;
            m_timers[l_index] = p_clip.length;

            return true;
        }

        public void SetMusic(AudioClip p_clip)
        {
            m_musicPlayer.Stop();
            m_musicPlayer.clip = p_clip;
            m_musicPlayer.loop = true;
            m_musicPlayer.Play();
        }

        public void SetMusicVolume(float p_volume)
        {
            m_musicPlayer.volume = p_volume;
        }

        private int GetFirstUnused()
        {
            for (int l_idx = 0; l_idx < m_used.Length; l_idx++)
            {
                if (!m_used[l_idx]) return l_idx;
            }

            return -1;
        }

        private void Update()
        {
            for (int l_idx = 0; l_idx < m_pool.Length; l_idx++)
            {
                if (m_used[l_idx])
                {
                    m_timers[l_idx] -= Time.deltaTime;

                    if (m_timers[l_idx] <= 0.0f)
                    {
                        m_used[l_idx] = false;
                    }
                }
            }
        }
    }
}
