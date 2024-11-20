namespace TRPG.Unit.AI
{
    public class FallbackBehaviour : AIAbilityBehaviour
    {
        public override int Evaluate()
        {
            int highestScore = 0;

            bool isHealthCritical = aiContext.Health.CurrentHealth < 30;
            bool isSurrounded = aiContext.AICombatBrain.IsSurrounded();

            if (isHealthCritical) highestScore += 30;

            if (isSurrounded) highestScore += 10;
            

            return highestScore;
        }
    }
}