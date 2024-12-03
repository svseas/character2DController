namespace CSharp2DCharacterController.Scenes.Interfaces.Combat
{
    public interface ICombatSystem : Core.ISystem
    {
        void HandleAttackInput();
        new void Initialize();
        bool IsAttacking { get; }
    }
}