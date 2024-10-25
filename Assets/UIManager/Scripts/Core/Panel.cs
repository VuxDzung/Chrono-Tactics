namespace DevOpsGuy.GUI
{
    public class Panel : UIBehaviour
    {
        public override void OnShortcutPressed()
        {
            manager.HideAllPanels();
            base.OnShortcutPressed();
        }
    }
}