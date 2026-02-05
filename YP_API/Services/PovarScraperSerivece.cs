using HtmlAgilityPack;

namespace YP_API.Services
{
    public interface IPovarScraperService
    {
        Task<string?> FindImageAsync(string productName);
    }

    public class PovarScraperService : IPovarScraperService
    {
        private readonly HttpClient _httpClient;

        public PovarScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Povar.ru может блокировать запросы без User-Agent
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<string?> FindImageAsync(string productName)
        {
            try
            {
                // 1. Формируем URL поиска (как в форме на сайте: action="/xmlsearch")
                string searchUrl = $"https://povar.ru/xmlsearch?query={Uri.EscapeDataString(productName)}";

                // 2. Получаем HTML страницы результатов
                var html = await _httpClient.GetStringAsync(searchUrl);

                // 3. Загружаем HTML в парсер
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 4. Ищем ПЕРВЫЙ блок рецепта (.recipe)
                // XPath логика: Найти div с классом 'recipe', внутри него найти span с классом 'thumb', внутри него img
                var imgNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'recipe')]//span[contains(@class, 'thumb')]/img");

                if (imgNode != null)
                {
                    // Получаем атрибут src
                    string src = imgNode.GetAttributeValue("src", null);

                    // Если ссылка относительная (начинается с /), добавляем домен
                    if (!string.IsNullOrEmpty(src) && src.StartsWith("/"))
                    {
                        return $"https://povar.ru{src}";
                    }

                    return src;
                }

                return null; // Ничего не нашли
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка парсинга Povar.ru: {ex.Message}");
                return null;
            }
        }
    }
}