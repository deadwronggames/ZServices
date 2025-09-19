namespace DeadWrongGames.ZServices.Time
{
    public interface IBaseUpdatable { }

    public interface IUpdatable : IBaseUpdatable
    {
        void OnUpdate();
    }

    public interface IFixedUpdatable : IBaseUpdatable
    {
        void OnFixedUpdate();
    }

    public interface ILateUpdatable : IBaseUpdatable
    {
        void OnLateUpdate();
    }
}
