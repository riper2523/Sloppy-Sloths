using System.Collections;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Prefabs.MapBuilder.ServerInteraction;

namespace Assets.Prefabs.MapBuilder.ServerInteraction.Tests
{
    public class ServerDriverTests
    {
        private UnityServerDriver _driver;

        [OneTimeSetUp]
        public void Setup()
        {
            _driver = ScriptableObject.CreateInstance<UnityServerDriver>();
            _driver.ConfigFileName = "test_server_config.json";
        }

        [UnityTest]
        public IEnumerator GetMaps_ReturnsArray() => RunAsync(async () =>
        {
            var maps = await _driver.GetMapsAsync();
            Assert.IsNotNull(maps, "Server should return at least an empty array");
        });

        [UnityTest]
        public IEnumerator FullUploadAndFetchFlow() => RunAsync(async () =>
        {
            string testMap = "Test-Map-" + System.Guid.NewGuid().ToString().Substring(0, 5);
            string dummyJson = "{\"Type\": 0, \"NodeContainerDTOs\": []}";

            ServerActionResult uploadResult = await _driver.UploadMapAsync(testMap, dummyJson);
            Assert.AreEqual(ServerActionResult.SUCCESS, uploadResult, "Upload should succeed");

            var (fetchResult, fetched) = await _driver.GetMapAsync(testMap);
            Assert.AreEqual(ServerActionResult.SUCCESS, fetchResult);
            Assert.IsNotNull(fetched, "Should be able to fetch the uploaded map");
            Assert.AreEqual(testMap, fetched.MapName);
        });

        [UnityTest]
        public IEnumerator GetMap_NonExistent_ReturnsNull() => RunAsync(async () =>
        {

            var (result, map) = await _driver.GetMapAsync("DoesNotExist_" + System.Guid.NewGuid());
            Assert.AreEqual(ServerActionResult.USER_NOT_FOUND, result, "Should return USER_NOT_FOUND for non-existent maps");
            Assert.IsNull(map, "Should return null for non-existent maps");
        });

        [UnityTest]
        public IEnumerator UploadMap_DuplicateName_ReturnsFalse() => RunAsync(async () =>
        {
            string testMap = "Duplicate-Test-" + System.Guid.NewGuid().ToString().Substring(0, 5);
            string dummyJson = "{\"Type\": 0, \"NodeContainerDTOs\": []}";

            await _driver.UploadMapAsync(testMap, dummyJson);


            ServerActionResult second = await _driver.UploadMapAsync(testMap, dummyJson);
            Assert.AreEqual(ServerActionResult.ALREADY_EXISTS, second, "Second upload with same name should fail with ALREADY_EXISTS");
        });

        [UnityTest]
        public IEnumerator ChangeOwner_Flow() => RunAsync(async () =>
        {
            string testMap = "Owner-Test-" + System.Guid.NewGuid().ToString().Substring(0, 5);
            string dummyJson = "{\"Type\": 0, \"NodeContainerDTOs\": []}";

            await _driver.UploadMapAsync(testMap, dummyJson);
            var (_, map) = await _driver.GetMapAsync(testMap);
            Assert.IsNotEmpty(map.Owner.Nick);

            ServerActionResult changed = await _driver.ChangeOwnerAsync(map!, new OwnerData("AnonymousSloth"));
            Assert.AreEqual(ServerActionResult.SUCCESS, changed, "Ownership change should succeed");

            var (_, updatedMap) = await _driver.GetMapAsync(testMap);
            Assert.AreEqual("AnonymousSloth", updatedMap!.Owner.Nick, "Owner should be updated to AnonymousSloth");
        });

        [UnityTest]
        public IEnumerator UploadMap_InvalidName_ReturnsFalse() => RunAsync(async () =>
        {
            // "Invalid.Name" contains a dot, which is URL-safe but fails our regex: ^[a-zA-Z0-9_-]+$
            string invalidName = "Invalid.Name";
            string dummyJson = "{\"Type\": 0, \"NodeContainerDTOs\": []}";


            ServerActionResult result = await _driver.UploadMapAsync(invalidName, dummyJson);
            Assert.AreEqual(ServerActionResult.FAILED, result, "Server should reject names containing dots with FAILED");
        });

        private IEnumerator RunAsync(System.Func<Task> asyncAction)
        {
            var task = asyncAction();
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception.InnerException;
            }
        }
    }
}
