using DevOpsGuy.GUI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TRPG.Unit
{
    public class AbilitiesController : CoreNetworkBehaviour
    {
        public Action<AbilityType> OnSelectAbilityServer;
        public Action<AbilityType> OnDeselectAbilityServer;
        public Action<AbilityType> OnActivateAbilityServer;

        public Action<AbilityType, bool> OnSelectAbilityClient;
        public Action<AbilityType, bool> OnDeselectAbilityClient;
        public Action<AbilityType, bool> OnActivateAbilityClient;

        public Action<AbilityType, bool, bool> OnDelayStarted;
        public Action<AbilityType, bool, bool> OnDelayFinished;
        public Action<AbilityType, bool, bool> OnDurationStarted;
        public Action<AbilityType, bool, bool> OnDurationFinished;

        [SerializeField] protected List<AbilityBehaviour> abilityBehaviourList = new List<AbilityBehaviour>();

        private UnitController context;

        protected readonly SyncTimer delayTimer = new SyncTimer();
        protected readonly SyncTimer durationTimer = new SyncTimer();

        protected readonly SyncVar<AbilityType> currentAbility = new SyncVar<AbilityType>();
        protected readonly SyncVar<int> currentStepIndex = new SyncVar<int>();

        public bool HasActiveAbility => currentAbility.Value != AbilityType.None;
        public AbilityType CurrentAbility => currentAbility.Value;

        public List<AbilityBehaviour> AbilityList => abilityBehaviourList;

        //Server-Side fields [Cannot read on client]
        protected float _delay;
        protected float _duration;
        private void Awake()
        {
            delayTimer.OnChange += OnDelayChange;
            durationTimer.OnChange += OnDurationChange;
        }

        private void OnDestroy()
        {
            delayTimer.OnChange -= OnDelayChange;
            durationTimer.OnChange -= OnDurationChange;
        }

        public virtual void Setup(UnitController context)
        {
            this.context = context;

            abilityBehaviourList.ForEach(ability => {
                ability.Initialized(this, context);

                OnSelectAbilityServer += ability.OnSelectServer;
                OnActivateAbilityServer += ability.OnActivateServer;
                OnDeselectAbilityServer += ability.OnDeselectServer;

                OnSelectAbilityClient += ability.OnSelectCallback;
                OnDeselectAbilityClient += ability.OnDeselectCallback;
                OnActivateAbilityClient += ability.OnActivateCallback;

                OnDelayStarted += ability.OnDelayStarted;
                OnDelayFinished += ability.OnDelayFinished;
                OnDurationStarted += ability.OnDurationStart;
                OnDurationFinished += ability.OnDurationFinished;
            });
        }

        public override void Update()
        {
            base.Update();
            float deltaTime = Time.deltaTime;
            delayTimer.Update(deltaTime);
            durationTimer.Update(deltaTime);
        }

        public void LoadAbilityToUI(HUD hud)
        {
            AimHUD.OnFire += ConfirmAbility;
            AimHUD.OnCancel += CancelAbility;
            abilityBehaviourList.ForEach(a => hud.AssignUIAbility(a.Data.Type, a.Data.Thumbnail, SelectAbility));
        }

        [ServerRpc]
        public void CancelAbility()
        {
            OnDeselectAbilityServer?.Invoke(currentAbility.Value);
            ResetDefaultAbility();
            OnDeselectAbilityCallback();
        }

        [ServerRpc]
        public virtual void SelectAbility(AbilityType type)
        {
            if (!context.UnitOwner.Value.IsOwnerTurn) return;

            if (!context.IsSelected) return;

            if (!context.HasEnoughPoint || context.CC.IsMoving) return;

            Ability queryAbility = abilityBehaviourList.FirstOrDefault(a => a.Data.Type == type)?.Data;
            currentAbility.Value = type;
            Debug.Log($"{gameObject.name}.SelectAbility={currentAbility.Value}");
            ExecuteCurrentStep(context, currentAbility.Value);
        }

        [ServerRpc]
        public virtual void ConfirmAbility()
        {
            Debug.Log($"ConfirmAbility:{context.gameObject.name} | Ability: {currentAbility.Value}");

            ExecuteCurrentStep(context, currentAbility.Value);
        }

        [Server]
        private void ExecuteCurrentStep(UnitController context, AbilityType abilityType)
        {
            Ability queryAbility = abilityBehaviourList.FirstOrDefault(a => a.Data.Type == currentAbility.Value)?.Data;
           
            if (queryAbility == null)
            {
                Debug.LogError($"Query Ability {abilityType} is null (Unit: {context.gameObject.name})");
                return;
            }

            if (currentStepIndex.Value >= queryAbility.Steps.Length)
            {
                Debug.LogWarning("Invalid skill step index.");
                return;
            }

            if (!context.UnitOwner.Value.HasEnoughPoint(context))
            {
                Debug.Log("<color=red>Invalid:</color>Not enough action points!");
                return;
            }

            var actionType = queryAbility.Steps[currentStepIndex.Value];

            switch (actionType)
            {
                case AbilityStep.Select:
                    Debug.Log($"ExecuteCurrentStep.Select: {abilityType}");
                    OnSelectAbilityServer?.Invoke(abilityType);
                    OnSelectAbilityCallback(abilityType);
                    break;
                case AbilityStep.Deploy:
                    Debug.Log($"ExecuteCurrentStep.Deploy: {abilityType}");
                    TryStartAbility(queryAbility);
                    currentStepIndex.Value = 0;
                    break;
            }
            currentStepIndex.Value++;
        }

        [Server]
        public virtual void ResetDefaultAbility()
        {
            currentAbility.Value = AbilityType.None;
            currentStepIndex.Value = 0;
        }

        [Server]
        public virtual void TryStartAbility(Ability ability)
        {
            OnActivateAbilityServer?.Invoke(currentAbility.Value);
            OnActivateAbilityCallback();
            context.UnitOwner.Value.SpendActionPoint(context, ability.Cost);

            if (ability.TimerConfig.useTimer)
            {
                SetTimer(ability.TimerConfig.delay, ability.TimerConfig.clip != null ? ability.TimerConfig.clip.length : ability.TimerConfig.duration);
                delayTimer.StartTimer(_delay);
            }
        }

        [Server]
        protected virtual void SetTimer(float delay, float duration)
        {
            _delay = delay;
            _duration = duration * context.WeaponManager.CurrentWeaponData.strikeCount; //The animation shall represent the strike count amount!
        }

        #region Client Rpc [Callback]
        [ObserversRpc]
        protected void OnSelectAbilityCallback(AbilityType ability)
        {
            Debug.Log($"OnSelectAbilityCallback.Ability: {ability}");
            OnSelectAbilityClient?.Invoke(ability, IsOwner);
        }

        [ObserversRpc]
        protected void OnDeselectAbilityCallback()
        {
            OnDeselectAbilityClient?.Invoke(currentAbility.Value, IsOwner);
        }

        [ObserversRpc]
        protected void OnActivateAbilityCallback()
        {
            if (IsOwner)
            {
                context.Hud.HideUIAbilities();
            }

            OnActivateAbilityClient?.Invoke(currentAbility.Value, IsOwner);
        }
        #endregion

        #region Timer 
        public virtual void OnDelayChange(SyncTimerOperation op, float prev, float next, bool asServer)
        {
            if (op == SyncTimerOperation.Start)
                OnDelayTimerStarted(asServer);
            else if (op == SyncTimerOperation.Finished)
                OnDelayTimerFinished(asServer);
        }

        public virtual void OnDurationChange(SyncTimerOperation op, float prev, float next, bool asServer)
        {
            if (op == SyncTimerOperation.Start)
                OnDurationTimerStarted(asServer);
            else if (op == SyncTimerOperation.Finished)
                OnDurationTimerFinished(asServer);
        }

        public virtual void OnDelayTimerStarted(bool asServer)
        {
            OnDelayStarted?.Invoke(currentAbility.Value, asServer, IsOwner);
        }

        public virtual void OnDelayTimerFinished(bool asServer)
        {
            if (asServer)
                durationTimer.StartTimer(_duration);
            OnDelayFinished?.Invoke(currentAbility.Value, asServer, IsOwner);
        }

        public virtual void OnDurationTimerStarted(bool asServer)
        {
            OnDurationStarted?.Invoke(currentAbility.Value, asServer, IsOwner);
        }

        public virtual void OnDurationTimerFinished(bool asServer)
        {
            if (asServer)
            {
                //After finish the skill, reset all the flag variables back to default [Dung].
                currentAbility.Value = AbilityType.None;
            }
            else
            {
                if (IsOwner)
                {
                    context.Hud.ShowUIAbilities();
                    context.EnableCellsAroundUnit();
                    context.DisableTPCamera();
                }
            }
            OnDurationFinished?.Invoke(currentAbility.Value, asServer, IsOwner);
        }
        #endregion
    }
}