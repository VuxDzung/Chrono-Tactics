using FishNet.Object;
using UnityEngine;

namespace TRPG
{
    public class BaseWeapon : CoreNetworkBehaviour
    {
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
        public virtual void OnDamageTarget()
        {

        }
    }
}