namespace CSharp2DCharacterController.Scenes.Interfaces.Core
{
    public interface IStateMachine : ISystem
    {
        void RegisterState(string name, IState state);
        void ChangeState(string newState);
        string GetCurrentState();
    }
}