using DevOpsGuy.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu(fileName = "Ability-Fire", menuName = "TRPG/Ability/Fire")]
    public class AbilityFire : Ability
    {
        public override void OnSelectCallback(AbilityType type, bool isOwner)
        {
            base.OnSelectCallback(type, isOwner);
            if (isOwner)
                UIManager.ShowUI<AimHUD>();
        }

        public override void OnDeselectCallback(AbilityType type, bool isOwner)
        {
            base.OnDeselectCallback(type, isOwner);
            if (isOwner)
            {
                UIManager.HideUI<AimHUD>();
            }
        }
    }
}