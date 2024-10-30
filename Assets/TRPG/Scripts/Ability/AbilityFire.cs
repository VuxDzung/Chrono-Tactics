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
            context.CombatBrain.Scanning();

            if (!context.CombatBrain.HasEnemy) //Reset the ability
            {
                context.AbilityController.ResetDefaultAbility();
            }
        }

        #endregion

        #region Callback

        public override void OnSelectCallback(AbilityType type, UnitController context, bool isOwner)
        {
            base.OnSelectCallback(type, context, isOwner);
            if (isOwner)
            {
                if (context.CombatBrain.HasEnemy)
                {
                    UIManager.ShowUI<AimHUD>();
                    context.EnableTPCamera();
                    GridManager.Singleton.DisableAllCells();
                }
                else
                {
                    UIManager.ShowUI<MessageBoxTimer>().SetMessage("", "No enemy available!");
                }
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