using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Newtonsoft.Json;
using UP.Models;

using static UP.Pages.Receipts;

namespace UP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string _token;

        public ApiService(string baseUrl = "https://localhost:7197/")
        {
            _baseUrl = baseUrl;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<List<IngredientDto>> SearchIngredientsAsync(string term)
        {
            // Предполагаем, что у тебя есть DTO для ингредиента
            try
            {
                // Запрос к API поиска ингредиентов (замени путь на свой реальный, если он другой)
                var response = await _httpClient.GetAsync($"api/recipes/ingredients/search?query={term}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<IngredientDto>>();
                }
            }
            catch
            {
                // Игнорируем ошибки поиска пока
            }
            return new List<IngredientDto>();
        }


        // Добавь эти методы в класс ApiService

        // 1. Отправка продуктов в холодильник (превращаем строки в список для API)
        public async Task<bool> UpdateFridgeAsync(int userId, List<string> productNames)
        {
            // Сначала нам нужно найти ID ингредиентов по их названиям
            // Предполагаем, что есть метод поиска или кеш ингредиентов.
            // Для примера: ищем через API поиска ингредиентов, чтобы получить ID.

            var fridgeItems = new List<object>();

            foreach (var name in productNames)
            {
                // Ищем ингредиент в базе, чтобы узнать его ID
                var foundIngredients = await SearchIngredientsAsync(name); // Используем твой существующий поиск
                var bestMatch = foundIngredients.FirstOrDefault();

                if (bestMatch != null)
                {
                    fridgeItems.Add(new
                    {
                        IngredientId = bestMatch.Id,
                        Quantity = 1 // По умолчанию 1, если в UI нет выбора кол-ва
                    });
                }
            }

            // Отправляем список ID в контроллер UserPreferences
            var response = await _httpClient.PostAsJsonAsync($"api/userpreferences/user/{userId}/fridge", fridgeItems);
            return response.IsSuccessStatusCode;
        }

        // 2. Генерация меню на основе холодильника и аллергий
        public async Task<bool> GenerateMenuAsync(int userId)
        {
            // Этот запрос должен инициировать логику подбора рецептов на бэкенде
            // Предположим, у тебя есть метод POST api/menu/generate
            var response = await _httpClient.PostAsync($"api/menu/generate?userId={userId}", null);
            return response.IsSuccessStatusCode;
        }

        // 3. Получение списка всех доступных меню
        public async Task<List<AvailableMenu>> GetUserMenusAsync(int userId)
        {
            // GET api/menu/user/{userId}
            var response = await _httpClient.GetAsync($"api/menu/user/{userId}/all");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AvailableMenu>>();
            }
            return new List<AvailableMenu>();
        }

        // 4. Получение деталей конкретного меню (список рецептов внутри)
        public async Task<MenuDto> GetMenuDetailsAsync(int menuId)
        {
            var response = await _httpClient.GetAsync($"api/menu/{menuId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MenuDto>();
            }
            return null;
        }

        public async Task<bool> SetInventoryByNamesAsync(int userId, List<string> productNames)
        {
            try
            {
                var items = new List<object>();

                foreach (var name in productNames)
                {
                    var trimmed = name.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    var id = await FindIngredientIdByNameAsync(trimmed);
                    if (id == null)
                        throw new Exception($"Не удалось найти ингредиент '{trimmed}'");

                    items.Add(new { IngredientId = id.Value, Quantity = 1.0, Unit = "" });
                }

                if (items.Count == 0)
                    throw new Exception("Не удалось распознать ни одного продукта");

                var json = System.Text.Json.JsonSerializer.Serialize(items);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _httpClient.PostAsync($"{_baseUrl}api/Inventory/set/{userId}", content);
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetInventoryByNamesAsync error: {ex.Message}");
                throw;
            }
        }

        public void SetToken(string token)
        {
            _token = token;
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public void ClearToken()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Вспомогательный метод для POST JSON (замена PostAsJsonAsync)
        private async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T body)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(url, content);
        }

        public async Task<UserData> LoginAsync(string username, string password)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(password), "password" }
                };

                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/login", formData);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка входа: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    throw new Exception("Пустой ответ от сервера");

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка входа");

                var userData = new UserData
                {
                    Id = responseObj.id ?? responseObj.data?.id ?? 0,
                    Username = responseObj.username ?? responseObj.data?.username ?? username,
                    Email = responseObj.email ?? responseObj.data?.email ?? "",
                    Token = responseObj.token ?? responseObj.data?.token ?? ""
                };

                SetToken(userData.Token);
                return userData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при входе: {ex.Message}");
            }
        }

        // Если хочешь использовать универсальный POST JSON из WPF
        // Измени этот метод, чтобы он возвращал HttpResponseMessage
        public async Task<HttpResponseMessage> PostJsonAsync<T>(string relativeUrl, T body, bool ensureSuccess = true)
        {
            var resp = await PostJsonAsync(_baseUrl + relativeUrl, body); // вызывает приватный метод
            if (ensureSuccess)
                resp.EnsureSuccessStatusCode();
            return resp;
        }


        // Универсальный GET с System.Text.Json
        public async Task<T> GetAsync<T>(string relativeUrl)
        {
            var resp = await _httpClient.GetAsync(_baseUrl + relativeUrl);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
            return result;
        }

        public async Task<bool> SetFridgeByNamesAsync(int userId, List<string> productNames)
        {
            try
            {
                var items = new List<object>();

                foreach (var name in productNames)
                {
                    var trimmed = name.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    var id = await FindIngredientIdByNameAsync(trimmed);

                    // Если не нашли — пробуем создать!
                    if (id == null)
                    {
                        id = await CreateIngredientByNameAsync(trimmed);
                    }

                    // Если даже создать не удалось — тогда ошибка
                    if (id == null)
                        throw new Exception($"Не удалось создать или найти ингредиент '{trimmed}'");


                    items.Add(new { IngredientId = id.Value, Quantity = 1.0 });
                }

                if (items.Count == 0)
                    throw new Exception("Не удалось распознать ни одного продукта");

                var json = System.Text.Json.JsonSerializer.Serialize(items);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _httpClient.PostAsync($"{_baseUrl}api/UserPreferences/{userId}/fridge", content);
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetFridgeByNamesAsync error: {ex.Message}");
                throw; // пробрасываем, чтобы показать сообщение пользователю
            }
        }
        public async Task<int?> FindIngredientIdByNameAsync(string name)
        {
            try
            {
                var url = $"{_baseUrl}Ingredients/search?name={Uri.EscapeDataString(name)}";
                var response = await _httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return null;

                // Десериализуем как динамический объект
                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);

                // Проверяем, что ответ не пустой
                if (responseObj == null || responseObj.data == null) return null;

                // Проверяем, что в массиве data есть хотя бы один элемент
                if (responseObj.data.Count == 0) return null;

                var first = responseObj.data[0];

                // ВАЖНО: Используем маленькую букву 'id', как в ответе сервера
                int id = first.id;

                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FindIngredientIdByNameAsync error: {ex.Message}");
                return null;
            }
        }



        public async Task<List<RecipeDto>> GetRecipesByFridgeAsync(int userId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"{_baseUrl}api/Recipes/by-fridge/{userId}");
                var responseString = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения рецептов по холодильнику: {resp.StatusCode}");

                // 1. Сначала читаем ответ как динамический объект, чтобы добраться до поля "data"
                // (Newtonsoft.Json.JsonConvert уже умеет игнорировать регистр по умолчанию)
                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);

                // 2. Проверяем, есть ли поле data
                if (responseObj == null || responseObj.data == null)
                {
                    return new List<RecipeDto>();
                }

                // 3. Превращаем содержимое data в список рецептов
                string dataJson = responseObj.data.ToString();
                var recipes = JsonConvert.DeserializeObject<List<RecipeDto>>(dataJson);

                return recipes ?? new List<RecipeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecipesByFridgeAsync error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }




        public async Task<UserData> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(email), "email" },
                    { new StringContent(password), "password" }
                };

                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/register", formData);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка регистрации: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    throw new Exception("Пустой ответ от сервера");

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка регистрации");

                var userData = new UserData
                {
                    Id = responseObj.id ?? responseObj.data?.id ?? 0,
                    Username = responseObj.username ?? responseObj.data?.username ?? username,
                    Email = responseObj.email ?? responseObj.data?.email ?? email,
                    Token = responseObj.token ?? responseObj.data?.token ?? ""
                };

                SetToken(userData.Token);
                return userData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при регистрации: {ex.Message}");
            }
        }

        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения рецептов: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return new List<RecipeDto>();

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка получения рецептов");

                if (responseObj.data != null)
                {
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObj.data.ToString())
                           ?? new List<RecipeDto>();
                }

                return new List<RecipeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecipesAsync error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }

        public async Task<RecipeDto> GetRecipeAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/{id}");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения рецепта: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return null;

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка получения рецепта");

                if (responseObj.data != null)
                    return JsonConvert.DeserializeObject<RecipeDto>(responseObj.data.ToString());

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecipeAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RecipeDto>> GetFavoritesAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return new List<RecipeDto>();

                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/favorites/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения избранного: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return new List<RecipeDto>();

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка получения избранного");

                if (responseObj.data != null)
                {
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObj.data.ToString())
                           ?? new List<RecipeDto>();
                }

                return new List<RecipeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetFavoritesAsync error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int recipeId)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return false;

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}Recipes/{recipeId}/favorite/{userId}",
                    null);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка изменения избранного: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return false;

                return responseObj.success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleFavoriteAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<MenuDto> GetCurrentMenuAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return null;

                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/user/{userId}/current");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения меню: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return null;

                bool success = responseObj.success ?? false;
                if (!success)
                    return null; // нет меню — это норм

                if (responseObj.data != null)
                    return JsonConvert.DeserializeObject<MenuDto>(JsonConvert.SerializeObject(responseObj.data));

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentMenuAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<ShoppingListDto> GetCurrentShoppingListAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return null;

                var response = await _httpClient.GetAsync($"{_baseUrl}ShoppingList/user/{userId}/current");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения списка покупок: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return null;

                bool success = responseObj.success ?? false;
                if (!success)
                    return null;

                if (responseObj.data != null)
                    return JsonConvert.DeserializeObject<ShoppingListDto>(JsonConvert.SerializeObject(responseObj.data));

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentShoppingListAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GenerateShoppingListAsync(int menuId)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return false;

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}ShoppingList/generate-from-menu/{menuId}/{userId}",
                    null);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка генерации списка покупок: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return false;

                return responseObj.success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateShoppingListAsync error: {ex.Message}");
                return false;
            }
        }
        // В файле Services/ApiService.cs добавь этот метод:

        public async Task<int?> CreateIngredientByNameAsync(string name)
        {
            try
            {
                var newIng = new { Name = name };
                var response = await PostJsonAsync("api/Ingredients/create", newIng, ensureSuccess: false);

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObj != null && responseObj.success == true && responseObj.data != null)
                {
                    // ИСПРАВЛЕНИЕ: .id вместо .Id
                    return (int)responseObj.data.id;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return null;
            }
        }


        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Ingredients/search");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения ингредиентов: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return new List<IngredientDto>();

                bool success = responseObj.success ?? false;
                if (!success)
                    throw new Exception(responseObj.message?.ToString() ?? "Ошибка получения ингредиентов");

                if (responseObj.data != null)
                {
                    return JsonConvert.DeserializeObject<List<IngredientDto>>(responseObj.data.ToString())
                           ?? new List<IngredientDto>();
                }

                return new List<IngredientDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetIngredientsAsync error: {ex.Message}");
                return new List<IngredientDto>();
            }
        }
        // Метод для массового добавления продуктов (отправляет по одному)
        public async Task<bool> AddProductsToFridgeAsync(int userId, List<string> productNames)
        {
            try
            {
                bool allSuccess = true;

                foreach (var name in productNames)
                {
                    var trimmedName = name.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedName)) continue;

                    // Формируем данные формы (MultipartFormData)
                    var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(trimmedName), "productName");
                    formData.Add(new StringContent("1"), "quantity"); // Значение по умолчанию
                    formData.Add(new StringContent("шт"), "unit");    // Значение по умолчанию

                    // Отправляем запрос на добавление одного продукта
                    var response = await _httpClient.PostAsync($"{_baseUrl}api/Inventory/add/{userId}", formData);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Ошибка добавления '{trimmedName}': {response.StatusCode}");
                        allSuccess = false;
                    }
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в AddProductsToFridgeAsync: {ex.Message}");
                return false; // Возвращаем false, если что-то пошло не так
            }
        }

        public async Task<bool> AddToInventoryByNameAsync(string productName, decimal quantity = 1, string unit = "шт")
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                    return false;

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(productName), "productName" },
                    { new StringContent(quantity.ToString()), "quantity" },
                    { new StringContent(unit), "unit" }
                };

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}Inventory/add/{userId}",
                    formData);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка добавления в инвентарь: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return false;

                return responseObj.success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToInventoryByNameAsync error: {ex.Message}");
                return false;
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } // Исправляет ошибку CreatedAt
        public List<MenuItemDto> Items { get; set; } // Исправляет ошибку Items
    }

    public class MenuItemDto
    {
        public int RecipeId { get; set; }
        public string RecipeTitle { get; set; }
        public DateTime Date { get; set; }
        public string MealType { get; set; }
    }


    public class CurrentMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<MenuItemDto> Items { get; set; } 
    }
    public class IngredientDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RecipeDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; }
        public string Instructions { get; set; }
    }
}
