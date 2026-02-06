using Newtonsoft.Json;
using System.Text;
using YP_API.Models;
using YP_API.Models.AIAPI;
using static YP_API.Models.AIAPI.Responce.ResponceMenu;

namespace YP_API.Helpers
{

    public class GigaChatHelper
    {
        public static string ClientId = "019bca1f-0f50-72b2-b33b-7fb5c3b89be6";
        public static string AuthorizationKey = "MDE5YmNhMWYtMGY1MC03MmIyLWIzM2ItN2ZiNWMzYjg5YmU2OjE2NjQxYWQ0LWVhMjctNDYzYi1hYjRmLTRjZTI4ZDU1NTVkOA==";
        public static async Task<Models.AIAPI.Responce.ResponseMessage> GetAnswer(string token, List<Models.AIAPI.Request.Message> messages)
        {
            Models.AIAPI.Responce.ResponseMessage responseMessage = null;
            string Url = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";
            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
                using (HttpClient client = new HttpClient(Handler))
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    client.DefaultRequestHeaders.Add("X-Client-ID", ClientId);

                    Request DataRequest = new Request()
                    {
                        model = "GigaChat",
                        stream = false,
                        repetition_penalty = 1,
                        messages = messages
                    };

                    string JsonContent = JsonConvert.SerializeObject(DataRequest);
                    using (var content = new StringContent(JsonContent, Encoding.UTF8, "application/json"))
                    {
                        HttpResponseMessage Response = await client.PostAsync(Url, content);

                        if (Response.IsSuccessStatusCode)
                        {
                            string ResponseContent = await Response.Content.ReadAsStringAsync();
                            responseMessage = JsonConvert.DeserializeObject<Models.AIAPI.Responce.ResponseMessage>(ResponseContent);
                        }
                        else
                        {
                            string errorBody = await Response.Content.ReadAsStringAsync();
                            Console.WriteLine($"❌ API ошибка ({Response.StatusCode}): {errorBody}");
                        }
                    }
                }
            }
            return responseMessage;
        }
        public static async Task<GeneratedMenuDto?> GenerateAndParseMenuAsync(string token, List<Ingredient> ingredients, int daysCount)
        {
            var finalMenu = new GeneratedMenuDto
            {
                MenuName = "Сгенерированное меню",
                Items = new List<GeneratedMenuItemDto>()
            };

            // Генерируем каждый день отдельно
            for (int i = 1; i <= daysCount; i++)
            {
                // 1. Промпт теперь просит меню ТОЛЬКО НА 1 ДЕНЬ (день № i)
                string systemPrompt = CreateSingleDayPrompt(ingredients, i);

                var messages = new List<Request.Message>
        {
            new Request.Message { role = "user", content = systemPrompt }
        };

                var response = await GetAnswer(token, messages);
                if (response?.choices?.Count > 0)
                {
                    string json = CleanJson(response.choices[0].message.content);
                    try
                    {
                        // Десериализуем маленькую часть (1 день)
                        var dayMenu = JsonConvert.DeserializeObject<GeneratedMenuDto>(json);
                        if (dayMenu?.Items != null)
                        {
                            finalMenu.Items.AddRange(dayMenu.Items);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка генерации дня {i}: {ex.Message}");
                        // Можно добавить retry (повторную попытку) здесь
                    }
                }
            }

            return finalMenu.Items.Count > 0 ? finalMenu : null;
        }
        public static string CreateSingleDayPrompt(List<Ingredient> ingredients, int dayNumber)
        {
            // Преобразуем список доступных продуктов в строку
            string ingredientsString = string.Join(", ", ingredients.Select(i => i.Name));

            return $@"
Ты — профессиональный шеф-повар с критическим мышлением.
Твоя задача — составить реалистичное меню на ДЕНЬ №{dayNumber}, ИСПОЛЬЗУЯ ТОЛЬКО указанные продукты: {ingredientsString}.

=== ЖЕСТКИЕ ПРАВИЛА БЕЗОПАСНОСТИ ===
1. НЕЛЬЗЯ придумывать рецепты из непищевых предметов (камень, металл, пластик, стройматериалы и т.д.).
2. НЕЛЬЗЯ использовать ингредиенты, которые не являются продуктами питания (литий, бензин, краска, стена и т.п.).
3. НЕЛЬЗЯ выдумывать недостающие продукты — используй ТОЛЬКО предоставленные ингредиенты.
4. Если ингредиенты неподходящие, недостаточные или непищевые — ВЕРНИ ОШИБКУ в валидном JSON (см. структуру ниже).

=== КРИТИЧЕСКИ ВАЖНЫЕ ТРЕБОВАНИЯ К ВЫВОДУ ===
1. ВЕРНИ ТОЛЬКО ВАЛИДНЫЙ JSON. Без ```json, без вступлений, без пояснений.
2. ИНГРЕДИЕНТЫ (ingredients) ДОЛЖНЫ БЫТЬ МАССИВОМ ОБЪЕКТОВ:
   Правильный формат: {{ ""name"": ""Яйцо"", ""quantity"": 2, ""unit"": ""шт"" }}
   !!! ЗАПРЕЩЕНО строки вида ""Яйцо — 2 шт"" !!!
3. Поле quantity — ЧИСЛО (не строка). Для ""по вкусу"" — 0.
4. Поле instructions — МАССИВ СТРОК (шаги приготовления).

=== СТРУКТУРА ОТВЕТА ===
ЕСЛИ ингредиенты подходящие и достаточные для 3 приёмов пищи:
{{
  ""menuName"": ""Меню день {dayNumber}"",
  ""items"": [
    {{
      ""dayNumber"": {dayNumber},
      ""mealType"": ""Завтрак"",
      ""recipe"": {{
        ""title"": ""Реальное название блюда"",
        ""description"": ""Описание из реальной кулинарии"",
        ""calories"": 350,
        ""prepTime"": 15,
        ""cookTime"": 20,
        ""instructions"": [""Шаг 1"", ""Шаг 2""],
        ""ingredients"": [
          {{ ""name"": ""Яйцо"", ""quantity"": 2, ""unit"": ""шт"" }}
        ]
      }}
    }},
    ...обед и ужин аналогично...
  ]
}}

ЕСЛИ ингредиенты НЕПОДХОДЯЩИЕ (непищевые, недостаточные, опасные):
{{
  ""error"": true,
  ""errorMessage"": ""Невозможно составить меню: среди ингредиентов есть непищевые предметы (перечисли их) или недостаточно продуктов для полноценного питания."",
  ""invalidIngredients"": [""камень"", ""стена""],
  ""suggestion"": ""Добавьте реальные продукты питания: овощи, фрукты, крупы, мясо, молочные продукты и т.д.""
}}

=== ВАЖНО ===
— Проверь КАЖДЫЙ ингредиент на пищевую пригодность ДО генерации рецептов.
— Если сомневаешься в ингредиенте — считай его непищевым и верни ошибку.
— Все рецепты должны существовать в реальной кулинарии или быть логичной комбинацией указанных продуктов.
— Никаких выдуманных блюд из непищевых материалов!
";
        }
        public static string CreateMenuPrompt(List<Ingredient> ingredients, int daysCount)
        {
            // ИСПРАВЛЕНИЕ: Преобразуем список объектов в строку с названиями через запятую
            string ingredientsString = string.Join(", ", ingredients.Select(i => i.Name));

            return $@"
Ты — профессиональный диетолог и шеф-повар. Твоя задача — составить меню на {daysCount} дней.
Меню должно быть составлено с приоритетным использованием следующих ингредиентов (но можно добавлять и обычные специи/масло): {ingredientsString}.

ТЫ ОБЯЗАН ВЕРНУТЬ ОТВЕТ ТОЛЬКО В ФОРМАТЕ JSON. 
НЕ ПИШИ НИКАКОГО ВСТУПИТЕЛЬНОГО ИЛИ ЗАКЛЮЧИТЕЛЬНОГО ТЕКСТА.
НЕ ИСПОЛЬЗУЙ MARKDOWN (```json). ПРОСТО ЧИСТЫЙ JSON.

Используй следующую структуру JSON:
{{
  ""menuName"": ""Название меню"",
  ""items"": [
    {{
      ""dayNumber"": 1,
      ""mealType"": ""Завтрак"",
      ""recipe"": {{
        ""title"": ""Название блюда"",
        ""description"": ""Краткое описание"",
        ""instructions"": ""Шаг 1... Шаг 2..."",
        ""calories"": 350,
        ""prepTime"": 15,
        ""cookTime"": 20,
        ""ingredients"": [
            {{ ""name"": ""Продукт 1"", ""quantity"": 2, ""unit"": ""шт"" }},
            {{ ""name"": ""Продукт 2"", ""quantity"": 100, ""unit"": ""мл"" }}
        ]
      }}
    }}
  ]
}}
";
        }
        private static string CleanJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            // Убираем ```json в начале и ``` в конце, если они есть
            json = json.Replace("```json", "").Replace("```", "").Trim();
            return json;
        }
        public static async Task<string> GetToken(string rqUID, string bearer)
        {
            string ReturnToken = null;
            string Url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
            using (HttpClientHandler Handler = new HttpClientHandler())
            {
                Handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyError) => true;
                using (HttpClient client = new HttpClient(Handler))
                {
                    HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, Url);
                    Request.Headers.Add("Accept", "application/json");
                    Request.Headers.Add("RqUID", rqUID);
                    Request.Headers.Add("Authorization", $"Basic {bearer}");
                    var Data = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
                    };
                    Request.Content = new FormUrlEncodedContent(Data);
                    HttpResponseMessage Response = await client.SendAsync(Request);
                    if (Response.IsSuccessStatusCode)
                    {
                        string ResponseContent = await Response.Content.ReadAsStringAsync();
                        Models.AIAPI.Responce.ResponseToken Token = JsonConvert.DeserializeObject<Models.AIAPI.Responce.ResponseToken>(ResponseContent);
                        ReturnToken = Token.access_token;
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка получения токена: {Response.StatusCode}");
                    }
                }
            }
            return ReturnToken;
        }

    }
}
