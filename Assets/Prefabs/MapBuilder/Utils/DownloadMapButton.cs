using System;
using UnityEngine;

namespace Assets.Prefabs.MapBuilder.Utils
{
    public class DownloadMapTriggered
    {
        public static DownloadMapTriggered instance = new();
    }

    public abstract class DownloadMapButtonBase : MonoBehaviour, IEventProvider<DownloadMapTriggered>
    {
        public abstract bool CanBeSaved { get; set; }

        public abstract event Action<DownloadMapTriggered> ProvidedEvent;
    }

    public class DownloadMapButton : DownloadMapButtonBase
    {
        public override event Action<DownloadMapTriggered> ProvidedEvent;
        public override bool CanBeSaved { get; set; }

        public void DispatchTheEvent()
        {
            ProvidedEvent?.Invoke(DownloadMapTriggered.instance);
        }
    }
}
