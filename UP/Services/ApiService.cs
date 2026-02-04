using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UP.Models;

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

            // BaseAddress должен быть корневой URL без /api/
            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            
            Console.WriteLine($"[ApiService] BaseAddress установлен на: {_httpClient.BaseAddress}");

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<IngredientDto>> SearchIngredientsAsync(string term)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ingredients/search?name={Uri.EscapeDataString(term)}");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                    
                    if (responseObj?.data != null)
                    {
                        var ingredients = JsonConvert.DeserializeObject<List<IngredientDto>>(
                            responseObj.data.ToString());
                        return ingredients ?? new List<IngredientDto>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SearchIngredientsAsync] Error: {ex.Message}");
            }
            return new List<IngredientDto>();
        }

        public async Task<List<Models.IngredientDto>> UpdateFridgeAsync(int userId)
        {
            try
            {
                var fridgeItems = new List<IngredientDto>();

                var response = await _httpClient.GetAsync($"api/ingredients/fridge/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObj?.data != null)
                {
                    var ingredients = JsonConvert.DeserializeObject<List<Models.IngredientDto>>(
                        responseObj.data.ToString());
                    return ingredients ?? new List<IngredientDto>();
                }
                else return null;
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateFridgeAsync] Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GenerateMenuAsync(int userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/menu/generate-week/{userId}", null);
                var responseString = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[GenerateMenuAsync] Status: {response.StatusCode}, Response: {responseString}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GenerateMenuAsync] Ошибка при генерации меню: {responseString}");
                    return false;
                }
                
                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                return responseObj?.success == true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateMenuAsync] Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<Models.MenuDto> GenerateMenuFromGigaChatAsync(int userId, List<Models.IngredientDto> ingredients)
        {
            try
            {
                var requestData = new
                {
                    Prompt = "Составь меню на день из моих продуктов",
                    Ingredients = ingredients
                };

                var json = JsonConvert.SerializeObject(requestData);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/Ai/ask/{userId}", content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[GenerateMenuAsync] Status: {response.StatusCode}, Response: {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GenerateMenuAsync] Ошибка при генерации меню: {responseString}");
                    return null;
                }

                var menu = JsonConvert.DeserializeObject<Models.MenuDto>(responseString);
                return menu;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GenerateMenuAsync] Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AvailableMenu>> GetUserMenusAsync(int userId)
        {
            try
            {
                Console.WriteLine($"[GetUserMenusAsync] Загружаем меню для пользователя {userId}");
                
                var response = await _httpClient.GetAsync($"api/menu/user/{userId}/all");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GetUserMenusAsync] Error: {response.StatusCode}");
                    return new List<AvailableMenu>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetUserMenusAsync] Response: {responseString}");
                
                var menus = JsonConvert.DeserializeObject<List<AvailableMenu>>(responseString);
                Console.WriteLine($"[GetUserMenusAsync] Десериализовано {menus?.Count ?? 0} меню");
                
                return menus ?? new List<AvailableMenu>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetUserMenusAsync] Exception: {ex.Message}\n{ex.StackTrace}");
                return new List<AvailableMenu>();
            }
        }

        public async Task<MenuDto> GetMenuDetailsAsync(int menuId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/menu/{menuId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GetMenuDetailsAsync] Error: {response.StatusCode}");
                    return null;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetMenuDetailsAsync] Response: {responseString}");

                // Используем JsonConvert для более надежного парсинга
                var responseObj = JsonConvert.DeserializeObject<ApiResponse<MenuDataDto>>(responseString);
                
                if (responseObj?.Data != null)
                {
                    var menuData = responseObj.Data;
                    
                    var menuDto = new MenuDto
                    {
                        Id = menuData.Id,
                        Name = menuData.Name,
                        CreatedAt = menuData.CreatedAt,
                        Items = new List<MenuItemDto>()
                    };

                    if (menuData.Items != null && menuData.Items.Count > 0)
                    {
                        foreach (var item in menuData.Items)
                        {
                            menuDto.Items.Add(new MenuItemDto
                            {
                                RecipeId = item.RecipeId,
                                RecipeTitle = item.RecipeTitle,
                                Date = item.Date,
                                MealType = item.MealType
                            });
                        }
                    }

                    Console.WriteLine($"[GetMenuDetailsAsync] Успешно загружено {menuDto.Items.Count} блюд");
                    return menuDto;
                }

                Console.WriteLine($"[GetMenuDetailsAsync] responseObj.Data == null");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMenuDetailsAsync] Exception: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<bool> SetInventoryByNamesAsync(int userId, List<Models.IngredientDto> productNames)
        {
            try
            {
                var items = new List<object>();
                foreach (var name in productNames)
                {
                    var trimmed = name.Name.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    
                    var id = await FindIngredientIdByNameAsync(trimmed);
                    if (id == null)
                        id = await CreateIngredientByNameAsync(trimmed);
                    
                    if (id == null)
                        throw new Exception($"Не удалось создать или найти ингредиент '{trimmed}'");
                    
                    items.Add(new { IngredientId = id.Value, Quantity = 1.0, Unit = "шт" });
                }
                if (items.Count == 0)
                    throw new Exception("Не удалось распознать ни одного продукта");

                var json = System.Text.Json.JsonSerializer.Serialize(items);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync($"api/inventory/set/{userId}", content);
                
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"SetInventoryByNamesAsync: HTTP {(int)resp.StatusCode} {resp.StatusCode}. Response body: {body}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetInventoryByNamesAsync error: {ex.Message}");
                return false;
            }
        }

        public void SetToken(string token)
        {
            _token = token;
            if (!string.IsNullOrEmpty(_token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        public void ClearToken()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

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
                var response = await _httpClient.PostAsync($"api/auth/login", formData);
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

        public async Task<HttpResponseMessage> PostJsonAsync<T>(string relativeUrl, T body, bool ensureSuccess = true)
        {
            var resp = await PostJsonAsync(relativeUrl, body);
            if (ensureSuccess)
                resp.EnsureSuccessStatusCode();
            return resp;
        }
        public async Task<T> GetAsync<T>(string relativeUrl)
        {
            var resp = await _httpClient.GetAsync(relativeUrl);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;

                    var id = await FindIngredientIdByNameAsync(trimmed);
                    if (id == null)
                        id = await CreateIngredientByNameAsync(trimmed);

                    if (id == null)
                        throw new Exception($"Не удалось создать или найти ингредиент '{trimmed}'");

                    items.Add(new { IngredientId = id.Value, Quantity = 1.0 });
                }

                if (items.Count == 0)
                    throw new Exception("Не удалось распознать ни одного продукта");

                var json = System.Text.Json.JsonSerializer.Serialize(items);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _httpClient.PostAsync($"api/userpreferences/user/{userId}/fridge", content);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    Console.WriteLine($"SetFridgeByNamesAsync: HTTP {(int)resp.StatusCode} {resp.StatusCode}. Response body: {body}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SetFridgeByNamesAsync error: {ex.Message}");
                return false;
            }
        }

        public async Task<int?> FindIngredientIdByNameAsync(string name)
        {
            try
            {
                var url = $"api/ingredients/search?name={Uri.EscapeDataString(name)}";
                var fullUrl = new Uri(_httpClient.BaseAddress, url);
                Console.WriteLine($"[FindIngredientIdByNameAsync] Full URL: {fullUrl}");
                var response = await _httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return null;

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null || responseObj.data == null) return null;
                if (responseObj.data.Count == 0) return null;

                var first = responseObj.data[0];
                int id = first.id;
                return id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FindIngredientIdByNameAsync error: {ex.Message}");
                return null;
            }
        }
        public async Task<Models.IngredientDto> FindIngredientByNameAsync(string name)
        {
            try
            {
                var url = $"api/ingredients/search?name={Uri.EscapeDataString(name)}";
                var fullUrl = new Uri(_httpClient.BaseAddress, url);
                Console.WriteLine($"[FindIngredientIdByNameAsync] Full URL: {fullUrl}");
                var response = await _httpClient.GetAsync(url);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return null;

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null || responseObj.data == null) return null;

                var firstIngredientToken = responseObj.data[0];
                var ingredient = firstIngredientToken.ToObject<Models.IngredientDto>();

                return ingredient;
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
                var resp = await _httpClient.GetAsync($"api/recipes/by-fridge/{userId}");
                var responseString = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения рецептов по холодильнику: {resp.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null || responseObj.data == null)
                    return new List<RecipeDto>();

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
        public async Task AddFridgeItem(int userId, Models.IngredientDto productName)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(productName);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/inventory/FridgeItem/add/{userId}", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"HTTP {response.StatusCode}: {responseBody}"
                    );
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
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

                var response = await _httpClient.PostAsync($"api/auth/register", formData);
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
                var response = await _httpClient.GetAsync($"api/recipes");
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
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObj.data.ToString()) ?? new List<RecipeDto>();

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
                var response = await _httpClient.GetAsync($"api/recipes/{id}");
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
                var response = await _httpClient.GetAsync($"api/favorites");
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
                    return JsonConvert.DeserializeObject<List<RecipeDto>>(responseObj.data.ToString()) ?? new List<RecipeDto>();

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

                var response = await _httpClient.PostAsync($"api/recipes/{recipeId}/favorite/{userId}", null);
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

                var response = await _httpClient.GetAsync($"api/menu/user/{userId}/current");
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Ошибка получения меню: {response.StatusCode}");

                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObj == null)
                    return null;

                bool success = responseObj.success ?? false;
                if (!success)
                    return null;

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

                var response = await _httpClient.GetAsync($"api/shoppinglist/user/{userId}/current");
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

                var response = await _httpClient.PostAsync($"api/shoppinglist/generate-from-menu/{menuId}/{userId}", null);
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

        public async Task<int?> CreateIngredientByNameAsync(string name)
        {
            try
            {
                var newIng = new { Name = name };
                var response = await PostJsonAsync("api/ingredients/create", newIng, ensureSuccess: false);

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (responseObj != null && responseObj.success == true && responseObj.data != null)
                    return (int)responseObj.data.id;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateIngredientByNameAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<IngredientDto>> GetIngredientsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/ingredients/search");
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
                    return JsonConvert.DeserializeObject<List<IngredientDto>>(responseObj.data.ToString()) ?? new List<IngredientDto>();

                return new List<IngredientDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetIngredientsAsync error: {ex.Message}");
                return new List<IngredientDto>();
            }
        }

        public async Task<bool> AddProductsToFridgeAsync(int userId, List<string> productNames)
        {
            try
            {
                bool allSuccess = true;
                foreach (var name in productNames)
                {
                    var trimmedName = name.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedName)) continue;

                    var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(trimmedName), "productName");
                    formData.Add(new StringContent("1"), "quantity");
                    formData.Add(new StringContent("шт"), "unit");

                    var response = await _httpClient.PostAsync($"api/inventory/add/{userId}", formData);

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
                Console.WriteLine($"AddProductsToFridgeAsync error: {ex.Message}");
                return false;
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

                var response = await _httpClient.PostAsync($"api/inventory/add/{userId}", formData);
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

        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            try
            {
                Console.WriteLine($"[DeleteMenuAsync] Удаляем меню {menuId}");
                
                var response = await _httpClient.DeleteAsync($"api/menu/{menuId}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[DeleteMenuAsync] Меню {menuId} успешно удалено");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[DeleteMenuAsync] Error: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteMenuAsync] Exception: {ex.Message}");
                return false;
            }
        }

    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }

    public class AvailableMenu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RecipeCount { get; set; }
        public int TotalDays { get; set; }
    }

    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuItemDto> Items { get; set; }
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

    public class MenuDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuItemDataDto> Items { get; set; }
    }

    public class MenuItemDataDto
    {
        public int RecipeId { get; set; }
        public string RecipeTitle { get; set; }
        public DateTime Date { get; set; }
        public string MealType { get; set; }
    }
}
