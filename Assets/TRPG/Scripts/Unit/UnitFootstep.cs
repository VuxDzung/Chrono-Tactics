using UnityEngine;

namespace TRPG.Unit
{
    public class UnitFootstep : CoreNetworkBehaviour
    {
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip[] clips;

        public void OnFootstep(AnimationEvent animationEvent)
        {
            if (IsClientInitialized)
            {
                if (animationEvent.animatorClipInfo.weight > 0.1f)
                {
                    int clipIndex = Random.Range(0, clips.Length);
                    m_AudioSource.PlayOneShot(clips[clipIndex]);
                }
            }
        }
    }
}