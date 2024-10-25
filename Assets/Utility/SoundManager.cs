using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace DevOpsGuy
{
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