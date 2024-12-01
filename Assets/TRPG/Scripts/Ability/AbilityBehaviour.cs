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

        [Server]
        protected virtual void OnSelectServer() { }
        [Server]
        protected virtual void OnDeselectServer() { }
        [Server]
        protected virtual void OnActivateServer() { }
        [Server]
        protected virtual void OnDelayStartedServer() { }
        [Server]
        protected virtual void OnDelayFinishedServer() { }
        [Server]
        protected virtual void OnDurationStartedServer() { }

        [Server]
        protected virtual void OnDurationFinishedServer() { }
        #endregion

        #region Callback
        public void OnSelectCallback(AbilityType type, bool isOwner)
        {
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

        protected virtual void OnDelayStartedClient(bool asOwner) { }
        protected virtual void OnDelayFinishedClient(bool asOwner) { }
        protected virtual void OnDurationStartedClient(bool asOwner) { }
        protected virtual void OnDurationFinishedClient(bool asOwner) { }

        #endregion

        #region Timer
        public void OnDelayStarted(AbilityType type, bool asServer, bool asOwner)
        {
            if (IsAbilityType(type))
            {
                if (asServer)
                {
                    OnDelayStartedServer();
                }
                else
                {
                    OnDelayStartedClient(asOwner);
                }
            }
        }
        public void OnDelayFinished(AbilityType type, bool asServer, bool asOwner)
        {
            if (IsAbilityType(type)) 
            {
                if (asServer)
                {
                    OnDelayFinishedServer();
                }
                else
                {
                    OnDelayFinishedClient(asOwner);
                }
            }
        }
        public void OnDurationStart(AbilityType type, bool asServer, bool asOwner)
        {
            if (IsAbilityType(type))
            {
                if (asServer)
                    OnDurationStartedServer();
                else
                    OnDurationStartedClient(asOwner);
            }
        }
        public void OnDurationFinished(AbilityType type, bool asServer, bool asOwner)
        {
            if (IsAbilityType(type))
            {
                if (asServer)
                {
                    OnDurationFinishedServer();
                }
                else
                {
                    OnDurationFinishedClient(asOwner);
                }
            }
        }
        #endregion
    }
}