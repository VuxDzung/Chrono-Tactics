using FishNet.Object;
using UnityEngine;

namespace TRPG
{
    [RequireComponent(typeof(AudioSource))]
    public class BaseWeapon : CoreNetworkBehaviour
    {
        [SerializeField] private AudioSource m_AudioSource;
        [SerializeField] private AudioClip m_Clip;

        [Server]
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            SetParentCallback(transform.parent);
        }

        [ObserversRpc]
        protected virtual void SetParentCallback(Transform serverParent)
        {
            transform.SetParent(serverParent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Play Muzzle VFX, Fire SFX, Hit VFX/SFX (Melee).
        /// </summary>
        [Client]
        public virtual void OnDamageTarget(bool isOwner)
        {
            m_AudioSource.PlayOneShot(m_Clip);
        }
    }
}