using Cinemachine;
using DevOpsGuy.GUI;
using FischlWorks_FogWar;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TRPG.Unit
{
    public class UnitController : CoreNetworkBehaviour
    {
        public Action OnSelectCallback;
        public Action OnDeselectCallback;

        [SerializeField] private UnitData data;
        [SerializeField] private GameObject selectObj;
        [SerializeField] private CinemachineVirtualCamera tpCamera;

        private readonly SyncVar<bool> isSelected = new SyncVar<bool>();
        public readonly SyncVar<NetworkPlayer> UnitOwner = new SyncVar<NetworkPlayer>();

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

        public bool IsOwnerPlayer => Owner != null && IsOwner;

        public HUD Hud { get; private set; }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            Initialized();
        }

        public virtual void Initialized()
        {
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

            Health.OnDead += OnDead;
            Health.OnDeadCallback += AnimationController.DeadAnimation;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwnerPlayer)
            {
                Hud = UIManager.GetUI<HUD>();
                tpCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                tpCamera.enabled = false;
                AssignUnitOwnerRef();
            }
        }

        [ServerRpc]
        private void AssignUnitOwnerRef()
        {
            UnitOwner.Value = TRPGGameManager.Instance.GetPlayer(Owner);
        }

        [Server]
        public void Select(bool callback)
        {
            isSelected.Value = true;

            if (callback) SelectCallback();
        }

        [ObserversRpc]
        private void SelectCallback()
        {
            OnSelectCallback();
        }

        [Server]
        public void Deselect(bool callback)
        {
            isSelected.Value = false;
            if (callback) DeselectCallback();
        }

        [ObserversRpc]
        private void DeselectCallback()
        {
            OnDeselectCallback();    
        }

        [Client]
        private void SelectOwnerCallback()
        {
            if (IsOwnerPlayer)
            {
                selectObj.SetActive(true);
                Hud.ClearUIAbilities();
                AimHUD.OnFire = null;
                AimHUD.OnCancel = null;

                AbilityController.LoadAbilityToUI(Hud);
                WeaponManager.LoadWeaponUI();

                if (HasEnoughPoint)
                    EnableCellsAroundUnit();
            }
        }

        [Client]
        private void DeselectOwnerCallback()
        {
            if (IsOwnerPlayer)
            {
                Hud.ClearUIAbilities();
                selectObj.SetActive(false);
                GridManager.Singleton.DisableAllCells();
            }
        }

        [Server]
        public virtual void OnDead()
        {
            if (UnitOwner.Value != null) UnitOwner.Value.Unregister(this);
        }

        #region Locomotion 
        [Client]
        private void StartMove(bool isOwner)
        {
            if (IsOwnerPlayer)
            {
                GridManager.Singleton.DisableAllCells();
                //EnableTPCamera();
                Hud.HideUIAbilities();
            }
        }

        [Client]
        private void OnReachedDestination(bool isOwner)
        {
            if (IsOwnerPlayer)
            {
                //DisableTPCamera();
                SceneCamera.Singleton.MoveTo(transform.position, Quaternion.identity);
                Hud.ShowUIAbilities();
                if (HasEnoughPoint)
                    EnableCellsAroundUnit();
            }
        }

        protected virtual bool TryMove(Vector3 destination)
        {
            Vector3Int roundedDestination = MathUtil.RoundVector3(destination, 1);
            Vector3Int roundedPos = MathUtil.RoundVector3(transform.position, 1);

            if (GridManager.IsValidCell(roundedDestination))
            {
                if (Vector3Int.Distance(roundedPos, roundedDestination) <= data.viewRadius)
                {
                    Motor.MoveTo(roundedDestination);
                    return true;
                }
                else
                {
                    Debug.LogWarning($"{destination} is beyond the move range");
                }
            }
            return false;
        }

        [Server]
        public virtual bool TryMovePlayerUnit(Vector3 destination)
        {
            if (!HasEnoughPoint) return false;

            return TryMove(destination);
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
            if (!HasEnoughPoint)
            {

            }
            else
            {
                GridManager.Singleton.EnableSurroundingCells(transform.position, data.viewRadius);
            }
        }
        #endregion
    }
}