using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using TRPG.Unit;
using UnityEngine;
namespace TRPG
{
    public enum ActionPointCost
    {
        Half,
        Full
    }

    public enum AbilityType
    {
        None,
        Shoot,
        Overwatch,
        Grenade,
        Move,
        MeleeAttack
    }

    public enum AbilityStep
    {
        Select,
        Deploy
    }

    [Serializable]
    public struct TimerConfig
    {
        public bool useTimer;
        public float delay;
        public float duration;
        public AnimationClip clip;
    }

    [CreateAssetMenu(fileName = "AbilityData", menuName = "TRPG/Ability/Data")]
    public class Ability : ScriptableObject
    {
        [SerializeField] private AbilityType type;
        [SerializeField] private ActionPointCost cost;
        [SerializeField] private Sprite thumbnail;
        [SerializeField] private AbilityStep[] steps;
        [SerializeField] private TimerConfig timerConfig;

        public AbilityType Type => type;
        public ActionPointCost Cost => cost;
        public TimerConfig TimerConfig => timerConfig;
        public AbilityStep[] Steps => steps;

        public Sprite Thumbnail => thumbnail;
        public static int FromActionCostToInt(ActionPointCost cost)
        {
            if (cost == ActionPointCost.Half) return 1;

            else return 2;
        }
    }
}