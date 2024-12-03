using Godot;
using System.Collections.Generic;
using CSharp2DCharacterController.Scenes.Interfaces.Core;
using CSharp2DCharacterController.Scenes.Interfaces.Combat;
using CSharp2DCharacterController.Scenes.StateMachine.States;

namespace CSharp2DCharacterController.Scenes.StateMachine
{
    [GlobalClass]
    [Tool]
    public partial class CharacterStateMachine : Node, IStateMachine, ICharacterStateMachine
    {
        [Signal]
        public delegate void StateChangedEventHandler(string newState);

        private Dictionary<string, IState> _states = new Dictionary<string, IState>();
        private IState _currentState;
        private string _currentStateName;
        private bool _isInitialized;
        private bool _isChangingState = false;

        public CharacterBody2D Character { get; private set; }
        public AnimatedSprite2D Sprite { get; private set; }
        public ICombatSystem CombatSystem { get; private set; }
        public string CurrentState => _currentStateName;

        public override void _Ready()
        {
            if (!Engine.IsEditorHint())
            {
                CallDeferred(nameof(Initialize));
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                GD.Print("StateMachine already initialized, skipping");
                return;
            }

            Character = GetParent<CharacterBody2D>();
            if (Character == null)
            {
                GD.PrintErr("CharacterStateMachine: Failed to get CharacterBody2D parent!");
                return;
            }

            Sprite = Character.GetNode<AnimatedSprite2D>("Movement");
            if (Sprite == null)
            {
                GD.PrintErr("CharacterStateMachine: Failed to get AnimatedSprite2D!");
                return;
            }

            CombatSystem = Character.GetNode<ICombatSystem>("CombatSystem");
            if (CombatSystem == null)
            {
                GD.PrintErr("CharacterStateMachine: Failed to get CombatSystem!");
                return;
            }

            // Register and initialize states
            _states.Clear();
            RegisterInitialStates();
            InitializeStates();

            _isInitialized = true;
            GD.Print("StateMachine initialized successfully");

            // Set initial state
            ChangeState("idle");
        }

        private void RegisterInitialStates()
        {
            RegisterState("idle", new IdleState(this));
            RegisterState("moving", new MovingState(this));
            RegisterState("jumping", new JumpingState(this));
            RegisterState("falling", new FallingState(this));
            RegisterState("combat", new CombatState(this));
        }

        private void InitializeStates()
        {
            foreach (var state in _states.Values)
            {
                state.Initialize();
            }
        }

        // Make this public to satisfy the interface
        public void RegisterState(string name, IState state)
        {
            if (state == null)
            {
                GD.PrintErr($"Attempting to register null state for {name}");
                return;
            }

            _states[name] = state;
            GD.Print($"Registered state: {name}");
        }

        public void ChangeState(string newState)
        {
            if (!_isInitialized)
            {
                GD.PrintErr($"Attempting to change to state {newState} before initialization!");
                return;
            }

            if (_isChangingState)
            {
                GD.PrintErr($"Recursive state change detected for {newState}, ignoring!");
                return;
            }

            if (!_states.ContainsKey(newState))
            {
                GD.PrintErr($"State {newState} not found!");
                return;
            }

            if (_currentStateName == newState)
            {
                return;
            }

            try
            {
                _isChangingState = true;
                var oldState = _currentStateName;

                _currentState?.Exit();
                _currentStateName = newState;
                _currentState = _states[newState];
                _currentState.Enter();

                EmitSignal(SignalName.StateChanged, newState);
                GD.Print($"State change: {oldState} -> {newState}");
            }
            finally
            {
                _isChangingState = false;
            }
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint() || !_isInitialized) return;
            _currentState?.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Engine.IsEditorHint() || !_isInitialized) return;
            _currentState?.PhysicsUpdate(delta);
        }

        public override void _Input(InputEvent @event)
        {
            if (Engine.IsEditorHint() || !_isInitialized) return;
            _currentState?.HandleInput(@event);
        }

        public string GetCurrentState() => _currentStateName;
    }
}