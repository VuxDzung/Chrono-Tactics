using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIAnimationController : UnitAnimationController
    {
        protected AIUnitController aiContext;
        public override void Setup(UnitController context)
        {
            base.Setup(context);
            aiContext = context.GetComponent<AIUnitController>();
        }

        public override void Update()
        {
            UpdateAnimator();
        }

        protected override void UpdateAnimator()
        {
            Log("AI", $"{aiContext.gameObject.name}.MoveMagnitude={aiContext.CC.MoveMagnitude}", TextColor.Green);
            animator.SetFloat(PARAM_MOVE_MAGNITUDE, aiContext.CC.MoveMagnitude, 0.15f, Time.deltaTime);
        }
    }
}