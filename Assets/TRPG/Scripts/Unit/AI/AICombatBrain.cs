using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AICombatBrain : UnitCombatBrain
    {
        public const int OutNumberEnemyCount = 3;
        public AIUnitController AIUnitController { get; private set; }

        public override void Setup(UnitController context)
        {
            base.Setup(context);
            AIUnitController = context.GetComponent<AIUnitController>();
        }

        public UnitController SelectedTarget { get; set; }

        public virtual bool IsSurrounded()
        {
            Collider[] enemyCollider = Physics.OverlapSphere(context.transform.position, context.Data.viewRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));
            return enemyCollider.Length > OutNumberEnemyCount;
        }

        protected override void OnDamageServer()
        {
            if (SelectedTarget != null)
                SelectedTarget.Health.TakeDamage(AIUnitController.AIWeaponManager.CurrentAIWeaponData.baseDamage);
        }

        public virtual int CalculatePossibleTarget()
        {
            List<UnitController> playersUnitList = TRPGGameManager.Instance.GetAllPlayersUnits();
            int highestScore = 0;

            foreach (var player in playersUnitList)
            {
                int distance = GetDistance(player.transform);
                int score = 0;
/*
                if (WeaponData.IsCloseRangeWeapon(AIUnitController.AIWeaponManager.CurrentAIWeaponData.weaponType))
                {
                    if (distance > AIUnitController.AIWeaponManager.CurrentAIWeaponData.range) continue;

                    else score += 50;
                }
                else
                {
                    int hitChance = (int) AIUnitController.CombatBrain.CalculateHitChance(player, AIUnitController.AIWeaponManager.CurrentAIWeaponData.baseAccuracy);

                    if (hitChance >= 75) score += 50;

                    else if (hitChance >= 50) score += 20;
                }
*/
                //bool isHighPriority = player.IsHighPriority;

                if (player.Health.CurrentHealth <= AIUnitController.AIWeaponManager.CurrentAIWeaponData.baseDamage) score += 40; // Guaranteed kill

                else if (player.Health.CurrentHealth <= AIUnitController.AIWeaponManager.CurrentAIWeaponData.baseDamage * 1.5f) score += 20; // Likely kill

                score += 10 / Mathf.Max(1, distance);

                //if (isHighPriority) score += 30;

                highestScore = Mathf.Max(highestScore, score);

                if (highestScore == score) AIUnitController.AICombatBrain.SelectedTarget = player;
            }

            return highestScore;
        }

        public int GetDistance(Transform target)
        {
            // Calculate Manhattan distance as an example
            int dx = Mathf.Abs((int)(target.transform.position.x - AIUnitController.transform.position.x));
            int dy = Mathf.Abs((int)(target.transform.position.y - AIUnitController.transform.position.y));
            return dx + dy;
        }

        public virtual void ScanForClosestTarget()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, context.Data.viewRadius, SceneLayerMasks.GetLayerMaskByCategory(MaskCategory.Unit));
            List<HealthController> enemyList = new List<HealthController>();
            foreach (Collider collider in colliders)
            {
                HealthController sceneUnit = collider.GetComponent<HealthController>();
                if (sceneUnit != null && !sceneUnit.IsDead)
                    enemyList.Add(sceneUnit);
            }
        }

        protected override void OnDamageClient()
        {
            
        }
    }
}