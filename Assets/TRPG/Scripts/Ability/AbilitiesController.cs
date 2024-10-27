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

        public Action<AbilityType, bool> OnDelayStarted;
        public Action<AbilityType, bool> OnDelayFinished;
        public Action<AbilityType, bool> OnDurationStarted;
        public Action<AbilityType, bool> OnDurationFinished;


        [SerializeField] private List<Ability> abilityList = new List<Ability>();

        private UnitController context;

        private readonly SyncTimer delayTimer = new SyncTimer();
        private readonly SyncTimer durationTimer = new SyncTimer();

        private readonly SyncVar<AbilityType> currentAbility = new SyncVar<AbilityType>();
        private readonly SyncVar<int> currentStepIndex = new SyncVar<int>();

        public bool HasActiveAbility => currentAbility.Value != AbilityType.None;

        private float _delay;
        private float _duration;

        private void Awake()
        {
            delayTimer.OnChange += OnDelayChange;
            durationTimer.OnChange += OnDurationChange;

            AimHUD.OnFire += ConfirmAbility;
            AimHUD.OnCancel += CancelAbility;
        }

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            abilityList.ForEach(a => {
                a.Setup(context);

                OnSelectAbilityServer += a.OnSelectServer;
                OnActivateAbilityServer += a.OnActivateServer;
                OnDeselectAbilityServer += a.OnDeactivateServer;

                OnSelectAbilityClient += a.OnSelectCallback;
                OnDeselectAbilityClient += a.OnDeselectCallback;
                OnActivateAbilityClient += a.OnActivateCallback;

                OnDelayStarted += a.OnDelayStarted;
                OnDelayFinished += a.OnDelayFinished;
                OnDurationStarted += a.OnDurationStart;
                OnDurationFinished += a.OnDurationFinished;
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
            abilityList.ForEach(a => hud.AssignUIAbility(a.Type, a.Thumbnail, SelectAbility));
        }

        [ServerRpc]
        public void CancelAbility()
        {
            OnDeselectAbilityServer?.Invoke(currentAbility.Value);
            currentAbility.Value = AbilityType.None;
            ResetDefaultAbility();
            OnDeselectAbilityCallback();
        }

        [ServerRpc]
        public virtual void SelectAbility(AbilityType type)
        {
            if (!context.IsSelected) return;

            Ability queryAbility = abilityList.FirstOrDefault(a => a.Type == type);
            currentAbility.Value = type;
            Debug.Log($"{gameObject.name}.SelectAbility={type}");
            ExecuteCurrentStep();
        }

        [ServerRpc]
        public virtual void ConfirmAbility()
        {
            if (!context.IsSelected) return;

            ExecuteCurrentStep();
        }

        [Server]
        private void ExecuteCurrentStep()
        {
            Ability queryAbility = abilityList.FirstOrDefault(a => a.Type == currentAbility.Value);

            if (queryAbility == null || currentStepIndex.Value >= queryAbility.Steps.Length)
            {
                Debug.LogWarning("Invalid skill sequence or step index.");
                return;
            }

            if (!context.UnitOwner.HasEnoughPoint(context))
            {
                Debug.Log("<color=red>Invalid:</color>Not enough action points!");
                return;
            }

            var actionType = queryAbility.Steps[currentStepIndex.Value];

            switch (actionType)
            {
                case AbilityStep.Select:
                    Debug.Log("ExecuteCurrentStep:Select");
                    //Activate the target indicator.
                    //context.ActivateTargetIndicator();
                    OnSelectAbilityServer?.Invoke(currentAbility.Value);
                    OnSelectAbilityCallback();
                    break;
                case AbilityStep.Deploy:
                    Debug.Log("ExecuteCurrentStep:Deploy");
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
            Debug.Log($"{gameObject.name}.TryStartAbility.STATUS=STARTED");
            OnActivateAbilityServer?.Invoke(currentAbility.Value);
            OnActivateAbilityCallback();
            context.UnitOwner.SpendActionPoint(context, ability.Cost);

            if (ability.TimerConfig.useTimer)
            {
                SetTimer(ability.TimerConfig.delay, ability.TimerConfig.duration);
                delayTimer.StartTimer(_delay);
            }
        }

        [Server]
        protected virtual void SetTimer(float delay, float duration)
        {
            _delay = delay;
            _duration = duration;
        }

        #region Client Rpc [Callback]
        [ObserversRpc]
        private void OnSelectAbilityCallback()
        {
            OnSelectAbilityClient?.Invoke(currentAbility.Value, IsOwner);
        }

        [ObserversRpc]
        private void OnDeselectAbilityCallback()
        {
            OnDeselectAbilityClient?.Invoke(currentAbility.Value, IsOwner);
        }

        [ObserversRpc]
        private void OnActivateAbilityCallback()
        {
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
            OnDelayStarted?.Invoke(currentAbility.Value, asServer);
        }

        public virtual void OnDelayTimerFinished(bool asServer)
        {
            if (asServer)
                durationTimer.StartTimer(_duration);
            OnDelayFinished?.Invoke(currentAbility.Value, asServer);
        }

        public virtual void OnDurationTimerStarted(bool asServer)
        {
            OnDurationStarted?.Invoke(currentAbility.Value, asServer);
        }

        public virtual void OnDurationTimerFinished(bool asServer)
        {
            OnDurationFinished?.Invoke(currentAbility.Value, asServer);
            if (asServer)
                //After finish the skill, reset all the flag variables back to default.
                currentAbility.Value = AbilityType.None;
        }
        #endregion
    }
}