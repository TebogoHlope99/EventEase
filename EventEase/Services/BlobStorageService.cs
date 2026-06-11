using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventEase.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            IConfiguration configuration,
            ILogger<BlobStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Try multiple possible configuration keys
            var connectionString = _configuration["AzureStorage:ConnectionString"]
                ?? _configuration["AzureStorageConnectionString"]
                ?? _configuration["ConnectionStrings:AzureStorage"]
                ?? Environment.GetEnvironmentVariable("AzureStorage__ConnectionString");

            _logger.LogInformation($"Connection string found: {(string.IsNullOrEmpty(connectionString) ? "NO" : "YES")}");

            if (!string.IsNullOrEmpty(connectionString))
            {
                _logger.LogInformation($"Connection string starts with: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Azure Storage connection string is missing! Please check configuration.");
                throw new InvalidOperationException("Azure Storage connection string not configured. Please add 'AzureStorage:ConnectionString' to your configuration.");
            }

            // Validate connection string format (should not contain localhost or 127.0.0.1)
            if (connectionString.Contains("127.0.0.1") || connectionString.Contains("localhost") || connectionString.Contains("UseDevelopmentStorage=true"))
            {
                _logger.LogError("Connection string appears to be using local storage emulator. Please use your actual Azure Storage connection string.");
                throw new InvalidOperationException("Invalid connection string: Using local storage emulator. Please use your Azure Storage connection string from the Azure Portal.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger.LogInformation("BlobStorageService initialized successfully with Azure Storage.");
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create container if it doesn't exist
            try
            {
                await containerClient.CreateIfNotExistsAsync();
                await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);
                _logger.LogInformation($"Container '{containerName}' ready (created or already exists)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accessing container '{containerName}'");
                throw new Exception($"Cannot access container '{containerName}'. Please check your storage account permissions.", ex);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Upload file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            _logger.LogInformation($"Image uploaded: {blobClient.Uri}");
            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl, string containerName)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"Image deleted: {imageUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to delete image: {imageUrl}");
            }
        }

        public async Task<bool> ImageExistsAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            try
            {
                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);
                var containerName = uri.Segments[1].TrimEnd('/');
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        public string GetDefaultImageUrl(string type)
        {
            return type switch
            {
                "venue" => "https://placehold.co/600x400/3b82f6/white?text=Venue",
                "event" => "https://placehold.co/600x400/8b5cf6/white?text=Event",
                _ => "https://placehold.co/600x400/cccccc/white?text=No+Image"
            };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing Blob Storage connection...");
                var containerClient = _blobServiceClient.GetBlobContainerClient("test-container");
                var response = await containerClient.ExistsAsync();
                _logger.LogInformation($"Connection test result: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
                return false;
            }
        }
    }
}