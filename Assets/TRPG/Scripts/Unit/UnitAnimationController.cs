using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TRPG.Unit
{
    public class UnitAnimationController : CoreNetworkBehaviour
    {
        #region Animator Parameters
        public static int PARAM_MOVE_MAGNITUDE = Animator.StringToHash("MoveMagnitude");
        #endregion

        private Animator animator;
        private UnitController context;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            animator = GetComponent<Animator>();
        }

        public override void Update()
        {
            base.Update();
            if (IsOwner)
            {
                UpdateAnimator();
            }
        }

        public virtual void UpdateAnimator()
        {
            animator.SetFloat(PARAM_MOVE_MAGNITUDE, context.Motor.MoveMagnitude, 0.15f, Time.deltaTime);
        }
    }
}