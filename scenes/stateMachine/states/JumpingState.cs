using Godot;
namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public class JumpingState : BaseState
    {
        private const float JUMP_FORCE = -400.0f;
        private const float AIR_ACCELERATION = 800.0f;
        private const float AIR_MAX_SPEED = 250.0f;
        private const float AIR_CONTROL = 0.8f; // Reduced control in air

        public JumpingState(ICharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("j_up");

            // Apply initial jump velocity
            var velocity = StateMachine.Character.Velocity;
            velocity.Y = JUMP_FORCE;
            UpdateVelocity(velocity);
        }

        public override void HandleInput(InputEvent @event)
        {
            if (@event.IsActionPressed("attack"))
            {
                StateMachine.ChangeState("combat");
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
                float targetSpeed = input.X * AIR_MAX_SPEED;
                velocity.X = Mathf.MoveToward(
                    velocity.X,
                    targetSpeed,
                    AIR_ACCELERATION * AIR_CONTROL * (float)delta
                );
                UpdateFacingDirection(input.X);
            }

            velocity.Y += GetGravity() * (float)delta;


            if (velocity.Y >= 0)
            {
                StateMachine.ChangeState("falling");
                return;
            }

            UpdateVelocity(velocity);
        }

        public override void Exit()
        {
            base.Exit();
            LogDebug("Exiting jumping state");
        }
    }
}