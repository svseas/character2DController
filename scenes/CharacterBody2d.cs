using Godot;
using CSharp2DCharacterController.Scenes.Interfaces.Combat;
using CSharp2DCharacterController.Scenes.StateMachine;


namespace CSharp2DCharacterController.Scenes
{
    public partial class CharacterBody2d : CharacterBody2D
{
    public const float Speed = 300.0f;
    public const float JumpVelocity = -400.0f;

    private AnimatedSprite2D _sprite;
    private ICombatSystem _combatSystem;
    private CharacterStateMachine _stateMachine;

    public override void _Ready()
    {
        // Get required nodes - don't create them dynamically
        _sprite = GetNode<AnimatedSprite2D>("Movement");
        _combatSystem = GetNode<ICombatSystem>("CombatSystem");
        _stateMachine = GetNode<CharacterStateMachine>("CharacterStateMachine");

        // Validate all required nodes exist
        if (_sprite == null)
        {
            GD.PrintErr("Movement AnimatedSprite2D node not found!");
            return;
        }

        if (_combatSystem == null)
        {
            GD.PrintErr("CombatSystem node not found!");
            return;
        }

        if (_stateMachine == null)
        {
            GD.PrintErr("StateMachine node not found!");
            return;
        }

        // Initialize in correct order
        _combatSystem.Initialize();
        _stateMachine.Initialize();
        
        // Debug available animations
        if (_sprite.SpriteFrames != null)
        {
            var animations = _sprite.SpriteFrames.GetAnimationNames();
            GD.Print($"Available animations: {string.Join(", ", animations)}");
        }
    }

    public static float GetSpeed() => Speed;
    public static float GetJumpVelocity() => JumpVelocity;
}
}