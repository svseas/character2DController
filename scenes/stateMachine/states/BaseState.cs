using Godot;
using CSharp2DCharacterController.Scenes.Interfaces.Core;
using CSharp2DCharacterController.Scenes.Interfaces.Combat;
namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public interface ICharacterStateMachine
    {
        CharacterBody2D Character { get; }
        AnimatedSprite2D Sprite { get; }
        ICombatSystem CombatSystem { get; }
        void ChangeState(string newState);
    }

    public abstract class BaseState : IState
    {
        protected readonly ICharacterStateMachine StateMachine;
        protected bool IsDebugEnabled = false;

        protected BaseState(ICharacterStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        // ISystem implementation
        public virtual void Initialize() { }

        // IState implementation
        public virtual void Enter() 
        {
            if (IsDebugEnabled)
                GD.Print($"Entering {GetType().Name}");
        }

        public virtual void Exit() 
        {
            if (IsDebugEnabled)
                GD.Print($"Exiting {GetType().Name}");
        }

        public virtual void Update(double delta) { }

        public virtual void PhysicsUpdate(double delta) { }

        public virtual void HandleInput(InputEvent @event) { }

        // Utility methods for all states
        protected bool IsAnimationPlaying(string animName)
        {
            return StateMachine.Sprite.SpriteFrames?.HasAnimation(animName) == true &&
                   StateMachine.Sprite.Animation == animName &&
                   StateMachine.Sprite.IsPlaying();
        }

        protected void PlayAnimation(string animName, bool shouldReset = true)
        {
            if (StateMachine.Sprite.SpriteFrames?.HasAnimation(animName) == true)
            {
                if (shouldReset || StateMachine.Sprite.Animation != animName)
                {
                    StateMachine.Sprite.Play(animName);
                }
            }
            else
            {
                GD.PrintErr($"Animation not found: {animName}");
            }
        }

        protected bool IsOnFloor()
        {
            return StateMachine.Character.IsOnFloor();
        }

        protected Vector2 GetMovementInput()
        {
            return Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        }

        protected void UpdateVelocity(Vector2 newVelocity)
        {
            StateMachine.Character.Velocity = newVelocity;
            StateMachine.Character.MoveAndSlide();
        }

        protected void UpdateFacingDirection(float directionX)
        {
            if (directionX != 0)
            {
                StateMachine.Sprite.FlipH = directionX < 0;
            }
        }

        protected float GetGravity()
        {
            return ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
        }

        protected void LogDebug(string message)
        {
            if (IsDebugEnabled)
                GD.Print($"[{GetType().Name}] {message}");
        }
    }
}