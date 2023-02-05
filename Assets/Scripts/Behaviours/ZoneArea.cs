using UnityEngine;

namespace Roots
{
    public class ZoneArea : MonoBehaviour
    {
        [SerializeField] private AudioClip m_music;
        
        private void OnTriggerEnter2D(Collider2D p_collider)
        {
            if (p_collider.gameObject.CompareTag("Player"))
            {
                FindObjectOfType<SoundController>().SetMusic(m_music);
            }
        }
    }
}
