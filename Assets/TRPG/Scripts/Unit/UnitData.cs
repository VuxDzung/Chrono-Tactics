using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu]
    public class UnitData : ScriptableObject
    {
        public string id;
        public string unitName;
        public Sprite thumbnail;
        public float defaultHealth;
    }
}