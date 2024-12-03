using Godot;

namespace CSharp2DCharacterController.Scenes
{
    public partial class InputSetup : Node
    {
        public override void _Ready()
        {

            if (!InputMap.HasAction("attack"))
            {
                InputMap.AddAction("attack");
                
                InputEventKey eventKey = new InputEventKey();
                eventKey.Keycode = Key.A;
                
                InputMap.ActionAddEvent("attack", eventKey);
            }
        }
    }
}