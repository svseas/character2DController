using Godot;

namespace CSharp2DCharacterController.Scenes.Interfaces.Core
{
    public interface IState : ISystem
    {
        void Enter();
        void Exit();
        void Update(double delta);
        void PhysicsUpdate(double delta);
        void HandleInput(InputEvent @event);
    }
}