using Godot;
using CSharp2DCharacterController.Scenes.Interfaces.Combat;

namespace CSharp2DCharacterController.Scenes.Combat
{
    [GlobalClass] 
    public partial class CombatSystem : Node, ICombatSystem
    {
        private enum AttackState
        {
            None,
            Attacking,
            ComboWindow
        }

        // Constants
        private const int MAX_COMBO = 3;
        private const string ATTACK_ANIMATION_PREFIX = "atk_";
        
        // Exported variables
        [Export] private float ComboWindowDuration = 0.2f;
        [Export] private NodePath SpritePath = "Movement";
        
        // Private fields
        private AttackState _currentState = AttackState.None;
        private int _currentCombo = 0;
        private bool _isAnimationPlaying;
        private Timer _comboWindowTimer;
        private bool _attackQueued;
        private AnimatedSprite2D _sprite;
        private bool _isInitialized;

        // Public properties
        public bool IsAttacking => _currentState == AttackState.Attacking;
        public int CurrentCombo => _currentCombo;
        public bool IsComboAvailable => _currentCombo < MAX_COMBO;

        public override void _Ready()
        {
            Initialize();
        }

        public override void _ExitTree()
        {
            CleanupResources();
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            SetupSprite();
            SetupComboTimer();
            
            _isInitialized = true;
        }

        private void SetupSprite()
        {
            _sprite = GetParent().GetNode<AnimatedSprite2D>(SpritePath);
            if (_sprite == null)
            {
                GD.PrintErr($"CombatSystem: Failed to get AnimatedSprite2D at path: {SpritePath}");
                return;
            }

            _sprite.AnimationFinished += OnAnimationFinished;
        }

        private void SetupComboTimer()
        {
            _comboWindowTimer = new Timer
            {
                WaitTime = ComboWindowDuration,
                OneShot = true,
                ProcessMode = ProcessModeEnum.Pausable
            };
            AddChild(_comboWindowTimer);
            _comboWindowTimer.Timeout += OnComboWindowExpired;
        }

        private void CleanupResources()
        {
            if (_sprite != null)
            {
                _sprite.AnimationFinished -= OnAnimationFinished;
            }

            if (_comboWindowTimer != null)
            {
                _comboWindowTimer.Timeout -= OnComboWindowExpired;
                _comboWindowTimer.QueueFree();
            }
        }

        public void HandleAttackInput()
        {
            if (!CanProcessAttack()) return;

            switch (_currentState)
            {
                case AttackState.None:
                    StartAttack(1);
                    break;

                case AttackState.Attacking when IsComboAvailable && !_attackQueued:
                    QueueNextAttack();
                    break;

                case AttackState.ComboWindow when IsComboAvailable:
                    ExecuteQueuedCombo();
                    break;
            }
        }

        private bool CanProcessAttack()
        {
            if (!_isInitialized || _sprite == null) return false;
            if (_currentState == AttackState.ComboWindow && _currentCombo >= MAX_COMBO) return false;
            return true;
        }

        private void StartAttack(int comboNumber)
        {
            _currentState = AttackState.Attacking;
            _currentCombo = comboNumber;
            _isAnimationPlaying = true;
            _attackQueued = false;

            PlayAttackAnimation(comboNumber);
        }

        private void PlayAttackAnimation(int comboNumber)
        {
            string animationName = $"{ATTACK_ANIMATION_PREFIX}{comboNumber}";
            if (_sprite.SpriteFrames.HasAnimation(animationName))
            {
                _sprite.Play(animationName);
            }
            else
            {
                GD.PrintErr($"Animation {animationName} not found!");
                ResetCombo();
            }
        }

        private void QueueNextAttack()
        {
            _attackQueued = true;
        }

        private void ExecuteQueuedCombo()
        {
            _comboWindowTimer.Stop();
            StartNextCombo();
        }

        private void StartNextCombo()
        {
            StartAttack(_currentCombo + 1);
        }

        private void OnAnimationFinished()
        {
            if (!IsAttacking) return;

            string currentAnimation = _sprite.Animation;
            if (currentAnimation.StartsWith(ATTACK_ANIMATION_PREFIX))
            {
                _isAnimationPlaying = false;
                HandlePostAnimation();
            }
        }

        private void HandlePostAnimation()
        {
            if (_attackQueued)
            {
                StartNextCombo();
            }
            else
            {
                StartComboWindow();
            }
        }

        private void StartComboWindow()
        {
            _currentState = AttackState.ComboWindow;
            _comboWindowTimer.Start();
        }

        private void OnComboWindowExpired()
        {
            ResetCombo();
        }

        private void ResetCombo()
        {
            _currentState = AttackState.None;
            _currentCombo = 0;
            _attackQueued = false;
            _isAnimationPlaying = false;
        }

        // Optional: Debug methods
        public string GetDebugInfo()
        {
            return $"State: {_currentState}, Combo: {_currentCombo}, AnimPlaying: {_isAnimationPlaying}, Queued: {_attackQueued}";
        }
    }
}