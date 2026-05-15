using System;
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Utils
{
    public class EnableMovementTriggered
    {
        public static EnableMovementTriggered instance;
    }

    public abstract class EnableMovementButtonBase : MonoBehaviour, IEventProvider<EnableMovementTriggered>
    {
        public abstract bool CanBeMoved { get; set; }

        public abstract event Action<EnableMovementTriggered> ProvidedEvent;
    }

    public class EnableMovementButton : EnableMovementButtonBase
    {
        public override event Action<EnableMovementTriggered> ProvidedEvent;
        public override bool CanBeMoved { get; set; }

        public void DispatchTheEvent()
        {
            //Leaving it like this for now, in the future it is possible, that this will change e.g. by introducing fields to EnableMovementTriggered
            ProvidedEvent?.Invoke(EnableMovementTriggered.instance);
        }
    }
}
