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
        public static async Task<Models.AIAPI.Responce.ResponseMessage> GetAnswer(string token, List<Request.Message> messages)
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

            for (int i = 1; i <= daysCount; i++)
            {
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
                        var dayMenu = JsonConvert.DeserializeObject<GeneratedMenuDto>(json);
                        if (dayMenu?.Items != null)
                        {
                            finalMenu.Items.AddRange(dayMenu.Items);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка генерации дня {i}: {ex.Message}");
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
Ты — профессиональный шеф-повар и технический ассистент.
Твоя задача — составить меню на ДЕНЬ №{dayNumber}, используя продукты: {ingredientsString}.

=== КРИТИЧЕСКИ ВАЖНЫЕ ПРАВИЛА ===
1. ВЕРНИ ТОЛЬКО ВАЛИДНЫЙ JSON. Без ```json, без вступлений, без 'Вот ваше меню'.
2. ИНГРЕДИЕНТЫ (ingredients) ДОЛЖНЫ БЫТЬ МАССИВОМ ОБЪЕКТОВ.
   !!! ЗАПРЕЩЕНО использовать строки вида ""Яйцо — 2 шт"" !!!
   Правильный формат: {{ ""name"": ""Яйцо"", ""quantity"": 2, ""unit"": ""шт"" }}
3. Поле quantity должно быть ЧИСЛОМ (не строкой ""2"", а числом 2). Если вес по вкусу — пиши 0.
4. Поле instructions (инструкция) должно быть МАССИВОМ СТРОК (шагов).

=== СТРУКТУРА JSON ===
Используй строго этот шаблон:
{{
  ""menuName"": ""Меню день {dayNumber}"",
  ""items"": [
    {{
      ""dayNumber"": {dayNumber},
      ""mealType"": ""Завтрак"",
      ""recipe"": {{
        ""title"": ""Название блюда"",
        ""description"": ""Краткое описание"",
        ""calories"": 350,
        ""prepTime"": 15,
        ""cookTime"": 20,
        ""instructions"": [
           ""Нарежьте овощи."",
           ""Обжарьте их.""
        ],
        ""ingredients"": [
            {{ ""name"": ""Продукт А"", ""quantity"": 100, ""unit"": ""г"" }},
            {{ ""name"": ""Продукт Б"", ""quantity"": 2, ""unit"": ""шт"" }}
        ]
      }}
    }},
    {{
      ""dayNumber"": {dayNumber},
      ""mealType"": ""Обед"",
      ""recipe"": {{ ... аналогичная структура ... }}
    }},
    {{
      ""dayNumber"": {dayNumber},
      ""mealType"": ""Ужин"",
      ""recipe"": {{ ... аналогичная структура ... }}
    }}
  ]
}}
";
        }
        public static string CreateMenuPrompt(List<Ingredient> ingredients, int daysCount)
        {
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
                            ""imageurl"": ""Ссылка на картинку по типу (www.image.ru)""
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
