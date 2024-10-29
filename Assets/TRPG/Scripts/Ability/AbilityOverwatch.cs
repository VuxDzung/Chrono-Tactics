using UnityEngine;

namespace TRPG.Unit
{
    [CreateAssetMenu(fileName = "Ability-Overwatch", menuName = "TRPG/Ability/Overwatch")]
    public class AbilityOverwatch : Ability
    {
        public override void OnActivateServer(AbilityType type, UnitController context)
        {
            base.OnActivateServer(type, context);
            context.CombatBrain.StartOverwatch();
        }
    }
}
