using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIWeaponManager : WeaponManager
    {
        [SerializeField] private string aiWeaponId;

        public WeaponData CurrentAIWeaponData { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Init the AI weapon;
            CurrentAIWeaponData = config.GetDataBydId(aiWeaponId);

            // Instantiate and spawn primary weapon
            BaseWeapon _primaryWeapon = Instantiate(CurrentAIWeaponData.weaponPrefab);
            ServerManager.Spawn(_primaryWeapon.gameObject, Owner);
            primaryWeaponObj.Value = _primaryWeapon;
            

            _primaryWeapon.SetParent(context.BoneController.GetBoneRefByHandler(CurrentAIWeaponData.activeHandler).transform);
        }
    }
}