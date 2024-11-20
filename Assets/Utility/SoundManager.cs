using UnityEngine;
using Utils;

namespace DevOpsGuy
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : M_Singleton<SoundManager>
    {
        private AudioSource source;

        protected override void Awake()
        {
            base.Awake();
            source = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip)
        {
            source.PlayOneShot(clip, 1);
        }
    }
}