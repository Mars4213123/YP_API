// Services/ApiService.cs
using System;
using System.Collections.Generic;
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

        public ApiService(string baseUrl = "https://localhost:7000/api/")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
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
                throw new HttpRequestException($"API Error: {response.StatusCode} - {content}");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);

            if (!apiResponse.Success)
            {
                throw new Exception(apiResponse.Message);
            }

            return apiResponse.Data;
        }

        // Auth methods
        public async Task<UserData> LoginAsync(string username, string password)
        {
            var request = new LoginRequest { Username = username, Password = password };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}Auth/login", content);
            var authResponse = await HandleResponse<UserData>(response);

            SetToken(authResponse.Token);
            return authResponse;
        }

        public async Task<UserData> RegisterAsync(string username, string email, string password, List<string> allergies)
        {
            var request = new RegisterRequest
            {
                Username = username,
                Email = email,
                Password = password,
                Allergies = allergies
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}Auth/register", content);
            return await HandleResponse<UserData>(response);
        }

        // Recipe methods
        public async Task<List<RecipeDto>> GetRecipesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Recipes");
            return await HandleResponse<List<RecipeDto>>(response);
        }

        public async Task<RecipeDto> GetRecipeAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/{id}");
            return await HandleResponse<RecipeDto>(response);
        }

        public async Task<List<RecipeDto>> GetFavoritesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Recipes/favorites");
            return await HandleResponse<List<RecipeDto>>(response);
        }

        public async Task<bool> ToggleFavoriteAsync(int recipeId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}Recipes/{recipeId}/favorite", null);
            return response.IsSuccessStatusCode;
        }

        // Menu methods
        public async Task<MenuDto> GenerateMenuAsync(GenerateMenuRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}Menu/generate", content);
            return await HandleResponse<MenuDto>(response);
        }

        public async Task<MenuDto> GetCurrentMenuAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Menu/current");
            return await HandleResponse<MenuDto>(response);
        }

        // Shopping List methods
        public async Task<bool> GenerateShoppingListAsync(int menuId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}ShoppingList/generate-from-menu/{menuId}", null);
            return response.IsSuccessStatusCode;
        }
    }
}