using DevOpsGuy.GUI;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TRPG.Unit;
using UnityEngine;

namespace TRPG 
{
    public class AbilityShoot : AbilityBehaviour
    {
        public override void Initialized(AbilitiesController controller, UnitController context)
        {
            base.Initialized(controller, context);
            context.CombatBrain.OnScanningComplete += Scanned;
            context.CombatBrain.OnScanningCompleteCallback += ScannedCallback;
        }


        #region Server-Side
        protected override void OnSelectServer()
        {
            context.CombatBrain.Scanning();                
        }

        protected override void OnActivateServer()
        {
            context.CombatBrain.Fire();
        }

        private void Scanned(bool hasTarget)
        {
            if (!hasTarget) controller.ResetDefaultAbility();
        }

        #endregion

        #region Callback

        private void ScannedCallback(bool hasTarget, bool asOwner)
        {
            if (asOwner)
            {
                if (hasTarget)
                {
                    //UIManager.ShowUIStatic<AimHUD>();
                    //context.EnableTPCamera();
                    GridManager.Singleton.DisableAllCells();
                }
                else
                {
                    UIManager.ShowUIStatic<MessageBoxTimer>().SetMessage("No enemy available!");
                }
            }
        }

        protected override void OnSelectCallback(bool isOwner)
        {            

        }

        protected override void OnDeselectCallback(bool isOwner)
        {
            if (isOwner)
            {
                UIManager.HideUIStatic<AimHUD>();
                context.DisableTPCamera();
                context.EnableCellsAroundUnit();
            }
        }

        protected override void OnActivateCallback(bool isOwner)
        {
            if (isOwner)
            {
                UIManager.HideUIStatic<AimHUD>();
            }
        }

        protected override void OnDurationFinishedServer()
        {
            base.OnDurationFinishedServer();
            controller.ResetDefaultAbility();
        }
        #endregion
    }
}