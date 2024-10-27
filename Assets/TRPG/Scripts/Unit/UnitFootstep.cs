using UnityEngine;

namespace TRPG.Unit
{
    public class UnitFootstep : CoreNetworkBehaviour
    {
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip[] clips;

        public void OnFootstep()
        {
            if (IsClientInitialized)
            {
                int clipIndex = Random.Range(0, clips.Length);
                m_AudioSource.PlayOneShot(clips[clipIndex]);
            }
        }
    }
}