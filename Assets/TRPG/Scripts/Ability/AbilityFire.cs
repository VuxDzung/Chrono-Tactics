using DevOpsGuy.GUI;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu(fileName = "Ability-Fire", menuName = "TRPG/Ability/Fire")]
    public class AbilityFire : Ability
    {
        #region Server-Side
        public override void OnSelectServer(AbilityType type, UnitController context)
        {
            base.OnSelectServer(type, context);
            Debug.Log($"{context.UnitOwner.Value.gameObject.name}");
            Debug.Log($"{context.gameObject.name}.OnSelectServer");
            context.CombatBrain.Scanning();
        }

        #endregion

        #region Callback

        public override void OnSelectCallback(AbilityType type, UnitController context, bool isOwner)
        {
            base.OnSelectCallback(type, context, isOwner);
            if (isOwner)
            {
                UIManager.ShowUI<AimHUD>();
                context.EnableTPCamera();
                GridManager.Singleton.DisableAllCells();
            }
        }

        public override void OnDeselectCallback(AbilityType type, UnitController context, bool isOwner)
        {
            base.OnDeselectCallback(type, context, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
                context.DisableTPCamera();
                context.EnableCellsAroundUnit();
            }
        }

        public override void OnActivateCallback(AbilityType type, UnitController context, bool isOwner)
        {
            base.OnActivateCallback(type, context, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
            }  
        }

        public override void OnActivateServer(AbilityType type, UnitController context)
        {
            base.OnActivateServer(type, context);
            context.CombatBrain.Fire();
        }

        public override void OnDurationFinished(AbilityType type, UnitController context, bool asServer)
        {
            base.OnDurationFinished(type, context, asServer);
            if (asServer)
            {
                context.AbilityController.ResetDefaultAbility();
            }

            if (!asServer)
            {
                context.DisableTPCamera();
            }
        }
        #endregion
    }
}