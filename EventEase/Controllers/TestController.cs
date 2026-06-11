using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class TestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBlobStorageService _blobService;

        public TestController(IConfiguration configuration, IBlobStorageService blobService)
        {
            _configuration = configuration;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var results = new List<string>();

            // Test 1: Check Configuration
            var connString = _configuration["AzureStorage:ConnectionString"];
            results.Add($"1. Connection String Present: {(string.IsNullOrEmpty(connString) ? "NO ❌" : "YES ✅")}");

            if (!string.IsNullOrEmpty(connString))
            {
                results.Add($"   Length: {connString.Length} characters");
                results.Add($"   Starts with: {connString.Substring(0, Math.Min(50, connString.Length))}...");
            }

            // Test 2: Check Container Names
            var venueContainer = _configuration["AzureStorage:VenueContainerName"] ?? "venue-images";
            var eventContainer = _configuration["AzureStorage:EventContainerName"] ?? "event-images";
            results.Add($"2. Venue Container Name: {venueContainer}");
            results.Add($"3. Event Container Name: {eventContainer}");

            // Test 3: Test Blob Service Connection
            try
            {
                var testResult = await _blobService.TestConnectionAsync();
                results.Add($"4. Blob Service Connection: {(testResult ? "Working ✅" : "Failed ❌")}");
            }
            catch (Exception ex)
            {
                results.Add($"4. Blob Service Error: {ex.Message}");
            }

            // Test 4: Try to list containers
            try
            {
                var blobServiceClient = new BlobServiceClient(connString);
                var containers = blobServiceClient.GetBlobContainers().Take(5).ToList();
                results.Add($"5. Found {containers.Count} container(s) in storage account");
                foreach (var container in containers)
                {
                    results.Add($"   - {container.Name}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"5. Error listing containers: {ex.Message}");
            }

            ViewBag.Results = results;
            return View();
        }
    }
}