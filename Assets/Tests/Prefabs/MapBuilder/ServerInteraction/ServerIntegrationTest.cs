#nullable enable
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Prefabs.MapBuilder.ServerInteraction
{
    public class ServerIntegrationTest : MonoBehaviour
    {
        [SerializeField] private UnityServerDriver? serverDriver;
        [SerializeField] private string testMapName = "Test-Map";
        [SerializeField] private string testNick = "Walrus";

        [ContextMenu("Run Full Integration Test")]
        public async void RunTest()
        {
            if (serverDriver == null)
            {
                Debug.LogError("Test: No ServerDriver assigned.");
                return;
            }

            Debug.Log("--- Starting Async Integration Test ---");

            try
            {
                // 1. Get Maps
                var maps = await serverDriver.GetMapsAsync();
                if (maps == null)
                {
                    Debug.LogError("Test: Server unreachable while fetching maps.");
                    return;
                }
                Debug.Log($"Test: Found {maps.Length} maps on server.");

                // 2. Upload a Dummy Map
                string dummyJson = "{\"Type\": 0, \"NodeContainerDTOs\": []}";
                ServerActionResult uploadRes = await serverDriver.UploadMapAsync(testMapName, testNick, dummyJson);
                Debug.Log($"Test: Upload '{testMapName}' Result: {uploadRes}");

                // 3. Get the uploaded map
                var (_, map) = await serverDriver.GetMapAsync(testMapName);
                if (map != null)
                {
                    Debug.Log($"Test: Successfully fetched '{map.MapName}' owned by {map.Owner.Nick}");

                    // 4. Change Owner (Requires 'AnonymousSloth' to exist in DB)
                    ServerActionResult ownerRes = await serverDriver.ChangeOwnerAsync(map, new OwnerData("AnonymousSloth"));
                    Debug.Log($"Test: Change Owner to 'AnonymousSloth' Result: {ownerRes} (Note: Fails if 'AnonymousSloth' doesn't exist)");
                }
                else
                {
                    Debug.LogError($"Test: Failed to fetch '{testMapName}' after upload.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Test: Exception occurred: {e.Message}");
            }

            Debug.Log("--- Integration Test Finished ---");
        }
    }
}
