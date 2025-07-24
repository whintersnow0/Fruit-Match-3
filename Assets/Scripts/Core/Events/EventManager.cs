using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    public static EventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EventManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("EventManager");
                    instance = go.AddComponent<EventManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private Dictionary<Type, List<IEventListener>> eventListeners = new Dictionary<Type, List<IEventListener>>();

    public void Subscribe<T>(IEventListener<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (!eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType] = new List<IEventListener>();
        }

        eventListeners[eventType].Add(listener);
    }

    public void Unsubscribe<T>(IEventListener<T> listener) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Remove(listener);
        }
    }

    public void Publish<T>(T gameEvent) where T : IGameEvent
    {
        Type eventType = typeof(T);

        if (eventListeners.ContainsKey(eventType))
        {
            foreach (var listener in eventListeners[eventType])
            {
                if (listener is IEventListener<T> typedListener)
                {
                    typedListener.HandleEvent(gameEvent);
                }
            }
        }
    }
}
