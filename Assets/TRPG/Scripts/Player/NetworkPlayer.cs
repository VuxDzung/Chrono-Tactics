using FishNet.Object.Synchronizing;
using UnityEngine;
using TRPG.Unit;
using FishNet.Object;
using DevOpsGuy.GUI;
using FishNet.Connection;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TRPG
{
    public class NetworkPlayer : CoreNetworkBehaviour
    {
        private const string UNIT_NAME_FORMAT = "Unit [Owner:{0}|ID:[{1}]]";

        [SerializeField] private CommandInputManager commandInput;
        [SerializeField] private List<UnitController> unitPrefabList;

        private const int DEFAULT_ACTION_POINT = 2;

        private readonly SyncVar<bool> isOwnerTurn = new SyncVar<bool>();
        private readonly SyncDictionary<UnitController, int> unitDictionary = new SyncDictionary<UnitController, int>();
        private readonly SyncVar<UnitController> selectedUnit = new SyncVar<UnitController>();
        private readonly SyncVar<int> currentUnitIndex = new SyncVar<int>();

        private HUD hud;

        public bool IsOwnerTurn => isOwnerTurn.Value;
        public List<UnitController> ActiveUnitList => unitDictionary.Keys.ToList();

        public Action OnPlayerLose;
        public Action OnPlayerWin;
        public Action OnPlayerLoseCallback;
        public Action OnPlayerWinCallback;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                hud = UIManager.GetUI<HUD>();

                HUD.OnNextUnit += ChangeNextUnit;
                HUD.OnPrevUnit += ChangePrevUnit;
                HUD.OnEndTurn += EndTurn;

                MessageBoxTimer.OnShow += hud.HideUIAbilities;
                MessageBoxTimer.OnHide += hud.ShowUIAbilities;

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

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (IsOwner)
            {
                MessageBoxTimer.OnShow -= hud.HideUIAbilities;
                MessageBoxTimer.OnHide -= hud.ShowUIAbilities;
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
                ChangeFireTargetInput();
                MoveCameraInput();
            }
        }

        [Server]
        public virtual void Initialized(SpawnArea spawnArea, NetworkConnection owner)
        {
            unitPrefabList.ForEach(unitPrefab => {
                UnitController unit = Instantiate(unitPrefab, spawnArea.GetPoint().transform.position, spawnArea.GetPoint().rotation);
                unit.gameObject.name = string.Format(UNIT_NAME_FORMAT, OwnerId, unitDictionary.Count);
                ServerManager.Spawn(unit.gameObject, owner);

                RegisterUnit(unit);

                spawnArea.IncreasePointIndex();
            });
            currentUnitIndex.Value = 0;
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
                Physics.Raycast(GetRay(), out RaycastHit hit, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));
                if (hit.collider)
                {
                    OnSelectUnit(hit);
                }
            }
        }

        protected virtual void MovePlayerUnitInput()
        {
            if (commandInput.RightMouseDown)
            {
                Physics.Raycast(GetRay(), out RaycastHit hit, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Ground));
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

        protected virtual void MoveCameraInput()
        {
            if (commandInput.MoveCameraInput.magnitude > 0.1f)
                SceneCamera.Singleton.Move(commandInput.MoveCameraInput.x, commandInput.MoveCameraInput.y);
        }

        protected virtual void ChangeFireTargetInput()
        {
            if (commandInput.Tab)
            {
                OnChangeFireTarget();
            }
        }

        [ServerRpc]
        protected virtual void ChangeNextUnit()
        {
            if (selectedUnit.Value != null)
            {
                int index = unitDictionary.Keys.ToList().IndexOf(selectedUnit.Value);
                currentUnitIndex.Value = index;
            }

            currentUnitIndex.Value++;
            if (currentUnitIndex.Value >= unitDictionary.Count)
                currentUnitIndex.Value = 0;

            UnitController _selectedUnit = unitDictionary.Keys.ToList()[currentUnitIndex.Value];
            AssignSelectedUnit(_selectedUnit);
        }

        [ServerRpc]
        protected virtual void ChangePrevUnit()
        {
            if (selectedUnit.Value != null)
            {
                int index = unitDictionary.Keys.ToList().IndexOf(selectedUnit.Value);
                currentUnitIndex.Value = index;
            }

            currentUnitIndex.Value--;
            if (currentUnitIndex.Value < 0)
                currentUnitIndex.Value = unitDictionary.Count - 1;

            UnitController _selectedUnit = unitDictionary.Keys.ToList()[currentUnitIndex.Value];
            AssignSelectedUnit(_selectedUnit);
        }

        [ServerRpc]
        private void OnChangeFireTarget()
        {
            if (selectedUnit.Value != null && selectedUnit.Value.AbilityController.CurrentAbility == AbilityType.Shoot)
            {
                selectedUnit.Value.CombatBrain.ChangeToNextTarget();
            }
        }

        [ServerRpc]
        protected virtual void OnMovePlayerUnit(Vector3 destination)
        {
            if (selectedUnit.Value.TryMovePlayerUnit(destination))
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
                        AssignSelectedUnitRpc(null);
                    }
                    else
                    {
                        AssignSelectedUnitRpc(selectedUnit);
                    }
                }
            }
            else
            {
                //Show warning popup.
            }
        }

        [ServerRpc]
        protected virtual void AssignSelectedUnitRpc(UnitController unit)
        {
            AssignSelectedUnit(unit);
        }

        [Server]
        protected virtual void AssignSelectedUnit(UnitController unit)
        {
            if (selectedUnit.Value != null)
                selectedUnit.Value.Deselect(true);

            selectedUnit.Value = unit;
            if (selectedUnit.Value != null)
                selectedUnit.Value.Select(true);
        }

        [Server]
        private void ResetUnitActionPoint()
        {
            unitDictionary.Keys.ToList().ForEach(key => unitDictionary[key] = DEFAULT_ACTION_POINT);
        }

        [Server]
        private void ResetUnitAbility()
        {
            unitDictionary.Keys.ToList().ForEach(key => {
                key.AbilityController.ResetDefaultAbility();
                key.CombatBrain.ResetOverwatch();
            });
        }

        [Server]
        public virtual void StartOwnerTurn()
        {
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
            unitDictionary.Keys.ToList().ForEach(key => key.Deselect(true));
            OnStopTurnCallback();
        }

        [ServerRpc]
        protected virtual void EndTurn()
        {
            TRPGGameManager.Instance.ChangeNextPlayerTurn();
        }

        [ObserversRpc]
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

        public static Ray GetRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return ray;
        }

        public static Vector3 GetMouseWorldPosition(MaskCategory maskCategory)
        {
            Physics.Raycast(GetRay(), out RaycastHit hit, SceneLayerMasks.GetLayerMaskByCategory(maskCategory));

            return hit.transform ? hit.point : Vector3.zero;
        }
    }
}