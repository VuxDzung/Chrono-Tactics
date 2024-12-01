using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu]
    public class UnitData : ScriptableObject
    {
        public string id;
        public float defaultHealth;
        public float evasion;
        public int viewRadius;
    }

    [Serializable]
    public class UnitProfile
    {
        public string id;
        public string unitName;
        public Sprite thumbnail;
        public GameObject model;
        public UnitController prefab;
    }
}