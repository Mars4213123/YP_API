using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                try
                {
                    var simpleAuth = JsonConvert.DeserializeObject<SimpleAuthResponse>(content, settings);
                    if (simpleAuth != null && !string.IsNullOrEmpty(simpleAuth.Token))
                    {
                        if (typeof(T) == typeof(UserData))
                        {
                            var userData = new UserData
                            {
                                Id = simpleAuth.Id,
                                Username = simpleAuth.Username,
                                Email = simpleAuth.Email,
                                Token = simpleAuth.Token
                            };
                            return (T)(object)userData;
                        }
                    }
                }
                catch (JsonException) { }

                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content, settings);
                    if (apiResponse != null && apiResponse.Success)
                    {
                        return apiResponse.Data;
                    }
                }
                catch (JsonException) { }

                return JsonConvert.DeserializeObject<T>(content, settings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обработки ответа: {ex.Message}");
            }
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
                throw;
            }
        }

        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes");
                return await HandleResponse<List<RecipeDto>>(response);
            }
            catch (Exception ex)
            {
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
                throw;
            }
        }

        public async Task<List<RecipeDto>> GetFavoritesAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/favorites/{userId}");
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
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.PostAsync($"{_baseUrl}Recipes/{recipeId}/favorite/{userId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleFavorite error: {ex.Message}");
                return false;
            }
        }

        public async Task<MenuDto> GenerateMenuAsync(GenerateMenuRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.PostAsync($"{_baseUrl}Menu/generate/{userId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Menu generation failed: {response.StatusCode} - {errorContent}");
                }

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
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/current/{userId}");
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
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/history/{userId}");
                return await HandleResponse<List<MenuDto>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMenuHistory error: {ex.Message}");
                return new List<MenuDto>();
            }
        }

        public async Task<ShoppingListDto> GetCurrentShoppingListAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.GetAsync($"{_baseUrl}ShoppingList/current/{userId}");
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
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.PostAsync($"{_baseUrl}ShoppingList/generate-from-menu/{menuId}/{userId}", null);
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
                return new List<RecipeDto>();
            }
        }
        public async Task CheckAvailableEndpoints()
        {
            try
            {
                var endpoints = new[]
                {
            $"{_baseUrl}Menu/generate",
            $"{_baseUrl}Menu",
            $"{_baseUrl}Menu/create",
            $"{_baseUrl}Menu/plan",
            $"{_baseUrl}MenuPlans",
            $"{_baseUrl}MenuPlan"
        };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(endpoint);
                        Console.WriteLine($"Endpoint {endpoint}: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Endpoint {endpoint}: Error - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Check endpoints error: {ex.Message}");
            }
        }

        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/ingredients");
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

                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.PostAsync($"{_baseUrl}Inventory/add/{userId}", formData);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToInventory error: {ex.Message}");
                return false;
            }
        }
    }
}