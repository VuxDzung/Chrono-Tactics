using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TRPG
{
    [CreateAssetMenu]
    public class WeaponConfigSO : ScriptableObject
    {
        [SerializeField] private List<WeaponData> dataList;

        public List<WeaponData> Data => dataList;

        public WeaponData GetDataBydId(string weaponId)
        {
            WeaponData weaponData = dataList.FirstOrDefault(data => data.id.Equals(weaponId));
            return weaponData;
        }
    }
}