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
        public AnimationClip clip;
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

        // Unified type check for all actions
        protected bool IsAbilityType(AbilityType targetType)
        {
            return type == targetType;
        }

        #region Server-Side
        public virtual void OnSelectServer(AbilityType type, UnitController context)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnActivateServer(AbilityType type, UnitController context)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDeactivateServer(AbilityType type, UnitController context)
        {
            if (!IsAbilityType(type)) return;
        }
        #endregion

        #region Callback
        public virtual void OnSelectCallback(AbilityType type, UnitController context, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDeselectCallback(AbilityType type, UnitController context, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnActivateCallback(AbilityType type, UnitController context, bool isOwner)
        {
            if (!IsAbilityType(type)) return;
        }
        #endregion

        #region Timer
        public virtual void OnDelayStarted(AbilityType type, UnitController context, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDelayFinished(AbilityType type, UnitController context, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDurationStart(AbilityType type, UnitController context, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        public virtual void OnDurationFinished(AbilityType type, UnitController context, bool asServer)
        {
            if (!IsAbilityType(type)) return;
        }
        #endregion
    }
}