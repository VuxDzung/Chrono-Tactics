using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIAbilityBehaviour : AbilityBehaviour
    {
        protected AIUnitController aiContext;

        public override void Initialized(AbilitiesController controller, UnitController context)
        {
            base.Initialized(controller, context);
            aiContext = controller.GetComponent<AIUnitController>();
        }

        public virtual int Evaluate() { return -1; }

        public virtual void Execute() { }

        public int GetDistance(Transform target)
        {
            // Calculate Manhattan distance as an example
            int dx = Mathf.Abs((int) (target.transform.position.x - aiContext.transform.position.x));
            int dy = Mathf.Abs((int)(target.transform.position.y - aiContext.transform.position.y));
            return dx + dy;
        }
    }
}