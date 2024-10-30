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
        public Action<AbilityType, UnitController> OnSelectAbilityServer;
        public Action<AbilityType, UnitController> OnDeselectAbilityServer;
        public Action<AbilityType, UnitController> OnActivateAbilityServer;

        public Action<AbilityType, UnitController, bool> OnSelectAbilityClient;
        public Action<AbilityType, UnitController, bool> OnDeselectAbilityClient;
        public Action<AbilityType, UnitController, bool> OnActivateAbilityClient;

        public Action<AbilityType, UnitController, bool> OnDelayStarted;
        public Action<AbilityType, UnitController, bool> OnDelayFinished;
        public Action<AbilityType, UnitController, bool> OnDurationStarted;
        public Action<AbilityType, UnitController, bool> OnDurationFinished;


        [SerializeField] private List<Ability> abilityList = new List<Ability>();

        private UnitController context;

        private readonly SyncTimer delayTimer = new SyncTimer();
        private readonly SyncTimer durationTimer = new SyncTimer();

        private readonly SyncVar<AbilityType> currentAbility = new SyncVar<AbilityType>();
        private readonly SyncVar<int> currentStepIndex = new SyncVar<int>();

        public bool HasActiveAbility => currentAbility.Value != AbilityType.None;
        public AbilityType CurrentAbility => currentAbility.Value;

        //Server-Side fields [Cannot read on client]
        private float _delay;
        private float _duration;

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

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                AimHUD.OnFire += ConfirmAbility;
                AimHUD.OnCancel += CancelAbility;
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (IsOwner)
            {
                AimHUD.OnFire -= ConfirmAbility;
                AimHUD.OnCancel -= CancelAbility;
            }
        }

        public virtual void Setup(UnitController context)
        {
            this.context = context;

            abilityList.ForEach(a => {
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
            OnDeselectAbilityServer?.Invoke(currentAbility.Value, context);
            ResetDefaultAbility();
            OnDeselectAbilityCallback();
        }

        [ServerRpc]
        public virtual void SelectAbility(AbilityType type)
        {
            if (!context.UnitOwner.Value.IsOwnerTurn) return;

            if (!context.IsSelected) return;

            if (!context.HasEnoughPoint || context.Motor.IsMoving) return;
            Debug.Log($"{gameObject.name}:{type.ToString()}");
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
            if (!context.UnitOwner.Value.IsOwnerTurn) return;
            Debug.Log($"UnitOwner: [{context.UnitOwner.Value.gameObject.name}]");

            Ability queryAbility = abilityList.FirstOrDefault(a => a.Type == currentAbility.Value);

            if (queryAbility == null || currentStepIndex.Value >= queryAbility.Steps.Length)
            {
                Debug.LogWarning("Invalid skill sequence or step index.");
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
                    Debug.Log("ExecuteCurrentStep:Select");
                    OnSelectAbilityServer?.Invoke(currentAbility.Value, context);
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
            OnActivateAbilityServer?.Invoke(currentAbility.Value, context);
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
        private void OnSelectAbilityCallback()
        {
            OnSelectAbilityClient?.Invoke(currentAbility.Value, context, IsOwner);
        }

        [ObserversRpc]
        private void OnDeselectAbilityCallback()
        {
            OnDeselectAbilityClient?.Invoke(currentAbility.Value, context, IsOwner);
        }

        [ObserversRpc]
        private void OnActivateAbilityCallback()
        {
            OnActivateAbilityClient?.Invoke(currentAbility.Value, context, IsOwner);
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
            OnDelayStarted?.Invoke(currentAbility.Value, context, asServer);
        }

        public virtual void OnDelayTimerFinished(bool asServer)
        {
            if (asServer)
                durationTimer.StartTimer(_duration);
            OnDelayFinished?.Invoke(currentAbility.Value, context, asServer);
        }

        public virtual void OnDurationTimerStarted(bool asServer)
        {
            OnDurationStarted?.Invoke(currentAbility.Value, context, asServer);
        }

        public virtual void OnDurationTimerFinished(bool asServer)
        {
            OnDurationFinished?.Invoke(currentAbility.Value, context, asServer);
            if (asServer)
                //After finish the skill, reset all the flag variables back to default.
                currentAbility.Value = AbilityType.None;
        }
        #endregion
    }
}