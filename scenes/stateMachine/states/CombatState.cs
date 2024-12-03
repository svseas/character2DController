using Godot;

namespace CSharp2DCharacterController.Scenes.StateMachine.States
{
    public class CombatState : BaseState
    {
        private const float COMBAT_FRICTION = 800.0f;
        private bool _isTransitioning = false;
        private Timer _recoveryTimer;
        private Vector2 _lastDirection;
        private bool _isInitialized;
        private bool _attackTriggeredThisFrame = false;

        public CombatState(ICharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Initialize()
        {
            if (_isInitialized) return;

            if (StateMachine.Character == null)
            {
                GD.PrintErr("CombatState: Character reference is null!");
                return;
            }

            if (StateMachine.CombatSystem == null)
            {
                GD.PrintErr("CombatState: CombatSystem reference is null!");
                return;
            }

            _recoveryTimer = new Timer
            {
                Name = "CombatRecoveryTimer",
                OneShot = true,
                WaitTime = 0.2f
            };
            _recoveryTimer.Timeout += OnRecoveryComplete;
            StateMachine.Character.CallDeferred(Node.MethodName.AddChild, _recoveryTimer);

            _isInitialized = true;
            GD.Print("CombatState initialized successfully");
        }

        public override void Enter()
        {
            if (!_isInitialized)
            {
                GD.PrintErr("Combat state not properly initialized!");
                StateMachine.ChangeState("idle");
                return;
            }

            base.Enter();
            GD.Print("Entering combat state, triggering initial attack");
            _isTransitioning = false;
            _attackTriggeredThisFrame = true;
            _lastDirection = GetMovementInput();
            
            if (_lastDirection.X != 0)
            {
                UpdateFacingDirection(_lastDirection.X);
            }

            // Trigger initial attack
            TriggerAttack();
        }

        public override void HandleInput(InputEvent @event)
        {
            if (_isTransitioning) 
            {
                GD.Print("Combat state: Ignoring input while transitioning");
                return;
            }

            if (@event.IsActionPressed("attack") && !_attackTriggeredThisFrame)
            {
                GD.Print("Combat state: Attack button pressed");
                _attackTriggeredThisFrame = true;
                TriggerAttack();
                return;
            }

            // Handle movement input
            if (@event is InputEventKey)
            {
                Vector2 newDirection = GetMovementInput();
                if (newDirection.X != 0)
                {
                    _lastDirection = newDirection;
                    UpdateFacingDirection(newDirection.X);
                }
            }
        }

        private void TriggerAttack()
        {
            GD.Print("Combat state: Triggering attack");
            StateMachine.CombatSystem.HandleAttackInput();
        }

        public override void PhysicsUpdate(double delta)
        {
            if (!_isInitialized) return;

            var character = StateMachine.Character;
            var velocity = character.Velocity;

            // Apply friction
            velocity.X = Mathf.MoveToward(velocity.X, 0, COMBAT_FRICTION * (float)delta);

            if (!IsOnFloor())
            {
                velocity.Y += GetGravity() * (float)delta;
            }

            UpdateVelocity(velocity);

            // Check combat system state
            if (!StateMachine.CombatSystem.IsAttacking && !_isTransitioning)
            {
                GD.Print("Combat state: Attack finished, starting recovery");
                StartRecovery();
            }
            
            // Handle falling
            if (!IsOnFloor() && velocity.Y > 0 && !StateMachine.CombatSystem.IsAttacking)
            {
                StateMachine.ChangeState("falling");
            }

            // Reset the attack trigger flag at the end of the physics frame
            _attackTriggeredThisFrame = false;
        }

        private void StartRecovery()
        {
            if (!_isInitialized || _recoveryTimer == null) return;

            GD.Print("Combat state: Starting recovery");
            _isTransitioning = true;
            if (_recoveryTimer.IsInsideTree())
            {
                _recoveryTimer.Start();
            }
        }

        private void OnRecoveryComplete()
        {
            if (!_isInitialized) return;

            GD.Print("Combat state: Recovery complete");
            if (IsOnFloor())
            {
                Vector2 input = GetMovementInput();
                StateMachine.ChangeState(input.X != 0 ? "moving" : "idle");
            }
            else
            {
                StateMachine.ChangeState("falling");
            }
        }

        public override void Exit()
        {
            GD.Print("Combat state: Exiting");
            if (_isInitialized && _recoveryTimer != null && _recoveryTimer.IsInsideTree())
            {
                _recoveryTimer.Stop();
            }
            _isTransitioning = false;
            _attackTriggeredThisFrame = false;
            base.Exit();
        }

        public override void Update(double delta)
        {
            // Process unhandled inputs during Update only if we haven't already triggered an attack
            if (Input.IsActionJustPressed("attack") && !_isTransitioning && !_attackTriggeredThisFrame)
            {
                GD.Print("Combat state: Attack detected during Update");
                _attackTriggeredThisFrame = true;
                TriggerAttack();
            }
            
            base.Update(delta);
        }
    }
}