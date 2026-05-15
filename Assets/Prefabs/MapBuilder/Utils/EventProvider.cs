using System;

namespace Assets.Prefabs.MapBuilder.Utils
{
    interface IEventProvider<T>
    {
        event Action<T> ProvidedEvent;
    }
}
