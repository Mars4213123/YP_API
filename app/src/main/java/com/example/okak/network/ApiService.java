package com.example.okak.network;

import com.google.gson.annotations.SerializedName;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    // ============================================
    // Auth Endpoints
    // ============================================

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

    // ============================================
    // Recipe Endpoints
    // ============================================

    @GET("api/Recipes")
    Call<ApiResponse<List<RecipeShort>>> getRecipes(
            @Query("Name") String name,
            @Query("Tags") List<String> tags,
            @Query("ExcludedAllergens") List<String> excludedAllergens,
            @Query("CuisineTypes") List<String> cuisineTypes,
            @Query("MaxPrepTime") Integer maxPrepTime,
            @Query("MaxCookTime") Integer maxCookTime,
            @Query("MaxCalories") Double maxCalories,
            @Query("Difficulty") String difficulty,
            @Query("SortBy") String sortBy,
            @Query("SortDescending") Boolean sortDescending,
            @Query("PageNumber") Integer pageNumber,
            @Query("PageSize") Integer pageSize
    );

    class ApiResponse<T> {
        @SerializedName("success")
        public boolean success;
        @SerializedName("message")
        public String message;
        @SerializedName("error")
        public String error;
        @SerializedName("data")
        public T data;
    }

    @GET("api/Recipes/{id}")
    Call<ApiResponse<RecipeDetail>> getRecipeDetail(
            @Path("id") int recipeId,
            @Query("userId") Integer userId
    );

    @POST("api/Recipes/{id}/favorite/{userId}")
    Call<ApiResponse<ToggleFavoriteResponse>> toggleFavorite(
            @Path("id") int recipeId,
            @Path("userId") int userId
    );

    @GET("api/Recipes/favorites/{userId}")
    Call<ApiResponse<List<RecipeShort>>> getFavorites(
            @Path("userId") int userId
    );

    // ============================================
    // Menu Endpoints
    // ============================================

    @GET("api/Menu/current/{userId}")
    Call<ApiResponse<MenuDetail>> getCurrentMenu(
            @Path("userId") int userId
    );

    @FormUrlEncoded
    @POST("api/Menu/generate/{userId}")
    Call<ApiResponse<MenuShort>> generateMenu(
            @Path("userId") int userId,
            @Field("days") int days,
            @Field("targetCaloriesPerDay") Double targetCaloriesPerDay,
            @Field("cuisineTags") List<String> cuisineTags,
            @Field("mealTypes") List<String> mealTypes,
            @Field("useInventory") boolean useInventory
    );

    @GET("api/Menu/history/{userId}")
    Call<ApiResponse<List<MenuShort>>> getMenuHistory(
            @Path("userId") int userId
    );

    // ============================================
    // User Endpoints
    // ============================================

    @GET("api/User/profile/{userId}")
    Call<UserProfile> getUserProfile(
            @Path("userId") int userId
    );

    @FormUrlEncoded
    @POST("api/User/profile/{userId}")
    Call<BaseResponse> updateUserProfile(
            @Path("userId") int userId,
            @Field("FullName") String fullName,
            @Field("Email") String email
    );

    @GET("api/User/allergies/{userId}")
    Call<List<String>> getUserAllergies(
            @Path("userId") int userId
    );

    @POST("api/User/allergies/{userId}")
    Call<BaseResponse> updateUserAllergies(
            @Path("userId") int userId,
            @Body UpdateAllergiesDto allergiesDto
    );

    // ============================================
    // Shopping List Endpoints
    // ============================================

    @GET("api/ShoppingList/current/{userId}")
    Call<ShoppingList> getCurrentShoppingList(
            @Path("userId") int userId
    );

    @POST("api/ShoppingList/generate-from-menu/{menuId}/{userId}")
    Call<ApiResponse<GenerateShoppingListResponse>> generateShoppingList(
            @Path("menuId") int menuId,
            @Path("userId") int userId
    );

    @FormUrlEncoded
    @POST("api/ShoppingList/{listId}/items/{itemId}/toggle")
    Call<BaseResponse> toggleShoppingListItem(
            @Path("listId") int listId,
            @Path("itemId") int itemId,
            @Field("isPurchased") boolean isPurchased
    );

    // ============================================
    // DTOs and Response Classes
    // ============================================

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
        @SerializedName("message")
        public String message;
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
        public double totalCalories;

        @SerializedName("meals")
        public List<Meal> meals;
    }

    class Meal {
        @SerializedName("id")
        public int id;
        @SerializedName("recipeId")
        public int recipeId;
        @SerializedName("recipeTitle")
        public String recipeTitle;
        @SerializedName("mealType")
        public String mealType;
        @SerializedName("calories")
        public double calories;
        @SerializedName("imageUrl")
        public String imageUrl;
    }

    class ShoppingList {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("isCompleted")
        public boolean isCompleted;
        @SerializedName("items")
        public List<ShoppingListItem> items;
    }

    class ShoppingListItem {
        @SerializedName("id")
        public int id;
        @SerializedName("ingredientName")
        public String name;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
        @SerializedName("isPurchased")
        public boolean isBought;
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
        @SerializedName("allergies")
        public List<String> allergies;
    }

    class ToggleFavoriteResponse {
        @SerializedName("recipeId")
        public int recipeId;
        @SerializedName("isFavorite")
        public boolean isFavorite;
    }

    class GenerateShoppingListResponse {
        @SerializedName("id")
        public int id;
        @SerializedName("name")
        public String name;
        @SerializedName("message")
        public String message;
        @SerializedName("itemsCount")
        public int itemsCount;
    }

    class UpdateAllergiesDto {
        @SerializedName("Allergies")
        public List<String> allergies;
        public UpdateAllergiesDto(List<String> allergies) {
            this.allergies = allergies;
        }
    }
}