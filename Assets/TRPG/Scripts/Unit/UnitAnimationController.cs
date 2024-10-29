using FishNet.Component.Animating;
using FishNet.Object;
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
        public static int PARAM_STOP_FIRE = Animator.StringToHash("StopFire");
        #endregion

        #region Animator Layer Index 
        public static int LAYER_BASE = 0;
        public static int LAYER_ARMS = 1;
        public static int LAYER_FULL_BODY = 2;
        #endregion

        #region Animator States
        public static string STATE_RIFLE_FIRE = "FireRifle";
        public static string STATE_DEAD = "Dead";
        #endregion

        private Animator animator;
        private UnitController context;
        private NetworkAnimator nwAnimator;

        public virtual void Setup(UnitController context)
        {
            this.context = context;
            animator = GetComponent<Animator>();
            nwAnimator = GetComponent<NetworkAnimator>();
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


        public virtual void TriggerFireAnimation()
        {
            CrossFade(STATE_RIFLE_FIRE, 0.2f, LAYER_ARMS);
        }

        public virtual void DeadAnimation()
        {
            CrossFade(STATE_DEAD, 0.2f, LAYER_FULL_BODY);
        }

        public virtual void CrossFade(string state, float duration, int layerIndex)
        {
            nwAnimator.CrossFadeInFixedTime(state, duration, layerIndex);
        }

        public void StartFire()
        {
            animator.SetBool(PARAM_STOP_FIRE, false);
        }

        public void StopFire()
        {
            animator.SetBool(PARAM_STOP_FIRE, true);
        }
    }
}