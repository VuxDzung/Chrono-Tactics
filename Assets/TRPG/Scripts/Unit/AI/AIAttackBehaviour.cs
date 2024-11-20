using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIAttackBehaviour : AIAbilityBehaviour
    {
        /// <summary>
        /// Evaluate the most possible target.
        /// If the current unit is using a melee weapon, select the current target shall be the one who the unit has already select before when advance.
        /// Else if the weapon is long range weapon, recalculate the possible target.
        /// </summary>
        /// <returns></returns>
        [Server]
        public override int Evaluate()
        {
            base.Evaluate();
            List<UnitController> playersUnitList = TRPGGameManager.Instance.GetAllPlayersUnits();
            int highestScore = 0;

            foreach (var player in playersUnitList)
            {
                int distance = GetDistance(player.transform);
                int score = 0;

                if (WeaponData.IsCloseRangeWeapon(aiContext.AIWeaponManager.CurrentAIWeaponData.weaponType))
                {
                    if (distance > aiContext.AIWeaponManager.CurrentAIWeaponData.range) continue;

                    else score += 50;

                    aiContext.AICombatBrain.SelectedTarget = player;
                    return score;
                }
                else
                {
                    int hitChance = (int)aiContext.CombatBrain.CalculateHitChance(player, aiContext.AIWeaponManager.CurrentAIWeaponData.baseAccuracy);
                    
                    if (hitChance >= 75) score += 50;

                    else if (hitChance >= 50) score += 20;
                }

                //bool isHighPriority = player.IsHighPriority;

                if (player.Health.CurrentHealth <= aiContext.AIWeaponManager.CurrentAIWeaponData.baseDamage) score += 40; // Guaranteed kill
                
                else if (player.Health.CurrentHealth <= aiContext.AIWeaponManager.CurrentAIWeaponData.baseDamage * 1.5f) score += 20; // Likely kill

                score += 10 / Mathf.Max(1, distance);

                //if (isHighPriority) score += 30;

                highestScore = Mathf.Max(highestScore, score);

                if (highestScore == score) aiContext.AICombatBrain.SelectedTarget = player; 
            }

            return highestScore;
        }

        protected override void OnActivateServer()
        {
            if (WeaponData.IsCloseRangeWeapon(aiContext.AIWeaponManager.CurrentAIWeaponData.weaponType))
            {
                aiContext.AIAnimationController.MeleeAnimation();
            }
        }

        protected override void OnDurationFinishedServer()
        {
            controller.ResetDefaultAbility();
        }
    }
}