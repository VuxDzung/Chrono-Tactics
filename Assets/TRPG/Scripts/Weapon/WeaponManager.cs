using DevOpsGuy.GUI;
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
        [SerializeField] protected WeaponConfigSO config;

        protected UnitController context;

        protected readonly SyncVar<string> selectedWeaponId = new SyncVar<string>();
        protected readonly SyncVar<string> primaryWeaponId = new SyncVar<string>();
        protected readonly SyncVar<string> secondaryWeaponId = new SyncVar<string>();

        protected readonly SyncVar<BaseWeapon> primaryWeaponObj = new SyncVar<BaseWeapon>();
        protected readonly SyncVar<BaseWeapon> secondaryWeaponObj = new SyncVar<BaseWeapon>();
        protected readonly SyncVar<BaseWeapon> selectedWeapon = new SyncVar<BaseWeapon>();


        public WeaponConfigSO Config => config;
        public BaseWeapon CurrentWeapon => selectedWeapon.Value;
        public WeaponData CurrentWeaponData => config.GetDataBydId(selectedWeaponId.Value);

        public virtual void Setup(UnitController context)
        {
            this.context = context;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (Owner != null && IsOwner)
            {
                LoadSelectedWeapons();
            }
        }

        [Client]
        public virtual void LoadWeaponUI()
        {
            WeaponData primaryWeaponData = config.GetDataBydId(primaryWeaponId.Value);
            WeaponData secondaryWeaponData = config.GetDataBydId(secondaryWeaponId.Value);
            UIManager.GetUIStatic<HUD>().SwapSpriteField.SetSprites(primaryWeaponData.thumbnail, secondaryWeaponData.thumbnail);
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

            // Instantiate and spawn primary weapon
            BaseWeapon _primaryWeapon = Instantiate(primaryWeaponData.weaponPrefab);
            ServerManager.Spawn(_primaryWeapon.gameObject, Owner);
            primaryWeaponObj.Value = _primaryWeapon;

            // Instantiate and spawn secondary weapon
            BaseWeapon _secondaryWeapon = Instantiate(secondaryWeaponData.weaponPrefab);
            ServerManager.Spawn(_secondaryWeapon.gameObject, Owner);
            secondaryWeaponObj.Value = _secondaryWeapon;

            selectedWeapon.Value = primaryWeaponObj.Value;
            selectedWeaponId.Value = primaryWeaponId.Value;

            _primaryWeapon.SetParent(context.BoneController.GetBoneRefByHandler(primaryWeaponData.activeHandler).transform);
            _secondaryWeapon.SetParent(context.BoneController.GetBoneRefByHandler(secondaryWeaponData.inactiveHandler).transform);
        }

        [ServerRpc]
        private void ChangeWeapon()
        {
            WeaponData currentData = config.GetDataBydId(selectedWeaponId.Value);   
            selectedWeapon.Value.SetParent(context.BoneController.GetBoneRefByHandler(currentData.inactiveHandler).transform);

            if (!selectedWeaponId.Value.Equals(primaryWeaponId.Value))
            {
                selectedWeaponId.Value = primaryWeaponId.Value;
                selectedWeapon.Value = primaryWeaponObj.Value;
            }

            if (!selectedWeaponId.Value.Equals(secondaryWeaponId.Value))
            {
                selectedWeaponId.Value = secondaryWeaponId.Value;
                selectedWeapon.Value = secondaryWeaponObj.Value;
            }

            WeaponData newData = config.GetDataBydId(selectedWeaponId.Value);
            selectedWeapon.Value.SetParent(context.BoneController.GetBoneRefByHandler(currentData.activeHandler).transform);
        }
    }
}