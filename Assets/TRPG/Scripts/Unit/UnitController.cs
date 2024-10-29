using Cinemachine;
using DevOpsGuy.GUI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public readonly SyncVar<NetworkPlayer> UnitOwner = new SyncVar<NetworkPlayer>();

        private HUD hud;

        public bool IsSelected => isSelected.Value;
        public UnitData Data => data;

        public HealthController Health {  get; private set; }
        public BoneSnapController BoneController { get; private set; }  
        public UnitMotor Motor { get; private set; }
        public UnitAnimationController AnimationController { get; private set; }
        public AbilitiesController AbilityController { get; private set; }
        
        public UnitCombatBrain CombatBrain { get; private set; }
        public WeaponManager WeaponManager { get; private set; }

        public bool HasEnoughPoint => UnitOwner.Value.HasEnoughPoint(this);

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Motor = GetComponent<UnitMotor>();
            AnimationController = GetComponent<UnitAnimationController>();
            AbilityController = GetComponent<AbilitiesController>();
            CombatBrain = GetComponent<UnitCombatBrain>();
            BoneController = GetComponent<BoneSnapController>();
            WeaponManager = GetComponent<WeaponManager>();
            Health = GetComponent<HealthController>();

            if (Motor) Motor.Setup(this);
            if (AnimationController) AnimationController.Setup(this);
            if (AbilityController) AbilityController.Setup(this);
            if (CombatBrain) CombatBrain.Setup(this);
            if (WeaponManager) WeaponManager.Setup(this);

            OnSelectCallback += SelectOwnerCallback;
            OnDeselectCallback += DeselectOwnerCallback;

            if (Motor) Motor.OnMoveStartedCallback += StartMove;
            if (Motor) Motor.OnMoveFinishedCallback += OnReachedDestination;

            Health.OnDeadCallback += AnimationController.DeadAnimation;
            Health.OnDead += OnDead;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                hud = UIManager.GetUI<HUD>();
                tpCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                tpCamera.enabled = false;
                AssignUnitOwnerRef();
            }
        }

        [ServerRpc]
        private void AssignUnitOwnerRef()
        {
            UnitOwner.Value = TRPGGameManager.Instance.GetPlayer(Owner);
            Debug.Log($"UnitOwner:ID={UnitOwner.Value.OwnerId} | Name={UnitOwner.Value.gameObject.name}");
        }

        [Server]
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
                AbilityController.LoadAbilityToUI(hud);
                WeaponManager.LoadWeaponUI();
                if (HasEnoughPoint)
                    EnableCellsAroundUnit();
            }
        }

        [Client]
        private void DeselectOwnerCallback()
        {
            if (IsOwner)
            {
                hud.ClearUIAbilities();
                selectObj.SetActive(false);
                GridManager.Singleton.DisableAllCells();
            }
        }

        [Server]
        public void OnDead()
        {
            UnitOwner.Value.Unregister(this);
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
                SceneCamera.Singleton.MoveTo(transform.position, Quaternion.identity);
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

                if (GridManager.Singleton.IsValidCell(roundedDestination))
                {
                    if (Vector3Int.Distance(roundedPos, roundedDestination) <= data.fov)
                    {
                        Motor.MoveTo(roundedDestination);
                        return true;
                    }
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