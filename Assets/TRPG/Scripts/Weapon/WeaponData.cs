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
        Grenade = 4,
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
        public Sprite thumbnail;
        /// <summary>
        /// This represent how many times this weapon perform a strike/shoot.
        /// </summary>
        public int strikeCount = 1;
        public float delayBetweenStrike;
        public float baseDamage = 10f;
        [Range(1f, 100f)]
        public float baseAccuracy = 80;
        public float range = 100f;
        public BaseWeapon weaponPrefab;
        public Handler activeHandler;
        public Handler inactiveHandler;

        public GrenadeSettings grenadeSettings;
    }

    [Serializable]
    public class GrenadeSettings
    {
        public float blastRadius;
        public float decreasePerWorldUnit;
    }
}