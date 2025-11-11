package com.example.okak.network; // (замени на свой package)

import com.google.gson.annotations.SerializedName;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.DELETE;
import retrofit2.http.Field;
import retrofit2.http.FormUrlEncoded;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

/**
 * Полностью реализованный интерфейс Retrofit для YP_API.
 * Использует @FormUrlEncoded для соответствия [FromForm] в .NET API.
 * * Токен авторизации должен добавляться АВТОМАТИЧЕСКИ через Interceptor в ApiClient.
 * Этот файл не нужно изменять.
 */
public interface ApiService {

    // ===================================================================================
    //  AUTH CONTROLLER (/api/Auth)
    // ===================================================================================

    /**
     * Регистрация нового пользователя.
     * API: [FromForm]
     */
    @FormUrlEncoded
    @POST("api/Auth/register")
    Call<UserAuthResponse> register(
            @Field("username") String username,
            @Field("email") String email,
            @Field("password") String password,
            @Field("fullName") String fullName,
            @Field("allergies") List<String> allergies // Retrofit отправит это как allergies=item1&allergies=item2
    );

    /**
     * Вход пользователя.
     * API: [FromForm]
     */
    @FormUrlEncoded
    @POST("api/Auth/login")
    Call<UserAuthResponse> login(
            @Field("username") String username, // API ожидает username
            @Field("password") String password
    );

    // ===================================================================================
    //  RECIPES CONTROLLER (/api/Recipes)
    // ===================================================================================

    /**
     * Получение списка всех рецептов с фильтрами.
     * API: [FromQuery] (AllowAnonymous)
     */
    @GET("api/Recipes")
    Call<PagedResult<RecipeShort>> getRecipes(
            @Query("Name") String name,
            @Query("Cuisine") String cuisine,
            @Query("Difficulty") String difficulty,
            @Query("MaxPrepTime") Integer maxPrepTime,
            @Query("MaxCookTime") Integer maxCookTime,
            @Query("MaxCalories") Integer maxCalories,
            @Query("SortBy") String sortBy, // "Title", "Calories", "PrepTime", "CookTime", "Rating"
            @Query("IsAscending") boolean isAscending,
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    /**
     * Получение детальной информации о рецепте.
     * API: (AllowAnonymous)
     */
    @GET("api/Recipes/{id}")
    Call<RecipeDetail> getRecipeDetail(@Path("id") int recipeId);

    /**
     * Создание нового рецепта.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Recipes")
    Call<BaseResponse> createRecipe(
            @Field("Title") String title,
            @Field("Description") String description,
            @Field("Instructions") String instructions,
            @Field("PrepTime") int prepTime,
            @Field("CookTime") int cookTime,
            @Field("Servings") int servings,
            @Field("Calories") double calories,
            @Field("ImageUrl") String imageUrl,
            @Field("CuisineType") String cuisineType,
            @Field("Difficulty") String difficulty,
            @Field("Ingredients") List<String> ingredients // Пример: "2_шт_Яйцо", "200_г_Мука"
    );

    /**
     * Добавить/удалить рецепт из избранного.
     * API: (Authorize)
     */
    @POST("api/Recipes/{id}/favorite")
    Call<BaseResponse> toggleFavorite(@Path("id") int recipeId);

    /**
     * Получить список избранных рецептов пользователя.
     * API: (Authorize)
     */
    @GET("api/Recipes/favorites")
    Call<PagedResult<RecipeShort>> getFavorites(
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    /**
     * Получить список всех кухонь (для фильтров).
     * API: (AllowAnonymous)
     */
    @GET("api/Recipes/cuisines")
    Call<List<String>> getCuisines();

    /**
     * Получить список всех сложностей (для фильтров).
     * API: (AllowAnonymous)
     */
    @GET("api/Recipes/difficulties")
    Call<List<String>> getDifficulties();


    // ===================================================================================
    //  MENU CONTROLLER (/api/Menu)
    // ===================================================================================

    /**
     * Получить текущее активное меню пользователя.
     * API: (Authorize)
     */
    @GET("api/Menu/current")
    Call<MenuDetail> getCurrentMenu();

    /**
     * Сгенерировать новое меню.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Menu/generate")
    Call<BaseResponse> generateMenu(
            @Field("Days") int days,
            @Field("TargetCaloriesPerDay") Double targetCalories,
            @Field("CuisineTags") List<String> cuisineTags,
            @Field("MealTypes") List<String> mealTypes, // "breakfast", "lunch", "dinner"
            @Field("UseInventory") boolean useInventory
    );

    /**
     * Получить историю меню пользователя.
     * API: (Authorize)
     */
    @GET("api/Menu/history")
    Call<PagedResult<MenuShort>> getMenuHistory(
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    /**
     * Перегенерировать один день в меню.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Menu/{menuId}/regenerate-day")
    Call<BaseResponse> regenerateDay(
            @Path("menuId") int menuId,
            @Field("Date") String date // "YYYY-MM-DD"
    );

    /**
     * Установить рейтинг для блюда в меню (для будущих рекомендаций).
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Menu/{menuId}/set-rating")
    Call<BaseResponse> setMenuRating(
            @Path("menuId") int menuId,
            @Field("RecipeId") int recipeId,
            @Field("Rating") int rating // 1-5
    );

    /**
     * Удалить меню.
     * API: (Authorize)
     */
    @DELETE("api/Menu/{menuId}")
    Call<BaseResponse> deleteMenu(@Path("menuId") int menuId);


    // ===================================================================================
    //  USER CONTROLLER (/api/User)
    // ===================================================================================

    /**
     * Получить профиль текущего пользователя.
     * API: (Authorize)
     */
    @GET("api/User/profile")
    Call<UserProfile> getUserProfile();

    /**
     * Обновить профиль текущего пользователя.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/User/profile")
    Call<BaseResponse> updateUserProfile(
            @Field("FullName") String fullName,
            @Field("Email") String email
    );

    /**
     * Получить список аллергий пользователя.
     * API: (Authorize)
     */
    @GET("api/User/allergies")
    Call<List<String>> getUserAllergies();

    /**
     * Обновить/установить список аллергий пользователя.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/User/allergies")
    Call<BaseResponse> updateUserAllergies(
            @Field("allergies") List<String> allergies
    );


    // ===================================================================================
    //  INVENTORY CONTROLLER (/api/Inventory)
    // ===================================================================================

    /**
     * Получить инвентарь (продукты) пользователя.
     * API: (Authorize)
     */
    @GET("api/Inventory")
    Call<List<InventoryItem>> getInventory();

    /**
     * Добавить продукт в инвентарь.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Inventory")
    Call<BaseResponse> addInventoryItem(
            @Field("IngredientName") String ingredientName,
            @Field("Quantity") double quantity,
            @Field("Unit") String unit
    );

    /**
     * Удалить продукт из инвентаря.
     * API: (Authorize)
     */
    @DELETE("api/Inventory/{ingredientId}")
    Call<BaseResponse> deleteInventoryItem(@Path("ingredientId") int ingredientId);

    /**
     * Обновить количество продукта в инвентаре.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/Inventory/update-quantity")
    Call<BaseResponse> updateInventoryQuantity(
            @Field("IngredientId") int ingredientId,
            @Field("NewQuantity") double newQuantity
    );


    // ===================================================================================
    //  SHOPPING LIST CONTROLLER (/api/ShoppingList)
    // ===================================================================================

    /**
     * Получить текущий список покупок (основанный на текущем меню).
     * API: (Authorize)
     */
    @GET("api/ShoppingList/current")
    Call<ShoppingList> getCurrentShoppingList();

    /**
     * Сгенерировать список покупок для конкретного меню.
     * API: [FromForm] (Authorize)
     */
    @FormUrlEncoded
    @POST("api/ShoppingList/generate")
    Call<ShoppingList> generateShoppingList(
            @Field("MenuId") int menuId
    );

    /**
     * Отметить продукт в списке покупок как купленный/не купленный.
     * API: (Authorize)
     */
    @FormUrlEncoded
    @POST("api/ShoppingList/{listId}/items/{itemId}/toggle")
    Call<BaseResponse> toggleShoppingListItem(
            @Path("listId") int listId,
            @Path("itemId") int itemId,
            @Field("isPurchased") boolean isPurchased
    );


    // ===================================================================================
    //  ВНУТРЕННИЕ КЛАССЫ ДЛЯ МОДЕЛЕЙ ДАННЫХ (DTO)
    //  (Эти классы используются для парсинга ответов от сервера)
    // ===================================================================================

    /**
     * Базовый ответ для успешных операций (POST, DELETE).
     */
    class BaseResponse {
        @SerializedName("success")
        public boolean success;
        @SerializedName("message")
        public String message;
        @SerializedName("error")
        public String error;
    }

    /**
     * Ответ при Логине и Регистрации.
     */
    class UserAuthResponse {
        @SerializedName("id")
        public int id;
        @SerializedName("username")
        public String username;
        @SerializedName("email")
        public String email;
        @SerializedName("fullName")
        public String fullName;
        @SerializedName("token")
        public String token; // Самое важное
    }

    /**
     * Обертка для ответов с пагинацией (списки рецептов, истории меню).
     */
    class PagedResult<T> {
        @SerializedName("items")
        public List<T> items;
        @SerializedName("totalCount")
        public int totalCount;
        @SerializedName("pageNumber")
        public int pageNumber;
        @SerializedName("pageSize")
        public int pageSize;
        @SerializedName("totalPages")
        public int totalPages;
    }

    /**
     * Краткая информация о рецепте (для списков).
     */
    class RecipeShort {
        @SerializedName("id")
        public int id;
        @SerializedName("title")
        public String title;
        @SerializedName("imageUrl")
        public String imageUrl;
        @SerializedName("calories")
        public double calories;
        @SerializedName("prepTime")
        public int prepTime;
        @SerializedName("cookTime")
        public int cookTime;
        @SerializedName("cuisineType")
        public String cuisineType;
        @SerializedName("difficulty")
        public String difficulty;
        @SerializedName("isFavorite")
        public boolean isFavorite;
    }

    /**
     * Полная информация о рецепте.
     */
    class RecipeDetail extends RecipeShort {
        @SerializedName("description")
        public String description;
        @SerializedName("instructions")
        public String instructions;
        @SerializedName("servings")
        public int servings;
        @SerializedName("ingredients")
        public List<Ingredient> ingredients;
    }

    /**
     * Ингредиент в рецепте.
     */
    class Ingredient {
        @SerializedName("name")
        public String name;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
    }

    /**
     * Профиль пользователя.
     */
    class UserProfile {
        @SerializedName("username")
        public String username;
        @SerializedName("email")
        public String email;
        @SerializedName("fullName")
        public String fullName;
        @SerializedName("createdAt")
        public String createdAt; // "YYYY-MM-DDTHH:mm:ss"
    }

    /**
     * Продукт в инвентаре.
     */
    class InventoryItem {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
    }

    /**
     * Список покупок.
     */
    class ShoppingList {
        @SerializedName("id")
        public int id;
        @SerializedName("menuId")
        public int menuId;
        @SerializedName("createdAt")
        public String createdAt;
        @SerializedName("items")
        public List<ShoppingListItem> items;
    }

    /**
     * Продукт в списке покупок.
     */
    class ShoppingListItem {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
        @SerializedName("isBought")
        public boolean isBought;
    }

    /**
     * Краткая информация о меню (для истории).
     */
    class MenuShort {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("startDate")
        public String startDate; // "YYYY-MM-DD"
        @SerializedName("endDate")
        public String endDate; // "YYYY-MM-DD"
        @SerializedName("totalCalories")
        public double totalCalories;
    }

    /**
     * Полная информация о меню (для /current).
     */
    class MenuDetail extends MenuShort {
        @SerializedName("days")
        public List<MenuDay> days;
    }

    /**
     * Один день в меню.
     */
    class MenuDay {
        @SerializedName("date")
        public String date; // "YYYY-MM-DD"
        @SerializedName("totalCalories")
        public double totalCalories;
        @SerializedName("meals")
        public List<Meal> meals;
    }

    /**
     * Прием пищи (Завтрак, Обед, Ужин).
     */
    class Meal {
        @SerializedName("mealType")
        public String mealType; // "breakfast", "lunch", "dinner"
        @SerializedName("recipe")
        public RecipeShort recipe; // Вложенный рецепт
    }
}