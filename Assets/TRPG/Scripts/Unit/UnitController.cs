using Cinemachine;
using DevOpsGuy.GUI;
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
        [SerializeField] private CinemachineVirtualCamera tpCamera;

        private readonly SyncVar<bool> isSelected = new SyncVar<bool>();

        private HUD hud;

        public bool IsSelected => isSelected.Value;

        public UnitData Data => data;

        public BoneSnapController BoneController { get; private set; }  
        public UnitMotor Motor { get; private set; }
        public UnitAnimationController AnimationController { get; private set; }
        public AbilitiesController AbilityController { get; private set; }
        public NetworkPlayer UnitOwner { get; private set; }
        public UnitCombatBrain CombatBrain { get; private set; }
        public WeaponManager WeaponManager { get; private set; }

        public bool HasEnoughPoint => UnitOwner.HasEnoughPoint(this);

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Motor = GetComponent<UnitMotor>();
            AnimationController = GetComponent<UnitAnimationController>();
            AbilityController = GetComponent<AbilitiesController>();
            CombatBrain = GetComponent<UnitCombatBrain>();
            BoneController = GetComponent<BoneSnapController>();
            WeaponManager = GetComponent<WeaponManager>();

            Motor.Setup(this);
            AnimationController.Setup(this);
            AbilityController.Setup(this);
            CombatBrain.Setup(this);
            WeaponManager.Setup(this);

            OnSelectCallback += SelectOwnerCallback;
            OnDeselectCallback += DeselectOwnerCallback;

            UnitOwner = TRPGGameManager.Instance.GetPlayer(Owner);

            Motor.OnMoveStartedCallback += StartMove;
            Motor.OnMoveFinishedCallback += OnReachedDestination;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                hud = UIManager.GetUI<HUD>();
                tpCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                tpCamera.enabled = false;
            }
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

        [Client]
        private void SelectOwnerCallback()
        {
            if (IsOwner)
            {
                selectObj.SetActive(true);
                if (HasEnoughPoint)
                    EnableCellsAroundUnit();
            }
        }

        [Client]
        private void DeselectOwnerCallback()
        {
            if (IsOwner)
            {
                selectObj.SetActive(false);
                GridManager.Singleton.DisableAllCells();
            }
        }

        #region Locomotion 
        [Client]
        private void StartMove(bool isOwner)
        {
            if (isOwner)
            {
                GridManager.Singleton.DisableAllCells(); 
                EnableTPCamera();
                hud.HideUIAbilities();
            }
        }

        [Client]
        private void OnReachedDestination(bool isOwner)
        {
            if (isOwner)
            {
                DisableTPCamera();
                hud.ShowUIAbilities();
                if (HasEnoughPoint)
                    EnableCellsAroundUnit();
            }
        }

        [Server]
        public bool TryMove(Vector3 destination)
        {
            if (HasEnoughPoint)
            {
                Vector3Int roundedDestination = MathUtil.RoundVector3(destination, 1);
                Vector3Int roundedPos = MathUtil.RoundVector3(transform.position, 1);
                if (Vector3Int.Distance(roundedPos, roundedDestination) <= data.fov)
                {
                    Motor.MoveTo(roundedDestination);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region TP Camera Controller 

        [Client]
        public virtual void EnableTPCamera()
        {
            tpCamera.enabled = true;
        }

        [Client]
        public virtual void DisableTPCamera()
        {
            tpCamera.enabled = false;
            SceneCamera.Singleton.ResetCameraTransform();
        }

        #endregion

        #region Unit Cells

        public virtual void EnableCellsAroundUnit()
        {
            GridManager.Singleton.EnableSurroundingCells(transform.position, data.fov);
        }

        #endregion
    }
}