namespace TRPG
{
    public class AbilityStrikeSword : AbilityBehaviour
    {
        protected override void OnActivateServer()
        {

        }

        protected override void OnActivateCallback(bool isOwner)
        {
            base.OnActivateCallback(isOwner);
            context.AnimationController.MeleeAnimation();
        }
    }
}