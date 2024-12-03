using Godot;
namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public class IdleState : BaseState
    {
        private const float FRICTION = 1000.0f;

        public IdleState(ICharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("idle");
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

            velocity.X = Mathf.MoveToward(velocity.X, 0, FRICTION * (float)delta);

            Vector2 input = GetMovementInput();
            if (input.X != 0)
            {
                StateMachine.ChangeState("moving");
                return;
            }

            if (Input.IsActionPressed("ui_up") && IsOnFloor())
            {
                StateMachine.ChangeState("jumping");
                return;
            }

            if (!IsOnFloor())
            {
                StateMachine.ChangeState("falling");
                return;
            }

            // Apply gravity
            if (!IsOnFloor())
            {
                velocity.Y += GetGravity() * (float)delta;
            }

            // Update velocity and move
            UpdateVelocity(velocity);
        }

        public override void Exit()
        {
            base.Exit();
            LogDebug("Exiting idle state");
        }
    }
}