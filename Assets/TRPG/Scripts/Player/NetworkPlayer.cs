using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRPG.Unit;
using FishNet.Object;
using DevOpsGuy.GUI;
using Unity.Burst.CompilerServices;
using FishNet.Connection;
using System.Linq;

namespace TRPG
{
    public enum ActionPointCost
    {
        Half,
        Full
    }

    public class NetworkPlayer : CoreNetworkBehaviour
    {
        private const string UNIT_NAME_FORMAT = "Unit [Owner:{0} | Index:[{1}]]";

        [SerializeField] private CommandInputManager commandInput;
        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private UnitController testUnitPrefab;

        private const int DEFAULT_ACTION_POINT = 2;

        private readonly SyncVar<bool> isOwnerTurn = new SyncVar<bool>();
        private readonly SyncDictionary<UnitController, int> unitDictionary = new SyncDictionary<UnitController, int>();
        private readonly SyncVar<UnitController> selectedUnit = new SyncVar<UnitController>();
        private readonly SyncVar<int> currentUnitIndex = new SyncVar<int>();

        private HUD hud;

        public bool IsOwnerTurn => isOwnerTurn.Value;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                hud = UIManager.GetUI<HUD>();

                HUD.OnNextUnit += ChangeNextUnit;
                HUD.OnPrevUnit += ChangePrevUnit;
                HUD.OnEndTurn += EndTurn;

                switch (OwnerId)
                {
                    case 0:
                        SceneCamera.Singleton.MoveTo(SceneContextManager.S.Player1CamPos.position, Quaternion.identity);
                        break;
                    case 1:
                        SceneCamera.Singleton.MoveTo(SceneContextManager.S.Player2CamPos.position, SceneCamera.Singleton.GetRotationHandler(180));
                        break;
                }
            }
        }

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            if (IsOwner)
            {
                SelectUnitInput();
                MovePlayerUnitInput();
                RotateCameraInput();
            }
        }

        [Server]
        public virtual void Initialized(SpawnArea spawnArea, NetworkConnection owner)
        {
            UnitController unit = Instantiate(testUnitPrefab, spawnArea.GetPoint().transform.position, spawnArea.GetPoint().rotation);
            unit.gameObject.name = string.Format(UNIT_NAME_FORMAT, OwnerId, unitDictionary.Count);
            ServerManager.Spawn(unit.gameObject, owner);

            RegisterUnit(unit);

            spawnArea.IncreasePointIndex();

            Debug.Log($"{gameObject.name}.Owner={owner.ClientId}");
        }

        [Server]
        public bool RegisterUnit(UnitController unit)
        {
            if (unitDictionary.ContainsKey(unit))
                return false;
            unitDictionary.Add(unit, DEFAULT_ACTION_POINT);
            return true;
        }

        [Server]
        public bool Unregister(UnitController unit)
        {
            return unitDictionary.Remove(unit);
        }

        protected virtual void SelectUnitInput()
        {
            if (commandInput.LeftMouseDown)
            {
                Physics.Raycast(GetRay(), out RaycastHit hit, unitLayer);
                if (hit.collider)
                {
                    OnSelectUnit(hit);
                }
            }
        }

        [ServerRpc]
        protected virtual void EndTurn()
        {
            TRPGGameManager.Instance.ChangeNextPlayerTurn();
        }

        protected virtual void MovePlayerUnitInput()
        {
            if (commandInput.RightMouseDown)
            {
                Physics.Raycast(GetRay(), out RaycastHit hit, groundLayer);
                if (hit.transform && selectedUnit.Value != null)
                {
                    OnMovePlayerUnit(hit.point);
                }
            }
        }

        protected virtual void RotateCameraInput()
        {
            if (commandInput.E)
            {
                SceneCamera.Singleton.Rotate(90, true);
            }

            if (commandInput.Q)
            {
                SceneCamera.Singleton.Rotate(90, false);
            }
        }

        protected virtual void ChangeFireTargetInput()
        {
            if (commandInput.Tab)
            {

            }
        }

        protected virtual void ChangeNextUnit()
        {
            currentUnitIndex.Value++;
            if (currentUnitIndex.Value >= unitDictionary.Count)
                currentUnitIndex.Value = 0;
        }

        protected virtual void ChangePrevUnit()
        {
            currentUnitIndex.Value--;
            if (currentUnitIndex.Value < 0)
                currentUnitIndex.Value = unitDictionary.Count - 1;
        }

        [ServerRpc]
        private void OnChangeFireTarget()
        {
            if (selectedUnit.Value != null)
            {
                selectedUnit.Value.CombatBrain.ChangeToNextTarget();
            }
        }

        [ServerRpc]
        protected virtual void OnMovePlayerUnit(Vector3 destination)
        {
            if (selectedUnit.Value.TryMove(destination))
                SpendActionPoint(selectedUnit.Value, ActionPointCost.Half);
        }

        /// <summary>
        /// When select an unit, the unit visual select shall show up along with the unit's UI.
        /// </summary>
        /// <param name="hit"></param>
        [Client]
        protected virtual void OnSelectUnit(RaycastHit hit)
        {
            UnitController selectedUnit = hit.transform.GetComponent<UnitController>();

            if (selectedUnit != null)
            {
                if (unitDictionary.ContainsKey(selectedUnit))
                {
                    if (selectedUnit.IsSelected)
                    {
                        AssignSelectedUnit(null);
                    }
                    else
                    {
                        AssignSelectedUnit(selectedUnit);
                    }
                }
            }
            else
            {
                //Show warning popup.
            }
        }

        [ServerRpc]
        protected virtual void AssignSelectedUnit(UnitController unit)
        {
            if (selectedUnit.Value != null)
                selectedUnit.Value.Deselect();

            selectedUnit.Value = unit;

            if (selectedUnit.Value != null)
                selectedUnit.Value.Select();
        }

        [Server]
        private void ResetUnitActionPoint()
        {
            unitDictionary.Keys.ToList().ForEach(key => unitDictionary[key] = 2);
        }

        [Server]
        private void ResetUnitAbility()
        {
            unitDictionary.Keys.ToList().ForEach(key => key.AbilityController.ResetDefaultAbility());
        }

        [Server]
        public virtual void StartOwnerTurn()
        {
            Debug.Log($"{gameObject.name}.StartTurn");
            isOwnerTurn.Value = true;
            commandInput.LockInput.Value = false;
            ResetUnitActionPoint();
            ResetUnitAbility();
            StartTurnCallback();
        }

        [Server]
        public virtual void StopOwnerTurn()
        {
            isOwnerTurn.Value = false;
            commandInput.LockInput.Value = true;
            unitDictionary.Keys.ToList().ForEach(key => key.Deselect());
            OnStopTurnCallback();
        }

        public virtual void StartTurnCallback()
        {
            if (IsOwner)
            {
                
            }
        }

        [ObserversRpc]
        public virtual void OnStopTurnCallback()
        {
            if (IsOwner)
            {
                
            }
        }

        #region Action Points

        [Server]
        public void SpendActionPoint(UnitController unit, ActionPointCost cost)
        {
            if (unitDictionary.ContainsKey(unit))
            {
                int finalPoint = unitDictionary[unit];
                switch (cost)
                {
                    case ActionPointCost.Half:
                        finalPoint -= 1;
                        break;
                    case ActionPointCost.Full:
                        finalPoint -= 2;
                        break;
                }
                if (finalPoint < 0)
                    finalPoint = 0;

                unitDictionary[unit] = finalPoint;
            }
        }

        public bool HasEnoughPoint(UnitController unit)
        {
            if (!unitDictionary.ContainsKey(unit))
                return false;
            return unitDictionary[unit] > 0;
        }
        #endregion

        public Ray GetRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return ray;
        }
    }
}