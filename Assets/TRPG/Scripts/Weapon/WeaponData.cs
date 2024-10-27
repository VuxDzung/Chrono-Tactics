using System;
using UnityEngine;

namespace TRPG
{
    public enum WeaponType
    {
        None = 0,
        Sword = 1,
        Pistol = 2,
        Rifle = 3,
    }

    public enum Handler
    {
        RightHand,
        LeftHand,
        Back,
        Belt,
    }

    [Serializable]
    public class WeaponData 
    {
        public string id;
        public string weaponName;
        public WeaponType weaponType;
        public float baseDamage = 10f;
        [Range(1f, 100f)]
        public float baseAccuracy = 80;
        public float range = 100f;
        public BaseWeapon weaponPrefab;
        public Handler activeHandler;
        public Handler inactiveHandler;
    }
}