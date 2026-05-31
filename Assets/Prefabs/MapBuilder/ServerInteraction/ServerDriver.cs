#nullable enable
using System.Threading.Tasks;

namespace Assets.Prefabs.MapBuilder.ServerInteraction
{
    //Methods of this class return null when server is unreachable
    public interface IServerDriver
    {
        Task<bool> IsServerAliveAsync();
        Task<MapData[]?> GetMapsAsync();
        Task<(ServerActionResult result, MapData? map)> GetMapAsync(string mapName);

        Task<ServerActionResult> ChangeOwnerAsync(MapData map, OwnerData newOwner);
        Task<ServerActionResult> UploadMapAsync(string mapName, string nick, string mapJson);
        Task<ServerActionResult> UpdateMapFileAsync(string mapName, string nick, string mapJson);
    }
}
