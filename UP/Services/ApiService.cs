using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UP.Models;

namespace UP.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _token;

        public ApiService(string baseUrl = "https://localhost:7197/api/")
        {
            _baseUrl = baseUrl;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetBaseUrl() => _baseUrl;

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"API Response: {response.StatusCode}");
            Console.WriteLine($"Response Content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP {response.StatusCode}: {content}");
            }

            if (string.IsNullOrEmpty(content))
            {
                return default(T);
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Пробуем разные форматы ответа
                try
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
                    if (apiResponse != null)
                    {
                        if (apiResponse.Success)
                        {
                            return apiResponse.Data;
                        }
                        else
                        {
                            throw new Exception(apiResponse.Message ?? "Ошибка сервера");
                        }
                    }
                }
                catch (JsonException)
                {
                    // Пробуем десериализовать напрямую
                    return JsonSerializer.Deserialize<T>(content, options);
                }

                throw new Exception($"Неизвестный формат ответа: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing response: {ex.Message}");
                throw new Exception($"Ошибка обработки ответа: {ex.Message}");
            }
        }

        // Аутентификация
        public async Task<UserData> LoginAsync(string username, string password)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(password), "password" }
                };

                Console.WriteLine($"Sending login request to: {_baseUrl}Auth/login");

                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/login", formData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ошибка входа: {response.StatusCode}");
                }

                var userData = await HandleResponse<UserData>(response);

                if (userData != null && !string.IsNullOrEmpty(userData.Token))
                {
                    SetToken(userData.Token);
                }

                return userData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserData> RegisterAsync(string username, string email, string password, List<string> allergies)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(email), "email" },
                    { new StringContent(password), "password" }
                };

                if (allergies != null)
                {
                    foreach (var allergy in allergies)
                    {
                        formData.Add(new StringContent(allergy), "allergies");
                    }
                }

                Console.WriteLine($"Sending register request to: {_baseUrl}Auth/register");

                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/register", formData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ошибка регистрации: {response.StatusCode}");
                }

                return await HandleResponse<UserData>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex.Message}");
                throw;
            }
        }

        // Рецепты
        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes");
                return await HandleResponse<List<RecipeDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecipes error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }

        public async Task<RecipeDto> GetRecipeAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/{id}");
                return await HandleResponse<RecipeDto>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecipe error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RecipeDto>> GetFavoritesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/favorites");
                return await HandleResponse<List<RecipeDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetFavorites error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }

        public async Task<bool> ToggleFavoriteAsync(int recipeId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}Recipes/{recipeId}/favorite", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleFavorite error: {ex.Message}");
                return false;
            }
        }

        // Меню
        public async Task<MenuDto> GenerateMenuAsync(GenerateMenuRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}Menu/generate", content);
                return await HandleResponse<MenuDto>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateMenu error: {ex.Message}");
                throw;
            }
        }

        public async Task<MenuDto> GetCurrentMenuAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/current");
                return await HandleResponse<MenuDto>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentMenu error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<MenuDto>> GetMenuHistoryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/history");
                return await HandleResponse<List<MenuDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMenuHistory error: {ex.Message}");
                return new List<MenuDto>();
            }
        }

        // Список покупок
        public async Task<ShoppingListDto> GetCurrentShoppingListAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}ShoppingList/current");
                return await HandleResponse<ShoppingListDto>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCurrentShoppingList error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GenerateShoppingListAsync(int menuId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}ShoppingList/generate-from-menu/{menuId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateShoppingList error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ToggleShoppingItemAsync(int listId, int itemId, bool isPurchased)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(isPurchased.ToString()), "isPurchased" }
                };

                var response = await _httpClient.PostAsync($"{_baseUrl}ShoppingList/{listId}/items/{itemId}/toggle", formData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleShoppingItem error: {ex.Message}");
                return false;
            }
        }

        // Ингредиенты и инвентарь
        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Ingredients");
                return await HandleResponse<List<IngredientDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetIngredients error: {ex.Message}");
                return new List<IngredientDto>();
            }
        }

        public async Task<bool> AddToInventoryAsync(int ingredientId, decimal quantity, string unit)
        {
            try
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(ingredientId.ToString()), "ingredientId" },
                    { new StringContent(quantity.ToString()), "quantity" },
                    { new StringContent(unit), "unit" }
                };

                var response = await _httpClient.PostAsync($"{_baseUrl}Inventory/add", formData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToInventory error: {ex.Message}");
                return false;
            }
        }

        // Поиск рецептов
        public async Task<List<RecipeDto>> SearchRecipesAsync(string query, List<string> tags = null, int? maxTime = null)
        {
            try
            {
                var url = $"{_baseUrl}Recipes/search?query={Uri.EscapeDataString(query ?? "")}";

                if (tags != null && tags.Count > 0)
                {
                    url += $"&tags={string.Join(",", tags)}";
                }

                if (maxTime.HasValue)
                {
                    url += $"&maxTime={maxTime.Value}";
                }

                var response = await _httpClient.GetAsync(url);
                return await HandleResponse<List<RecipeDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchRecipes error: {ex.Message}");
                return new List<RecipeDto>();
            }
        }
    }
}