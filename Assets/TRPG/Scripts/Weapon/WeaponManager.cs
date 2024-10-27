using FishNet.Object;
using FishNet.Object.Synchronizing;
using TRPG.Unit;
using UnityEngine;

namespace TRPG
{
    public class WeaponManager : CoreNetworkBehaviour
    {
        [SerializeField] private string testPrimaryId;
        [SerializeField] private string testSecondaryId;
        [SerializeField] private WeaponConfigSO config;

        protected UnitController context;

        private readonly SyncVar<string> selectedWeaponId = new SyncVar<string>();
        private readonly SyncVar<string> primaryWeaponId = new SyncVar<string>();
        private readonly SyncVar<string> secondaryWeaponId = new SyncVar<string>();

        private readonly SyncVar<BaseWeapon> primaryWeaponObj = new SyncVar<BaseWeapon>();
        private readonly SyncVar<BaseWeapon> secondaryWeaponObj = new SyncVar<BaseWeapon>();

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                LoadSelectedWeapons();
            }
        }

        /// <summary>
        /// This must be called on both client and server. Because it will spawn a non-networked object.
        /// </summary>
        [ServerRpc]
        private void LoadSelectedWeapons()
        {
            primaryWeaponId.Value = testPrimaryId;
            secondaryWeaponId.Value = testSecondaryId;

            WeaponData primaryWeaponData = config.GetDataBydId(primaryWeaponId.Value);
            WeaponData secondaryWeaponData = config.GetDataBydId(secondaryWeaponId.Value);

            //Initialize weapons.
            BaseWeapon _primaryWeapon = Instantiate(primaryWeaponData.weaponPrefab);
            BaseWeapon _secondaryWeapon = Instantiate(secondaryWeaponData.weaponPrefab);

            //Init weapons on server.
            ServerManager.Spawn(_primaryWeapon.gameObject, Owner);
            ServerManager.Spawn(_secondaryWeapon.gameObject, Owner);

            _primaryWeapon.SetParent(context.BoneController.GetBoneRefByHandler(primaryWeaponData.activeHandler).transform);
            _secondaryWeapon.SetParent(context.BoneController.GetBoneRefByHandler(secondaryWeaponData.inactiveHandler).transform);

            primaryWeaponObj.Value = _primaryWeapon;
            secondaryWeaponObj.Value = _secondaryWeapon;
        }

        private void ChangeWeapon()
        {
            if (!selectedWeaponId.Value.Equals(primaryWeaponId.Value))
                selectedWeaponId.Value = primaryWeaponId.Value;
            if (!selectedWeaponId.Value.Equals(secondaryWeaponId.Value))
                selectedWeaponId.Value = secondaryWeaponId.Value;
        }
    }
}