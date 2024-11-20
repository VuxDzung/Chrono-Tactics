using FishNet.Object;
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
        public void OnSelectServer(AbilityType type)
        {
            if (IsAbilityType(type)) OnSelectServer();
        }

        public void OnDeselectServer(AbilityType type)
        {
            if (IsAbilityType(type)) OnDeselectServer();
        }

        public void OnActivateServer(AbilityType type)
        {
            if (IsAbilityType(type)) OnActivateServer();
        }

        protected virtual void OnSelectServer() { }
        protected virtual void OnDeselectServer() { }
        protected virtual void OnActivateServer() { }

        protected virtual void OnDelayStartedServer() { }
        protected virtual void OnDelayFinishedServer() { }
        protected virtual void OnDurationStartedServer() { }
        protected virtual void OnDurationFinishedServer() { }
        #endregion

        #region Callback
        public void OnSelectCallback(AbilityType type, bool isOwner)
        {
            Debug.Log($"{context.gameObject.name} | Ability: {type}");
            if (IsAbilityType(type)) OnSelectCallback(isOwner);
        }
        public void OnDeselectCallback(AbilityType type, bool isOwner)
        {
            if (IsAbilityType(type)) OnDeselectCallback(isOwner);
        }
        public void OnActivateCallback(AbilityType type, bool isOwner)
        {
            if (IsAbilityType(type)) OnActivateCallback(isOwner);
        }

        protected virtual void OnSelectCallback(bool isOwner) { }
        protected virtual void OnDeselectCallback(bool isOwner) { } 
        protected virtual void OnActivateCallback(bool isOwner) { }

        [ObserversRpc]
        protected virtual void OnDelayStartedCallback() { }
        [ObserversRpc]
        protected virtual void OnDelayFinishedCallback() { }
        [ObserversRpc]
        protected virtual void OnDurationFinishedCallback() { }
        [ObserversRpc]
        protected virtual void OnDurationStartedCallback() { }
        #endregion

        #region Timer
        public void OnDelayStarted(AbilityType type, bool asServer)
        {
            if (IsAbilityType(type))
            {
                if (asServer)
                {
                    OnDelayStartedServer();
                    OnDelayStartedCallback();
                }
            }
        }
        public void OnDelayFinished(AbilityType type, bool asServer)
        {
            if (IsAbilityType(type)) 
            {
                if (asServer)
                {
                    OnDelayFinishedServer();
                    OnDelayFinishedCallback();
                }
            }
        }
        public void OnDurationStart(AbilityType type, bool asServer)
        {
            if (IsAbilityType(type))
            {
                OnDurationStartedServer();
                OnDurationStartedCallback();
            }
        }
        public void OnDurationFinished(AbilityType type, bool asServer)
        {
            if (IsAbilityType(type))
            {
                if (asServer)
                {
                    OnDurationFinishedServer();
                    OnDurationFinishedCallback();
                }
            }
        }
        #endregion
    }
}