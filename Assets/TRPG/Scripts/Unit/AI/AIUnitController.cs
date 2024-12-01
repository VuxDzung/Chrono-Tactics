using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIUnitController : UnitController
    {
        public AICombatBrain AICombatBrain { get; protected set; }
        public AIWeaponManager AIWeaponManager { get; protected set; }
        public AIAbilitiesController AIAbilities { get; protected set; }
        public AIAnimationController AIAnimationController { get; protected set; }

        public AIDecisionMaker AIPlayer { get; protected set; }

        public bool AIHasEnoughPoint => AIPlayer.HasEnoughPoint(this);

        public override void Initialized()
        {
            base.Initialized();
            AIPlayer = AIDecisionMaker.Instance;
            AICombatBrain = GetComponent<AICombatBrain>();
            AIWeaponManager = GetComponent<AIWeaponManager>();
            AIAbilities = GetComponent<AIAbilitiesController>();
            AIAnimationController = GetComponent<AIAnimationController>();

            AICombatBrain.Setup(this);
            AIWeaponManager.Setup(this);
            AIAbilities.Setup(this);
            AIAnimationController.Setup(this);
        }

        public override void OnDead()
        {
            AIDecisionMaker.Instance.UnregisterAI(this);
        }

        public virtual bool TryMoveAI(Vector3 destination)
        {
            if (!AIHasEnoughPoint)
            {
                Debug.Log($"{gameObject.name} does not have enough point!");
                return false;
            }
            Vector3 validPosition = GridManager.GetValidCell(destination);
            return TryMove(validPosition);
        }
    }
}