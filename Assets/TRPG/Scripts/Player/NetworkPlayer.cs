using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TRPG.Unit;
using FishNet.Object;

namespace TRPG
{
    public enum ActionPointCost
    {
        Half,
        Full
    }

    public class NetworkPlayer : CoreNetworkBehaviour
    {
        private readonly SyncVar<bool> isOwnerTurn = new SyncVar<bool>();
        [SerializeField] private CommandInputManager commandInput;
        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private UnitController testUnitPrefab;
        [SerializeField] private Transform testPosition;

        private const int DEFAULT_ACTION_POINT = 2;

        private readonly SyncDictionary<UnitController, int> unitDictionary = new SyncDictionary<UnitController, int>();
        private readonly SyncVar<UnitController> selectedUnit = new SyncVar<UnitController>();


        public override void OnStartClient()
        {
            base.OnStartClient();
            OnInitUnits();
        }

        [ServerRpc]
        protected virtual void OnInitUnits()
        {
            UnitController unit = Instantiate(testUnitPrefab, testPosition.position, Quaternion.identity);
            ServerManager.Spawn(unit.gameObject, Owner);
            RegisterUnit(unit);
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

        public override void OnClientUpdate()
        {
            base.OnClientUpdate();
            if (IsOwner)
            {
                SelectUnitInput();
                MovePlayerUnit();
            }
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

        protected virtual void MovePlayerUnit()
        {
            if (commandInput.RightMouseDown)
            {
                Physics.Raycast(GetRay(), out RaycastHit hit, groundLayer);
                if (hit.transform && selectedUnit.Value != null)
                {
                    if (!HasEnoughPoint(selectedUnit.Value)) return;

                    Vector3 pos = RoundVector3(hit.point, 1);
                    selectedUnit.Value.Motor.MoveTo(pos);
                    SpendActionPoint(selectedUnit.Value, ActionPointCost.Half);
                    Debug.Log($"UnitDestination={pos}");
                }
            }
        }
        /// <summary>
        /// When select an unit, the unit visual select shall show up along with the unit's UI.
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void OnSelectUnit(RaycastHit hit)
        {
            UnitController selectedUnit = hit.transform.GetComponent<UnitController>();

            if (selectedUnit != null)
            {
                if (unitDictionary.ContainsKey(selectedUnit))
                {
                    if (selectedUnit.IsSelected)
                    {
                        selectedUnit.Deselect();
                        AssignSelectedUnit(null);
                    }
                    else
                    {
                        selectedUnit.Select();
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
            selectedUnit.Value = unit;
        }

        [Server]
        public virtual void ResetUnitActionPoint()
        {
            foreach (var item in unitDictionary)
            {
                unitDictionary[item.Key] = 2;
            }
        }

        [Server]
        public virtual void StartOwnerTurn()
        {
            commandInput.LockInput.Value = false;
            isOwnerTurn.Value = true;
            ResetUnitActionPoint();
        }

        [Server]
        public virtual void StopOwnerTurn()
        {
            isOwnerTurn.Value = false;
            commandInput.LockInput.Value = true;
        }

        #region Action Points

        [ServerRpc]
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

        public static Vector3Int RoundVector3(Vector3 vector, int ceilSize)
        {
            int xA = Mathf.RoundToInt(vector.x / ceilSize);
            int yA = Mathf.RoundToInt(vector.y);
            int zA = Mathf.RoundToInt(vector.z / ceilSize);

            Vector3Int centerCoordinate = new Vector3Int(xA, yA, zA);

            return centerCoordinate;
        }
    }
}