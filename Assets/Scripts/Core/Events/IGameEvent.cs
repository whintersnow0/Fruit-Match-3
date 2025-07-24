public interface IGameEvent { }

public interface IEventListener { }

public interface IEventListener<T> : IEventListener where T : IGameEvent
{
    void HandleEvent(T gameEvent);
}