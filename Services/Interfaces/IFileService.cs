using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TheBugTracker.Services.Interfaces
{
    public interface IFileService
    {
        Task<byte[]> ConverFileToByteArrayAsync(IFormFile file);
        string ConverByteArrayToFile(byte[] fileData, string extension);
        string GetFileIcon(string file);
        string FormatFileSize(long bytes);
    }
}
