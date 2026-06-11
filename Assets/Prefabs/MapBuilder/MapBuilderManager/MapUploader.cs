#nullable enable
using UnityEngine;
using System.Threading.Tasks;
using Assets.Prefabs.MapBuilder;
using Assets.Prefabs.MapBuilder.Serialization;
using Assets.Prefabs.MapBuilder.ServerInteraction;
using Newtonsoft.Json;
using Assets.Prefabs.MapBuilder.Inventory;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    public enum UploadMapResult
    {
        SUCCESS,
        SERVER_UNREACHABLE,
        UPLOAD_FAILED,
        MISSING_DRIVER,
        SERIALIZATION_ERROR,
        MAP_NAME_TAKEN,
        USER_NOT_FOUND,
        MAP_UPDATED,
        MISSING_REQUIRED_ITEMS
    }

    public static class UploadMapResultHelper
    {
        public static string GetMessage(UploadMapResult result, string mapName)
        {
            return result switch
            {
                UploadMapResult.SUCCESS => $"Map '{mapName}' uploaded successfully!",
                UploadMapResult.MAP_UPDATED => $"Map '{mapName}' updated successfully!",
                UploadMapResult.SERVER_UNREACHABLE => "Server unreachable or network error occurred while uploading.",
                UploadMapResult.UPLOAD_FAILED => $"Failed to upload map '{mapName}'. The server rejected the request.",
                UploadMapResult.MISSING_DRIVER => "MapUploader error: Server Driver is not assigned.",
                UploadMapResult.SERIALIZATION_ERROR => $"Failed to serialize map '{mapName}' data.",
                UploadMapResult.MAP_NAME_TAKEN => $"The name '{mapName}' is already taken. Please choose another.",
                UploadMapResult.USER_NOT_FOUND => "Upload failed: Your user nick was not found in the database. Please run the seed command.",
                UploadMapResult.MISSING_REQUIRED_ITEMS => "Upload failed: Your map must contain exactly one Vehicle Builder and one Finish Line.",
                _ => "An unknown error occurred during map upload."
            };
        }

        public static void PrintResult(UploadMapResult result, string mapName)
        {
            string message = GetMessage(result, mapName);

            switch (result)
            {
                case UploadMapResult.SUCCESS:
                case UploadMapResult.MAP_UPDATED:
                    Debug.Log(message);
                    break;
                case UploadMapResult.MAP_NAME_TAKEN:
                case UploadMapResult.MISSING_REQUIRED_ITEMS:
                    // NOTE: This is information for the user, not a system error
                    Debug.Log(message);
                    break;
                default:
                    Debug.LogError($"Variant {result} message: {message}");
                    break;
            }
        }
    }

    [CreateAssetMenu(fileName = "MapUploader", menuName = "ScriptableObjects/MapUploader")]
    public class MapUploader : ScriptableObject
    {
        [SerializeField] private UnityServerDriver? serverDriver;
        [SerializeField] private string defaultNick = "Walrus";

        void Awake()
        {
            Debug.Assert(serverDriver is not null);
        }

        public async Task<UploadMapResult> UploadMap(string mapName, IMapStateDTO dto)
        {
            if (serverDriver == null)
            {
                return UploadMapResult.MISSING_DRIVER;
            }

            string json;
            try
            {
                json = JsonConvert.SerializeObject(dto, SerializationManager.GetSettings());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Serialization failed: {e.Message}");
                return UploadMapResult.SERIALIZATION_ERROR;
            }

            try
            {
                ServerActionResult result = await serverDriver.UploadMapAsync(mapName, json);

                return result switch
                {
                    ServerActionResult.SUCCESS => UploadMapResult.SUCCESS,
                    ServerActionResult.ALREADY_EXISTS => UploadMapResult.MAP_NAME_TAKEN,
                    ServerActionResult.USER_NOT_FOUND => UploadMapResult.USER_NOT_FOUND,
                    ServerActionResult.SERVER_UNREACHABLE => UploadMapResult.SERVER_UNREACHABLE,
                    _ => UploadMapResult.UPLOAD_FAILED
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UploadMap Exception: {e.Message}");
                return UploadMapResult.SERVER_UNREACHABLE;
            }
        }

        public async Task<UploadMapResult> UpdateExistingMap(string mapName, IMapStateDTO dto)
        {
            if (serverDriver == null) return UploadMapResult.MISSING_DRIVER;

            string json;
            try
            {
                json = JsonConvert.SerializeObject(dto, SerializationManager.GetSettings());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Serialization failed: {e.Message}");
                return UploadMapResult.SERIALIZATION_ERROR;
            }

            try
            {
                ServerActionResult result = await serverDriver.UpdateMapFileAsync(mapName, json);

                return result switch
                {
                    ServerActionResult.SUCCESS => UploadMapResult.MAP_UPDATED,
                    ServerActionResult.SERVER_UNREACHABLE => UploadMapResult.SERVER_UNREACHABLE,
                    _ => UploadMapResult.UPLOAD_FAILED
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UpdateMap Exception: {e.Message}");
                return UploadMapResult.SERVER_UNREACHABLE;
            }
        }
    }
}
