using System;
using System.Collections.Generic;
using System.Linq;
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
            _httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
        }

        public void SetToken(string token)
        {
            _token = token;
            // Не устанавливаем заголовок Authorization, если API не использует Bearer
        }

        public void ClearToken()
        {
            _token = null;
        }

        public async Task<UserData> LoginAsync(string username, string password)
        {
            try
            {
                Console.WriteLine($"Попытка входа для пользователя: {username}");

                var formData = new Dictionary<string, string>
                {
                    { "username", username },
                    { "password", password }
                };

                var content = new FormUrlEncodedContent(formData);

                Console.WriteLine($"Отправка запроса на: {_baseUrl}Auth/login");

                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/login", content);

                Console.WriteLine($"Статус ответа: {response.StatusCode}");

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Содержимое ответа: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка HTTP: {response.StatusCode}, Контент: {errorContent}");
                    throw new HttpRequestException($"Ошибка входа: {response.StatusCode}. {errorContent}");
                }

                // Парсим ответ
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                Console.WriteLine($"Распарсенный объект: {responseObject}");

                if (responseObject != null)
                {
                    var userData = new UserData();

                    // Формат ответа из вашего примера
                    userData.Id = responseObject.id ?? 0;
                    userData.Username = responseObject.username ?? username;
                    userData.Email = responseObject.email ?? "";
                    userData.FullName = responseObject.fullName ?? username;

                    // Не используем токен, так как API его не возвращает
                    Console.WriteLine($"Вход успешен для пользователя: {userData.Username}, ID: {userData.Id}");

                    return userData;
                }
                else
                {
                    var errorMessage = responseObject?.message?.ToString() ?? "Ошибка входа";
                    Console.WriteLine($"Ошибка входа: {errorMessage}");
                    throw new Exception(errorMessage);
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Ошибка парсинга JSON: {jsonEx.Message}");
                throw new Exception($"Ошибка обработки ответа от сервера: {jsonEx.Message}");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Ошибка сети: {httpEx.Message}");
                throw new HttpRequestException($"Ошибка сети: {httpEx.Message}. Проверьте, запущен ли сервер API.", httpEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка входа: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception($"Ошибка входа: {ex.Message}");
            }
        }

        public async Task<UserData> RegisterAsync(string username, string email, string password, List<string> allergies)
        {
            try
            {
                Console.WriteLine($"Регистрация пользователя: {username}, email: {email}");

                var formData = new Dictionary<string, string>
                {
                    { "username", username },
                    { "email", email },
                    { "password", password },
                    { "fullName", username }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync($"{_baseUrl}Auth/register", content);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ответ регистрации: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ошибка регистрации: {response.StatusCode}. {errorContent}");
                }

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return new UserData
                    {
                        Id = responseObject.data?.Id ?? responseObject.id ?? 0,
                        Username = responseObject.data?.Username ?? responseObject.username ?? username,
                        Email = responseObject.data?.Email ?? responseObject.email ?? email,
                        FullName = responseObject.data?.FullName ?? responseObject.fullName ?? username
                    };
                }
                else
                {
                    throw new Exception(responseObject?.message?.ToString() ?? "Ошибка регистрации");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка регистрации: {ex.Message}");
                throw new Exception($"Ошибка регистрации: {ex.Message}");
            }
        }

        // Остальные методы (GetRecipesAsync, GetRecipeAsync и т.д.) оставляем без изменений
        // Только убираем из них любые упоминания Authorization header

        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObject.data.ToString());
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
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<RecipeDto>(responseObject.data.ToString());
                }

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
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/favorites/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObject.data.ToString());
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
                var formData = new Dictionary<string, string>();
                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync($"{_baseUrl}Recipes/{recipeId}/favorite/{userId}", content);

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                return responseObject != null && responseObject.success == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleFavoriteAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<MenuDto> GenerateMenuAsync(GenerateMenuRequest request)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;

                // Создаем multipart form-data вместо JSON
                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(request.Days.ToString()), "days");

                if (request.TargetCaloriesPerDay.HasValue)
                {
                    formData.Add(new StringContent(request.TargetCaloriesPerDay.Value.ToString()), "targetCaloriesPerDay");
                }

                // Добавляем теги кухни
                if (request.CuisineTags != null && request.CuisineTags.Count > 0)
                {
                    foreach (var tag in request.CuisineTags)
                    {
                        formData.Add(new StringContent(tag), "cuisineTags");
                    }
                }

                if (request.MealTypes != null && request.MealTypes.Count > 0)
                {
                    foreach (var mealType in request.MealTypes)
                    {
                        formData.Add(new StringContent(mealType), "mealTypes");
                    }
                }

                formData.Add(new StringContent(request.UseInventory.ToString().ToLower()), "useInventory");

                Console.WriteLine($"Sending menu generation request to: {_baseUrl}Menu/generate/{userId}");
                Console.WriteLine($"Form data count: {formData.Count()} items");

                var response = await _httpClient.PostAsync($"{_baseUrl}Menu/generate/{userId}", formData);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Menu generation response status: {response.StatusCode}");
                Console.WriteLine($"Menu generation response content: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Menu generation failed: {response.StatusCode} - {responseString}");
                    return null;
                }

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<MenuDto>(responseObject.data.ToString());
                }

                Console.WriteLine($"Menu generation response parsing failed: {responseString}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateMenuAsync error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<MenuDto> GetCurrentMenuAsync()
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                var response = await _httpClient.GetAsync($"{_baseUrl}Menu/current/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<MenuDto>(responseObject.data.ToString());
                }

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
                var response = await _httpClient.GetAsync($"{_baseUrl}ShoppingList/current/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<ShoppingListDto>(responseObject.data.ToString());
                }

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
                var formData = new MultipartFormDataContent();
                var response = await _httpClient.PostAsync($"{_baseUrl}ShoppingList/generate-from-menu/{menuId}/{userId}", formData);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GenerateShoppingListAsync response: {responseString}");

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                return responseObject != null && responseObject.success == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GenerateShoppingListAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/ingredients");
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObject != null && responseObject.success == true)
                {
                    return JsonConvert.DeserializeObject<List<IngredientDto>>(responseObject.data.ToString());
                }

                return new List<IngredientDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetIngredientsAsync error: {ex.Message}");
                return new List<IngredientDto>();
            }
        }

        public async Task<bool> AddToInventoryAsync(int ingredientId, decimal quantity, string unit)
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    Console.WriteLine("User not logged in, cannot add to inventory");
                    return false;
                }

                var ingredients = await GetIngredientsAsync();
                var ingredient = ingredients.FirstOrDefault(i => i.Id == ingredientId);

                if (ingredient == null)
                {
                    Console.WriteLine($"Ingredient with ID {ingredientId} not found");
                    return false;
                }

                var formData = new Dictionary<string, string>
                {
                    { "productName", ingredient.Name },
                    { "quantity", quantity.ToString() },
                    { "unit", unit }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync($"{_baseUrl}Inventory/add/{userId}", content);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"AddToInventory response: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"AddToInventory failed: {response.StatusCode}");
                    return false;
                }

                try
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    return responseObject != null && responseObject.success == true;
                }
                catch (JsonException)
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToInventoryAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddToInventoryByNameAsync(string productName, decimal quantity = 1, string unit = "шт")
        {
            try
            {
                var userId = AppData.CurrentUser?.Id ?? 0;
                if (userId == 0)
                {
                    Console.WriteLine("User not logged in, cannot add to inventory");
                    return false;
                }

                var formData = new Dictionary<string, string>
                {
                    { "productName", productName },
                    { "quantity", quantity.ToString() },
                    { "unit", unit }
                };

                var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync($"{_baseUrl}Inventory/add/{userId}", content);

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"AddToInventoryByName response: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"AddToInventoryByName failed: {response.StatusCode}");
                    return false;
                }

                try
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                    return responseObject != null && responseObject.success == true;
                }
                catch (JsonException)
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToInventoryByNameAsync error: {ex.Message}");
                return false;
            }
        }
    }
}