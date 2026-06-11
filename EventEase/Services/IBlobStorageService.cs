using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EventEase.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task DeleteImageAsync(string imageUrl, string containerName);
        Task<bool> ImageExistsAsync(string imageUrl);
        string GetDefaultImageUrl(string type);
        Task<bool> TestConnectionAsync();
    }
}