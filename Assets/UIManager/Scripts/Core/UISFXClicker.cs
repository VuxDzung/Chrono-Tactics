using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevOpsGuy.GUI
{
    public class UISFXClicker : MonoBehaviour
    {
        [SerializeField]
        protected AudioClip clip;

        public virtual void Play()
        {
            SoundManager.Singleton.Play(clip);
        }
    }
}