using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [RequireComponent(typeof(UnitMotor))]
    [RequireComponent(typeof(UnitAnimationController))]
    public class UnitController : CoreNetworkBehaviour
    {
        public Action OnSelectCallback;
        public Action OnDeselectCallback;

        [SerializeField] private UnitData data;
        [SerializeField] private GameObject selectObj;

        private readonly SyncVar<bool> isSelected = new SyncVar<bool>();

        public bool IsSelected => isSelected.Value;

        public UnitData Data => data;
        public UnitMotor Motor { get; private set; }
        public UnitAnimationController AnimationController { get; private set; }
        public AbilitiesController AbilityController { get; private set; }
        public NetworkPlayer UnitOwner { get; private set; }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Motor = GetComponent<UnitMotor>();
            AnimationController = GetComponent<UnitAnimationController>();
            AbilityController = GetComponent<AbilitiesController>();

            Motor.Setup(this);
            AnimationController.Setup(this);
            AbilityController.Setup(this);

            OnSelectCallback += OnSelectOwner;
            OnDeselectCallback += OnDeselectOwner;

            UnitOwner = TRPGGameManager.Instance.GetPlayer(Owner);
        }

        [ServerRpc]
        public void Select()
        {
            isSelected.Value = true;
            SelectCallback();
        }

        [ObserversRpc]
        private void SelectCallback()
        {
            OnSelectCallback();
        }

        [Server]
        public void Deselect()
        {
            isSelected.Value = false;
            DeselectCallback();
        }

        [ObserversRpc]
        private void DeselectCallback()
        {
            OnDeselectCallback();
        }

        private void OnSelectOwner()
        {
            if (IsOwner)
            {
                selectObj.SetActive(true);
                GridManager.Singleton.EnableSurroundingCells(MathUtil.RoundVector2(new Vector2(transform.position.x, transform.position.z), 1), data.fov);
            }
        }

        private void OnDeselectOwner()
        {
            if (IsOwner)
            {
                selectObj.SetActive(false);
                GridManager.Singleton.DisableAllCells();
            }
        }
    }
}