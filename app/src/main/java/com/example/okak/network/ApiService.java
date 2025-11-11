package com.example.okak.network;

import com.google.gson.annotations.SerializedName;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.Field;
import retrofit2.http.FormUrlEncoded;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface ApiService {
    // Auth
    @FormUrlEncoded
    @POST("api/Auth/register")
    Call<UserAuthResponse> register(
            @Field("username") String username,
            @Field("email") String email,
            @Field("password") String password,
            @Field("fullName") String fullName,
            @Field("allergies") List<String> allergies
    );

    @FormUrlEncoded
    @POST("api/Auth/login")
    Call<UserAuthResponse> login(
            @Field("username") String username,
            @Field("password") String password
    );

    // Recipes
    @GET("api/Recipes")
    Call<PagedResult<RecipeShort>> getRecipes(
            @Query("Name") String name,
            @Query("Cuisine") String cuisine,
            @Query("Difficulty") String difficulty,
            @Query("MaxPrepTime") Integer maxPrepTime,
            @Query("MaxCookTime") Integer maxCookTime,
            @Query("MaxCalories") Integer maxCalories,
            @Query("SortBy") String sortBy,
            @Query("IsAscending") boolean isAscending,
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    @GET("api/Recipes/{id}")
    Call<RecipeDetail> getRecipeDetail(@Path("id") int recipeId);

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
            @Field("Ingredients") List<String> ingredients
    );

    @POST("api/Recipes/{id}/favorite")
    Call<BaseResponse> toggleFavorite(@Path("id") int recipeId);

    @GET("api/Recipes/favorites")
    Call<PagedResult<RecipeShort>> getFavorites(
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    @GET("api/Recipes/cuisines")
    Call<List<String>> getCuisines();

    @GET("api/Recipes/difficulties")
    Call<List<String>> getDifficulties();

    // Menu
    @GET("api/Menu/current")
    Call<MenuDetail> getCurrentMenu();

    @FormUrlEncoded
    @POST("api/Menu/generate")
    Call<BaseResponse> generateMenu(
            @Field("Days") int days,
            @Field("TargetCaloriesPerDay") Double targetCalories,
            @Field("CuisineTags") List<String> cuisineTags,
            @Field("MealTypes") List<String> mealTypes,
            @Field("UseInventory") boolean useInventory
    );

    @GET("api/Menu/history")
    Call<PagedResult<MenuShort>> getMenuHistory(
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize
    );

    @FormUrlEncoded
    @POST("api/Menu/{menuId}/regenerate-day")
    Call<BaseResponse> regenerateDay(
            @Path("menuId") int menuId,
            @Field("Date") String date
    );

    @FormUrlEncoded
    @POST("api/Menu/{menuId}/set-rating")
    Call<BaseResponse> setMenuRating(
            @Path("menuId") int menuId,
            @Field("RecipeId") int recipeId,
            @Field("Rating") int rating
    );

    @DELETE("api/Menu/{menuId}")
    Call<BaseResponse> deleteMenu(@Path("menuId") int menuId);

    // User
    @GET("api/User/profile")
    Call<UserProfile> getUserProfile();

    @FormUrlEncoded
    @POST("api/User/profile")
    Call<BaseResponse> updateUserProfile(
            @Field("FullName") String fullName,
            @Field("Email") String email
    );

    @GET("api/User/allergies")
    Call<List<String>> getUserAllergies();

    @POST("api/User/allergies")
    Call<BaseResponse> updateUserAllergies(
            @Body UpdateAllergiesDto allergiesDto
    );

    // Inventory
    @GET("api/Inventory")
    Call<List<InventoryItem>> getInventory();

    @FormUrlEncoded
    @POST("api/Inventory")
    Call<BaseResponse> addInventoryItem(
            @Field("IngredientName") String ingredientName,
            @Field("Quantity") double quantity,
            @Field("Unit") String unit
    );

    @DELETE("api/Inventory/{ingredientId}")
    Call<BaseResponse> deleteInventoryItem(@Path("ingredientId") int ingredientId);

    @FormUrlEncoded
    @POST("api/Inventory/update-quantity")
    Call<BaseResponse> updateInventoryQuantity(
            @Field("IngredientId") int ingredientId,
            @Field("NewQuantity") double newQuantity
    );

    // Shopping List
    @GET("api/ShoppingList/current")
    Call<ShoppingList> getCurrentShoppingList();

    @FormUrlEncoded
    @POST("api/ShoppingList/generate-from-menu/{menuId}")
    Call<ShoppingList> generateShoppingList(
            @Path("menuId") int menuId
    );

    @FormUrlEncoded
    @POST("api/ShoppingList/{listId}/items/{itemId}/toggle")
    Call<BaseResponse> toggleShoppingListItem(
            @Path("listId") int listId,
            @Path("itemId") int itemId,
            @Field("isPurchased") boolean isPurchased
    );

    // --- DTO ---

    class BaseResponse {
        @SerializedName("success")
        public boolean success;
        @SerializedName("message")
        public String message;
        @SerializedName("error")
        public String error;
    }

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
        public String token;
    }

    class PagedResult<T> {
        @SerializedName("items")
        public List<T> items;
        @SerializedName("totalCount")
        public int totalCount;
        @SerializedName("totalPages")
        public int totalPages;
    }

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

    class Ingredient {
        @SerializedName("name")
        public String name;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
    }

    class UserProfile {
        @SerializedName("username")
        public String username;
        @SerializedName("email")
        public String email;
        @SerializedName("fullName")
        public String fullName;
        @SerializedName("createdAt")
        public String createdAt;
        // ⭐️ ДОБАВЛЕНО: Android-приложение должно ожидать этот список
        @SerializedName("allergies")
        public List<String> allergies;
    }

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

    class MenuShort {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("startDate")
        public String startDate;
        @SerializedName("endDate")
        public String endDate;
        @SerializedName("totalCalories")
        public double totalCalories;
    }

    class MenuDetail extends MenuShort {
        @SerializedName("days")
        public List<MenuDay> days;
    }

    class MenuDay {
        @SerializedName("date")
        public String date;
        @SerializedName("totalCalories")
        public double totalCalories;
        @SerializedName("meals")
        public List<Meal> meals;
    }

    class Meal {
        @SerializedName("mealType")
        public String mealType;
        @SerializedName("recipe")
        public RecipeShort recipe;
    }

    class UpdateAllergiesDto {
        @SerializedName("allergies")
        public List<String> allergies;
        public UpdateAllergiesDto(List<String> allergies) {
            this.allergies = allergies;
        }
    }
}