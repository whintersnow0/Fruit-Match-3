using UnityEngine;
using System;

public abstract class BaseModel : IModel
{
    public event Action OnModelChanged;

    public virtual void Initialize() { }
    public virtual void Cleanup() { }

    protected void NotifyModelChanged()
    {
        OnModelChanged?.Invoke();
    }
}