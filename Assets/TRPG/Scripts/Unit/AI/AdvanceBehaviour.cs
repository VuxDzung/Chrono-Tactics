using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AdvanceBehaviour : AIAbilityBehaviour
    {
        public override int Evaluate()
        {
            int highestScore = 0;
            bool isHealthCritical = aiContext.Health.CurrentHealth > 60;

            if (isHealthCritical) highestScore += 10;

            return highestScore;
        }

        /// <summary>
        /// Get the closest position to the enemy.
        /// - If the current unit is long-ranged unit, move to the cover spot which is the closest to the enemy.
        /// - Else if the current unit is close-range unit, move to the point within the move range which is the closest point to the enemy.
        /// </summary>
        /// <param name="type"></param>
        protected override void OnActivateServer()
        {
            aiContext.AICombatBrain.CalculatePossibleTarget();
            if (aiContext.AICombatBrain.SelectedTarget == null)
            {
                // End the action now.
                Debug.Log("Has no target");
                return;
            }

            Debug.Log($"Selected Target of {aiContext.gameObject.name}: {aiContext.AICombatBrain.SelectedTarget.gameObject.name}");
            // Get the target position
            Vector3 targetPosition = aiContext.AICombatBrain.SelectedTarget.transform.position;

            // Determine the AI unit's movement range and type
            float moveRange = aiContext.Data.viewRadius;
            bool isLongRanged = WeaponData.IsLongRangeWeapon(aiContext.AIWeaponManager.CurrentAIWeaponData.weaponType);

            // Get the list of potential positions within the unit's move range
            List<Vector3> potentialPositions = GridManager.Singleton.GetSurroundingPositionList(aiContext.transform.position, aiContext.Data.viewRadius, true);

            Vector3 bestPosition = aiContext.transform.position; // Default to current position
            float bestScore = float.MinValue;

            foreach (Vector3 position in potentialPositions)
            {
                // Calculate distance to the target position
                float distanceToTarget = Vector3.Distance(position, targetPosition);

                // Evaluate cover if the unit is long-ranged
                Obstacle nearbyObstacle = SceneObstacleManager.Singleton.GetClosestObstacleToTarget(position);
                bool hasCover = nearbyObstacle != null && nearbyObstacle.HasAvailableSpot;

                // Scoring logic
                float score = 0;

                if (isLongRanged)
                {
                    // Long-ranged units prioritize cover and optimal range
                    score += hasCover ? 50 : -30; // Cover bonus
                    score -= distanceToTarget;    // Closer to target is better
                }
                else
                {
                    // Close-range units prioritize getting as close as possible to the target
                    score -= distanceToTarget * 2; // Stronger weight on being close
                }

                // Update the best position if the score improves
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = position;
                }
            }
            // Move the AI unit to the best position
            Debug.Log($"{aiContext.gameObject.name} move to best position: {bestPosition}");
            if (!aiContext.TryMoveAI(bestPosition))
            {
                Debug.LogWarning($"{gameObject.name} cannot move to {bestPosition}!");
            }
        }
    }
}