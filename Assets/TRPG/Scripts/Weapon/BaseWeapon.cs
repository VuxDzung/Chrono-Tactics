using FishNet;
using FishNet.Component.Transforming;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace TRPG
{
    [RequireComponent(typeof(AudioSource))]
    public class BaseWeapon : CoreNetworkBehaviour
    {
        private readonly SyncVar<Transform> netParent = new SyncVar<Transform>();
        [SerializeField] protected AudioSource m_AudioSource;
        [SerializeField] protected AudioClip m_Clip;

        [Server]
        public void SetParent(Transform parent)
        {
            netParent.Value = parent;// SetParent(parent, false);
            transform.parent = netParent.Value;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            SetParentCallback(netParent.Value);
        }

        [ObserversRpc]
        protected virtual void SetParentCallback(Transform serverParent)
        {
            transform.parent = serverParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            if (transform.parent != netParent.Value)
            {
                transform.parent = netParent.Value;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }    
        }

        /// <summary>
        /// Play Muzzle VFX, Fire SFX, Hit VFX/SFX (Melee).
        /// </summary>
        [Client]
        public virtual void OnDamageTarget(bool isOwner)
        {
            if (m_Clip != null) m_AudioSource.PlayOneShot(m_Clip);
        }
    }
}