#nullable enable
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Assets.Prefabs.MapBuilder.Serialization;

namespace Assets.Prefabs.MapBuilder.ServerInteraction
{
    [CreateAssetMenu(fileName = "ServerDriver", menuName = "ScriptableObjects/ServerDriver")]
    public class UnityServerDriver : ScriptableObject, IServerDriver
    {
        [SerializeField] private string serverBaseUrl = "http://127.0.0.1:3000";
        [SerializeField] private int requestTimeout = 5; // 5 seconds

        public async Task<bool> IsServerAliveAsync()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get($"{serverBaseUrl}/status");
            var (result, _) = await ExecuteRequestAsync(webRequest);
            return result == ServerActionResult.SUCCESS;
        }

        /// <summary>
        /// Helper to await UnityWebRequest and return a result enum.
        /// </summary>
        private async Task<(ServerActionResult, string?)> ExecuteRequestAsync(UnityWebRequest webRequest)
        {
            webRequest.timeout = requestTimeout;
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return (ServerActionResult.SUCCESS, webRequest.downloadHandler.text);
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                return (ServerActionResult.SERVER_UNREACHABLE, null);
            }

            if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                string? serverError = null;
                if (webRequest.downloadHandler != null && !string.IsNullOrEmpty(webRequest.downloadHandler.text))
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<ActionResponse>(webRequest.downloadHandler.text);
                        serverError = response?.errMsg;
                    }
                    catch { /* Not a JSON error response */ }
                }

                return webRequest.responseCode switch
                {
                    409 => (ServerActionResult.ALREADY_EXISTS, serverError),
                    400 => (ServerActionResult.FAILED, serverError),
                    403 => (ServerActionResult.UNAUTHORIZED, serverError),
                    404 => (ServerActionResult.USER_NOT_FOUND, serverError),
                    _ => LogAndReturnFailed(webRequest, serverError)
                };
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                return (ServerActionResult.FAILED, webRequest.error);
            }

            return (ServerActionResult.SERVER_UNREACHABLE, null);
        }

        private (ServerActionResult, string?) LogAndReturnFailed(UnityWebRequest webRequest, string? serverError)
        {
            Debug.LogError($"ServerDriver: {webRequest.method} {webRequest.url} failed: {webRequest.error} (Code: {webRequest.responseCode}). Server Message: {serverError ?? "None"}");
            return (ServerActionResult.FAILED, serverError);
        }

        public async Task<MapData[]?> GetMapsAsync()
        {
            string url = $"{serverBaseUrl}/maps/";
            using var webRequest = UnityWebRequest.Get(url);

            (var result, var json) = await ExecuteRequestAsync(webRequest);
            if (result != ServerActionResult.SUCCESS) return null;
            if (string.IsNullOrEmpty(json)) return Array.Empty<MapData>();

            try
            {
                var response = JsonConvert.DeserializeObject<GetMapsResponse>(json);
                return response?.maps ?? Array.Empty<MapData>();
            }
            catch (Exception e)
            {
                Debug.LogError($"ServerDriver: Failed to parse maps list: {e.Message}");
                return null;
            }
        }

        public async Task<(ServerActionResult result, MapData? map)> GetMapAsync(string mapName)
        {
            // 1. Fetch Metadata
            string metadataUrl = $"{serverBaseUrl}/maps/{mapName}";
            using var metadataRequest = UnityWebRequest.Get(metadataUrl);

            (var metaResult, var metadataJson) = await ExecuteRequestAsync(metadataRequest);
            if (metaResult != ServerActionResult.SUCCESS || string.IsNullOrEmpty(metadataJson)) return (metaResult, null);

            MapData? mapData;
            try
            {
                var response = JsonConvert.DeserializeObject<GetMapResponse>(metadataJson);
                mapData = response?.map;
            }
            catch (Exception e)
            {
                Debug.LogError($"ServerDriver: Failed to parse map metadata: {e.Message}");
                return (ServerActionResult.FAILED, null);
            }

            if (mapData == null) return (ServerActionResult.FAILED, null);

            // 2. Fetch File Content (IMapStateDTO)
            string fileUrl = $"{serverBaseUrl}/maps/{mapName}/file";
            using var fileRequest = UnityWebRequest.Get(fileUrl);

            (var fileResult, var fileJson) = await ExecuteRequestAsync(fileRequest);

            if (fileResult == ServerActionResult.SUCCESS && !string.IsNullOrEmpty(fileJson))
            {
                try
                {
                    var settings = SerializationManager.GetSettings();
                    mapData.MapStateDTO = JsonConvert.DeserializeObject<IMapStateDTO>(fileJson, settings);
                }
                catch (Exception e)
                {
                    Debug.LogError($"ServerDriver: Failed to parse map file content: {e.Message}");
                }
            }

            return (ServerActionResult.SUCCESS, mapData);
        }

        public async Task<ServerActionResult> ChangeOwnerAsync(MapData map, OwnerData newOwner)
        {
            string url = $"{serverBaseUrl}/maps/{map.MapName}/owner";
            var body = new { newOwnerNick = newOwner.Nick };
            string jsonBody = JsonConvert.SerializeObject(body);

            using var webRequest = new UnityWebRequest(url, "PATCH");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            (var result, var responseJson) = await ExecuteRequestAsync(webRequest);
            return result;
        }

        public async Task<ServerActionResult> UploadMapAsync(string mapName, string nick, string mapJson)
        {
            string url = $"{serverBaseUrl}/maps/{mapName}";

            List<IMultipartFormSection> formData = new()
            {
                new MultipartFormDataSection("nick", nick),
                new MultipartFormFileSection("mapFile", System.Text.Encoding.UTF8.GetBytes(mapJson), $"{mapName}.map", "application/json")
            };

            using var webRequest = UnityWebRequest.Post(url, formData);
            (var result, _) = await ExecuteRequestAsync(webRequest);
            return result;
        }

        public async Task<ServerActionResult> UpdateMapFileAsync(string mapName, string nick, string mapJson)
        {
            string url = $"{serverBaseUrl}/maps/{mapName}/file";

            List<IMultipartFormSection> formData = new()
            {
                new MultipartFormDataSection("nick", nick),
                new MultipartFormFileSection("mapFile", System.Text.Encoding.UTF8.GetBytes(mapJson), $"{mapName}.map", "application/json")
            };

            using var webRequest = UnityWebRequest.Post(url, formData);
            webRequest.method = "PATCH";

            (var result, _) = await ExecuteRequestAsync(webRequest);
            return result;
        }
    }
}
