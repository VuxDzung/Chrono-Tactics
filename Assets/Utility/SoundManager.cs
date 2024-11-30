using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace DevOpsGuy
{
    public enum MusicCategory
    {
        None,
        Normal,
        PrepareForBattle,
    }

    [Serializable]
    public struct BGMusic
    {
        public MusicCategory category;
        public AudioClip clip;
    }

    public class SoundManager : M_Singleton<SoundManager>
    {
        [SerializeField] private List<BGMusic> bgMusicList;

        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioSource bgAudioSource;

        protected override void Awake()
        {
            base.Awake();
        }

        public void Play(AudioClip clip)
        {
            sfxAudioSource.PlayOneShot(clip, 1);
        }

        public void PlayBGMusic(MusicCategory category, bool loop)
        {
            BGMusic music = bgMusicList.FirstOrDefault(_music => _music.category.Equals(category));
            
            if (bgAudioSource.clip != music.clip)
            {
                bgAudioSource.clip = music.clip;
                bgAudioSource.loop = loop;
                bgAudioSource.Play();
            }
        }
    }
}