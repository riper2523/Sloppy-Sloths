using System;
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Utils
{
    public class SaveTheMapTriggered
    {
        public static SaveTheMapTriggered instance = new();
    }

    public abstract class SaveTheMapButtonBase : MonoBehaviour, IEventProvider<SaveTheMapTriggered>
    {
        public abstract bool CanBeSaved { get; set; }

        public abstract event Action<SaveTheMapTriggered> ProvidedEvent;
    }

    public class SaveTheMapButton : SaveTheMapButtonBase
    {
        public override event Action<SaveTheMapTriggered> ProvidedEvent;
        public override bool CanBeSaved { get; set; }

        public void DispatchTheEvent()
        {
            //Leaving it like this for now, in the future it is possible, that this will change e.g. by introducing fields to EnableMovementTriggered
            ProvidedEvent?.Invoke(SaveTheMapTriggered.instance);
        }
    }
}
