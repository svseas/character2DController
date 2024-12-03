using Godot;

namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public class FallingState : BaseState
    {
        private const float AIR_ACCELERATION = 800.0f;
        private const float AIR_MAX_SPEED = 250.0f;
        private const float AIR_CONTROL = 0.8f; // Reduced control in air
        private const float FAST_FALL_MULTIPLIER = 1.5f;
        private const float COYOTE_TIME = 0.1f;
        
        private double _timeInAir = 0.0;
        private bool _canCoyoteJump = false;

        public FallingState(ICharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("j_down");
            
            // Enable coyote time if we just walked off a platform
            _canCoyoteJump = StateMachine.Character.Velocity.Y >= 0 && 
                            StateMachine.Character.Velocity.Y < GetGravity() * 0.5f;
            _timeInAir = 0.0;
        }

        public override void HandleInput(InputEvent @event)
        {
            if (@event.IsActionPressed("attack"))
            {
                StateMachine.ChangeState("combat");
                return;
            }

            // Handle coyote time jump
            if (@event.IsActionPressed("ui_up") && _canCoyoteJump)
            {
                StateMachine.ChangeState("jumping");
                return;
            }
        }

        public override void PhysicsUpdate(double delta)
        {
            _timeInAir += delta;
            if (_timeInAir > COYOTE_TIME)
            {
                _canCoyoteJump = false;
            }

            var character = StateMachine.Character;
            var velocity = character.Velocity;
            Vector2 input = GetMovementInput();

            if (input.X != 0)
            {
                float targetSpeed = input.X * AIR_MAX_SPEED;
                velocity.X = Mathf.MoveToward(
                    velocity.X,
                    targetSpeed,
                    AIR_ACCELERATION * AIR_CONTROL * (float)delta
                );
                UpdateFacingDirection(input.X);
            }

            float gravityMultiplier = input.Y > 0 ? FAST_FALL_MULTIPLIER : 1.0f;
            velocity.Y += GetGravity() * gravityMultiplier * (float)delta;

            if (IsOnFloor())
            {
                if (Mathf.Abs(velocity.X) > 0.1f)
                {
                    StateMachine.ChangeState("moving");
                }
                else
                {
                    StateMachine.ChangeState("idle");
                }
                return;
            }

            UpdateVelocity(velocity);
        }

        public override void Update(double delta)
        {
        }

        public override void Exit()
        {
            base.Exit();
            LogDebug($"Exiting falling state. Time in air: {_timeInAir}");
        }
    }
}