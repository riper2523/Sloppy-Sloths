#nullable enable
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Serialization;
using System;

namespace Assets.Prefabs.MapBuilder.ServerInteraction
{
    [Serializable]
    public record OwnerData(string Nick);

    [Serializable]
    public record MapData(string MapName, OwnerData Owner, string FilePath)
    {
        // This will be populated after downloading the file content
        [NonSerialized] public IMapStateDTO? MapStateDTO;
    }

    // Wrappers for Fastify responses
    [Serializable]
    public class GetMapsResponse
    {
        public MapData[] maps = Array.Empty<MapData>();
        public string? errMsg;
    }

    [Serializable]
    public class GetMapResponse
    {
        public MapData? map;
        public string? errMsg;
    }

    [Serializable]
    public class ActionResponse
    {
        public string? msg;
        public string? errMsg;
    }
}

// Shim to allow using 'init' properties and 'record' in older .NET versions (Unity)
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
