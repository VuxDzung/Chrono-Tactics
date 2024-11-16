using System.Collections;
using System.Collections.Generic;
using TRPG;
using TRPG.Unit;
using UnityEngine;

namespace TRPG
{
    public class AbilityBehaviour : CoreNetworkBehaviour
    {
        [SerializeField] private Ability data;

        public Ability Data => data;

        protected AbilitiesController controller;
        protected UnitController context;

        public virtual void Initialized(AbilitiesController controller, UnitController context)
        {
            this.controller = controller;
            this.context = context;
        }


        // Unified type check for all actions
        protected bool IsAbilityType(AbilityType targetType)
        {
            return data.Type == targetType;
        }

        #region Server-Side
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
        #endregion

        #region Callback
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
        #endregion

        #region Timer
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
        #endregion
    }
}