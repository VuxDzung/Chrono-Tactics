using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using TRPG.Unit;
using UnityEngine;

namespace TRPG 
{
    public class AbilityShoot : AbilityBehaviour
    {
        #region Server-Side
        public override void OnSelectServer(AbilityType type)
        {
            base.OnSelectServer(type);
            context.CombatBrain.Scanning();

            if (!context.CombatBrain.HasEnemy) //Reset the ability
            {
                context.AbilityController.ResetDefaultAbility();
            }
        }

        #endregion

        #region Callback

        public override void OnSelectCallback(AbilityType type, bool isOwner)
        {
            base.OnSelectCallback(type, isOwner);
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

        public override void OnDeselectCallback(AbilityType type, bool isOwner)
        {
            base.OnDeselectCallback(type, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
                context.DisableTPCamera();
                context.EnableCellsAroundUnit();
            }
        }

        public override void OnActivateCallback(AbilityType type, bool isOwner)
        {
            base.OnActivateCallback(type, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
            }
        }

        public override void OnActivateServer(AbilityType type)
        {
            base.OnActivateServer(type);
            context.CombatBrain.Fire();
        }

        public override void OnDurationFinished(AbilityType type, bool asServer)
        {
            base.OnDurationFinished(type, asServer);
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