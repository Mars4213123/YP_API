namespace YP_API.Services
{
    public interface IImageService
    {
        /// <summary>
        /// Ищет изображение по ключевому слову.
        /// Возвращает URL картинки или заглушку, если ничего не найдено.
        /// </summary>
        Task<string> GetImageAsync(string query);
    }
}
