using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Relay;
using UnityEngine;

namespace TRPG.Unit.AI
{
    public class AIAbilitiesController : AbilitiesController
    {
        protected AIUnitController aiContext;
        public override void Setup(UnitController context)
        {
            base.Setup(context);
            aiContext = context.GetComponent<AIUnitController>();
        }

        [Server]
        public override void TryStartAbility(Ability ability)
        {
            currentAbility.Value = ability.Type;
            OnActivateAbilityServer?.Invoke(currentAbility.Value);
            OnActivateAbilityCallback();

            if (ability.TimerConfig.useTimer)
            {
                SetTimer(ability.TimerConfig.delay, ability.TimerConfig.clip != null ? ability.TimerConfig.clip.length : ability.TimerConfig.duration);
                delayTimer.StartTimer(_delay);
            }
        }

        protected override void SetTimer(float delay, float duration)
        {
            _delay = delay;
            _duration = duration * aiContext.AIWeaponManager.CurrentAIWeaponData.strikeCount; //The animation shall represent the strike count amount!
        }
    }
}