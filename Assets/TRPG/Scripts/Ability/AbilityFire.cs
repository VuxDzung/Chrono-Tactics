using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu(fileName = "Ability-Fire", menuName = "TRPG/Ability/Fire")]
    public class AbilityFire : Ability
    {
        #region Server-Side
        public override void OnSelectServer(AbilityType type)
        {
            base.OnSelectServer(type);
            context.CombatBrain.Scanning();
        }

        #endregion

        #region Callback

        public override void OnSelectCallback(AbilityType type, bool isOwner)
        {
            base.OnSelectCallback(type, isOwner);
            if (isOwner)
            {
                Debug.Log("AbilityFire.OnSelectCallback");
                UIManager.ShowUI<AimHUD>();
                context.EnableTPCamera();
            }
        }

        public override void OnDeselectCallback(AbilityType type, bool isOwner)
        {
            base.OnDeselectCallback(type, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
                context.DisableTPCamera();
            }
        }

        public override void OnActivateCallback(AbilityType type, bool isOwner)
        {
            base.OnActivateCallback(type, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
                context.AnimationController.TriggerFireAnimation();
            }  
        }

        public override void OnDurationFinished(AbilityType type, bool asServer)
        {
            base.OnDurationFinished(type, asServer);
            if (!asServer)
            {
                context.AbilityController.CancelAbility();
            }
        }
        #endregion
    }
}