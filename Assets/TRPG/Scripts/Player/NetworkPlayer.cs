using FishNet.Object.Synchronizing;
using UnityEngine;
using TRPG.Unit;
using FishNet.Object;
using DevOpsGuy.GUI;
using FishNet.Connection;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace TRPG
{
    public class NetworkPlayer : CoreNetworkBehaviour
    {
        private const string UNIT_NAME_FORMAT = "Unit [Owner:{0}|ID:[{1}]]";

        [SerializeField] private UnitConfigList unitConfig;
        [SerializeField] private CommandInputManager commandInput;

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
        
        private void Awake()
        {
            unitDictionary.OnChange += _ActiveUnits_OnChange;
        }

        private void OnDestroy()
        {
            unitDictionary.OnChange -= _ActiveUnits_OnChange;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner)
            {
                hud = UIManager.GetUIStatic<HUD>();

                HUD.OnNextUnit += ChangeNextUnit_RPC_Server;
                HUD.OnPrevUnit += ChangePrevUnit_RPC_Server;
                HUD.OnEndTurn += EndTurn_RPC_Server;
                HUD.OnSurrender += Surrender;

                MessageBoxTimer.OnShow += hud.HideUIAbilities;
                MessageBoxTimer.OnHide += hud.ShowUIAbilities;

                switch (OwnerId)
                {
                    case 1:
                        SceneCamera.Singleton.MoveTo(SceneContextManager.S.Player1CamPos.position, Quaternion.identity);
                        break;
                    case 2:
                        SceneCamera.Singleton.MoveTo(SceneContextManager.S.Player2CamPos.position, Quaternion.LookRotation(-Vector3.forward));
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
        public virtual void Initialized(SpawnArea spawnArea, NetworkConnection owner, string[] playerUnitIDArr)
        {
            foreach (var unitId in playerUnitIDArr)
            {
                if (string.IsNullOrEmpty(unitId)) continue;

                UnitProfile profile = unitConfig.GetUnitProfileById(unitId);
                UnitController unit = Instantiate(profile.prefab, spawnArea.GetPoint().transform.position, spawnArea.GetPoint().rotation);
                unit.gameObject.name = string.Format(UNIT_NAME_FORMAT, OwnerId, unitDictionary.Count);
                ServerManager.Spawn(unit.gameObject, owner);

                RegisterUnit(unit);

                spawnArea.IncreasePointIndex();
            }

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
        public void Unregister(UnitController unit)
        {
            if (unitDictionary.Remove(unit))
            {
                if (AllUnitAreDead())
                {
                    OnDefeat();
                }
            }
        }

        public void Surrender()
        {
            // Show the Defeat UI.
            UIManager.ShowUIStatic<MessageBox>().SetMessage("DEFEATED", "Good luck next time!");

            Surrender_RPC_Server();
        }

        [ServerRpc]
        private void Surrender_RPC_Server()
        {
            OnDefeat();
        }

        [Server]
        public void OnDefeat()
        {
            NetworkPlayer competitor = NetworkPlayerManager.Instance.GetTheOtherPlayer(this);

            if (competitor != null)
                competitor.OnWin(competitor.Owner);

            StartCoroutine(LoadBackToMenuCoroutine());
        }

        [Server]
        public void OnWin(NetworkConnection winner)
        {
            OnWinCallback(winner);
        }

        /// <summary>
        /// Show the win UI for the winner.
        /// </summary>
        [TargetRpc]
        public void OnWinCallback(NetworkConnection receiver)
        {
            if (receiver.ClientId == Owner.ClientId)
            {
                UIManager.GetUIStatic<MessageBox>().SetMessage("Victory", "You're a GOAT");
            }
        }

        [Server]
        private IEnumerator LoadBackToMenuCoroutine()
        {
            NetworkPlayerManager.Instance.networkSceneAction.Value = ChangeSceneAction.Disconnect;

            yield return new WaitForSeconds(3);
            Debug.Log("Start Load Back To Menu scene.");
            NetworkSceneLoader.Singleton.LoadScene(SceneConfig.SCENE_DESERT_MAP, SceneConfig.SCENE_MENU);
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
                    MoveUnitTo_RPC_Server(hit.point);
                }
            }
        }

        [Client]
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

        [Client]
        protected virtual void MoveCameraInput()
        {
            if (commandInput.MoveCameraInput.magnitude > 0.1f)
                SceneCamera.Singleton.Move(commandInput.MoveCameraInput.x, commandInput.MoveCameraInput.y);
        }
        
        [Client]
        protected virtual void ChangeFireTargetInput()
        {
            if (commandInput.Tab)
            {
                OnChangeFireTarget();
            }
        }

        [ServerRpc]
        protected virtual void ChangeNextUnit_RPC_Server()
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
        protected virtual void ChangePrevUnit_RPC_Server()
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
        protected virtual void MoveUnitTo_RPC_Server(Vector3 destination)
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
                        AssignSelectedUnit_RPC_Server(null);
                    }
                    else
                    {
                        AssignSelectedUnit_RPC_Server(selectedUnit);
                    }
                    hud.SetUnitName(unitConfig.GetUnitProfileById(selectedUnit.Data.id).unitName);
                }
            }
            else
            {
                //Show warning popup.
            }
        }

        [ServerRpc]
        protected virtual void AssignSelectedUnit_RPC_Server(UnitController unit)
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

        [ObserversRpc]
        protected virtual void AssignSelectedUnit_RPC_Client(string unitID)
        {

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

        #region Turns Logic
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
        protected virtual void EndTurn_RPC_Server()
        {
            NetworkPlayerManager.Instance.ChangeNextPlayerTurn();
        }

        [ObserversRpc]
        protected virtual void StartTurnCallback()
        {
            if (IsOwner)
            {

            }
        }

        [ObserversRpc]
        protected virtual void OnStopTurnCallback()
        {
            if (IsOwner)
            {

            }
        }
        #endregion

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

        private void _ActiveUnits_OnChange(SyncDictionaryOperation op, UnitController key, int value, bool asServer)
        {
            /* Key will be provided for
            * Add, Remove, and Set. */
            switch (op)
            {
                //Adds key with value.
                case SyncDictionaryOperation.Add:
                    break;
                //Removes key.
                case SyncDictionaryOperation.Remove:

                    if (unitDictionary.Count == 0) // Mark the player is defeated when his/her units are all dead.
                        if (IsOwner)
                            Surrender();

                    break;
                //Sets key to a new value.
                case SyncDictionaryOperation.Set:
                    break;
                //Clears the dictionary.
                case SyncDictionaryOperation.Clear:
                    break;
                //Like SyncList, indicates all operations are complete.
                case SyncDictionaryOperation.Complete:
                    break;
            }
        }

        public static Ray GetRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return ray;
        }

        public static Vector3 GetMouseWorldPosition(MaskCategory maskCategory)
        {
            Physics.Raycast(GetRay(), out RaycastHit hit, SceneLayerMasks.GetLayerMaskByCategory(maskCategory));

            return hit.point;
        }

        
        public bool AllUnitAreDead()
        {
            return unitDictionary.Count == 0;
        }
    }
}