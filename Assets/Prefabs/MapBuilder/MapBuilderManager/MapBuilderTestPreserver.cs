#nullable enable
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    public static class MapBuilderTestPreserver
    {
        public static bool IsTesting { get; set; } = false;
        public static IMapStateDTO? SavedState { get; set; }
        public static string? SavedMapName { get; set; }
    }
}
