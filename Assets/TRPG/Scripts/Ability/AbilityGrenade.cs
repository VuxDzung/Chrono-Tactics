using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    public class AbilityGrenade : AbilityBehaviour
    {
        protected override void OnSelectServer()
        {
            context.CombatBrain.IsGrenadeAbility.Value = true;
        }

        protected override void OnSelectCallback(bool isOwner)
        {
            // Show the blast sphere.
            if (isOwner)
            {
                GridManager.Singleton.DisableAllCells();
                BlastSphere.Activate(context.WeaponManager.Config.GetDataByType(WeaponType.Grenade).grenadeSettings.blastRadius);
            }
        }

        protected override void OnDeselectServer()
        {
            context.AbilityController.CancelAbility();
        }

        protected override void OnDeselectCallback(bool isOwner)
        {
            if (isOwner)
            {
                BlastSphere.Deactivate();
                context.EnableCellsAroundUnit();
            }
        }

        protected override void OnActivateServer()
        {
            context.CombatBrain.RotateToTossDirection();
        }

        protected override void OnActivateCallback(bool isOwner)
        {
            if (isOwner)
            {
                BlastSphere.Deactivate();
                context.AnimationController.TossGrenadeAnimation();
            }
        }

        protected override void OnDurationFinishedServer()
        {
            context.CombatBrain.IsGrenadeAbility.Value = false;
            context.AbilityController.ResetDefaultAbility();
        }
    }
}