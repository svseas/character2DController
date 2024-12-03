using Godot;
using CSharp2DCharacterController.Scenes.StateMachine.States;

namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public class MovingState : BaseState
    {
        private const float ACCELERATION = 1000.0f;
        private const float MAX_SPEED = 300.0f;
        private const float TURNING_MULTIPLIER = 2.0f; // Faster direction changes

        public MovingState(ICharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("run");
        }

        public override void HandleInput(InputEvent @event)
        {
            if (@event.IsActionPressed("attack"))
            {
                StateMachine.ChangeState("combat");
                return;
            }

            if (@event.IsActionPressed("ui_up") && IsOnFloor())
            {
                StateMachine.ChangeState("jumping");
                return;
            }
        }

        public override void PhysicsUpdate(double delta)
        {
            var character = StateMachine.Character;
            var velocity = character.Velocity;
            Vector2 input = GetMovementInput();

            if (input.X != 0)
            {
                float targetSpeed = input.X * MAX_SPEED;
                float accelerationRate = ACCELERATION;

                if (Mathf.Sign(velocity.X) != Mathf.Sign(targetSpeed) && velocity.X != 0)
                {
                    accelerationRate *= TURNING_MULTIPLIER;
                }

                velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, accelerationRate * (float)delta);
                UpdateFacingDirection(input.X);
            }
            else
            {
                StateMachine.ChangeState("idle");
                return;
            }

            if (!IsOnFloor())
            {
                StateMachine.ChangeState("falling");
                return;
            }

            if (!IsOnFloor())
            {
                velocity.Y += GetGravity() * (float)delta;
            }

            UpdateVelocity(velocity);
        }

        public override void Update(double delta)
        {

        }

        public override void Exit()
        {
            base.Exit();
            LogDebug("Exiting moving state");
        }
    }
}