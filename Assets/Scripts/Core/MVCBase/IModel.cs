using System;

public interface IModel
{
    event Action OnModelChanged;
    void Initialize();
    void Cleanup();
}
