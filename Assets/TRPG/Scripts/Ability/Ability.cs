using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using TRPG;
using TRPG.Unit;
using UnityEngine;
namespace TRPG
{
    public enum AbilityType
    {
        None,
        Shoot,
        Overwatch,
        Grenade
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
    }

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
        protected UnitController context;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
        }

        // Unified type check for all actions
        protected bool IsAbilityType(AbilityType targetType)
        {
            return type == targetType;
        }

        public virtual void OnSelectServer(AbilityType type)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnActivateServer(AbilityType type)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDeactivateServer(AbilityType type)
        {
            if (!IsAbilityType(type)) return;
        }

        public virtual void OnSelectCallback(AbilityType type, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDeselectCallback(AbilityType type, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnActivateCallback(AbilityType type, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }


        public virtual void OnDelayStarted(AbilityType type, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDelayFinished(AbilityType type, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDurationStart(AbilityType type, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDurationFinished(AbilityType type, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
    }
}