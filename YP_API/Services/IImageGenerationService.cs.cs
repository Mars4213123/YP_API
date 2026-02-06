namespace YP_API.Services
{
    public interface IImageGenerationService
    {
        Task<string> GenerateImageUrlAsync(string prompt);
    }
}
