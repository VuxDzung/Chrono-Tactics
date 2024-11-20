using DevOpsGuy.GUI;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TRPG.Unit;
using UnityEngine;

namespace TRPG 
{
    public class AbilityShoot : AbilityBehaviour
    {
        #region Server-Side
        protected override void OnSelectServer()
        {
            context.CombatBrain.Scanning();

            if (!context.CombatBrain.HasEnemy) //Reset the ability
                controller.ResetDefaultAbility();
        }

        protected override void OnActivateServer()
        {
            if (context.CombatBrain.HasEnemy) context.CombatBrain.Fire();
        }

        #endregion

        #region Callback

        protected override void OnSelectCallback(bool isOwner)
        {            
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
                    UIManager.ShowUI<MessageBoxTimer>().SetMessage("No enemy available!");
                }
            }
        }

        protected override void OnDeselectCallback(bool isOwner)
        {
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
                context.DisableTPCamera();
                context.EnableCellsAroundUnit();
            }
        }

        protected override void OnActivateCallback(bool isOwner)
        {
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
            }
        }

        protected override void OnDurationFinishedServer()
        {
            base.OnDurationFinishedServer();
            controller.ResetDefaultAbility();
        }

        protected override void OnDurationFinishedCallback()
        {
            context.EnableCellsAroundUnit();
            context.DisableTPCamera();
        }
        #endregion
    }
}