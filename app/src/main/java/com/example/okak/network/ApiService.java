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
    class BaseResponse {
        @SerializedName("success")
        public boolean success;
        @SerializedName("message")
        public String message;
        @SerializedName("error")
        public String error;
    }
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
    @GET("api/Recipes")
    Call<ApiResponse<List<RecipeShort>>> getRecipes(
            @Query("Name") String name,
            @Query("CuisineTypes") List<String> cuisineTypes,
            @Query("Difficulty") String difficulty,
            @Query("MaxPrepTime") Integer maxPrepTime,
            @Query("MaxCookTime") Integer maxCookTime,
            @Query("MaxCalories") Integer maxCalories,
            @Query("SortBy") String sortBy,
            @Query("SortDescending") boolean sortDescending,
            @Query("PageNumber") int pageNumber,
            @Query("PageSize") int pageSize,
            @Query("userId") Integer userId
    );
    @GET("api/Recipes/{id}")
    Call<ApiResponse<RecipeDetail>> getRecipeDetail(@Path("id") int recipeId, @Query("userId") Integer userId);
    @POST("api/Recipes/{id}/favorite/{userId}")
    Call<ApiResponse<ToggleFavoriteResponse>> toggleFavorite(
            @Path("id") int recipeId,
            @Path("userId") int userId
    );
    @GET("api/Recipes/favorites/{userId}")
    Call<ApiResponse<List<RecipeShort>>> getFavorites(
            @Path("userId") int userId
    );
    @GET("api/Recipes/cuisines")
    Call<List<String>> getCuisines();
    @GET("api/Recipes/difficulties")
    Call<List<String>> getDifficulties();
    @GET("api/Menu/current/{userId}")
    Call<ApiResponse<MenuDetail>> getCurrentMenu(@Path("userId") int userId);
    @POST("api/Menu/generate/{userId}")
    @FormUrlEncoded
    Call<ApiResponse<MenuShort>> generateMenu(
            @Path("userId") int userId,
            @Field("days") int days,
            @Field("targetCaloriesPerDay") Double targetCalories,
            @Field("cuisineTags[]") List<String> cuisines,
            @Field("mealTypes[]") List<String> mealTypes,
            @Field("useInventory") boolean useInventory
    );
    @GET("api/Menu/history/{userId}")
    Call<ApiResponse<List<MenuShort>>> getMenuHistory(@Path("userId") int userId);
    @GET("api/User/profile/{userId}")
    Call<UserProfile> getUserProfile(@Path("userId") int userId);
    @FormUrlEncoded
    @POST("api/User/profile/{userId}")
    Call<BaseResponse> updateUserProfile(
            @Path("userId") int userId,
            @Field("FullName") String fullName,
            @Field("Email") String email
    );
    @GET("api/User/allergies/{userId}")
    Call<List<String>> getUserAllergies(@Path("userId") int userId);
    @POST("api/User/allergies/{userId}")
    Call<BaseResponse> updateUserAllergies(
            @Path("userId") int userId,
            @Body UpdateAllergiesDto allergiesDto
    );
    @GET("api/Inventory/{userId}")
    Call<ApiResponse<List<InventoryItem>>> getInventory(@Path("userId") int userId);
    @FormUrlEncoded
    @POST("api/Inventory/add/{userId}")
    Call<ApiResponse<InventoryItem>> addInventoryItem(
            @Path("userId") int userId,
            @Field("productName") String productName,
            @Field("quantity") double quantity,
            @Field("unit") String unit
    );
    @DELETE("api/Inventory/{userId}/items/{itemId}")
    Call<BaseResponse> deleteInventoryItem(
            @Path("userId") int userId,
            @Path("itemId") int itemId
    );
    @GET("api/ShoppingList/current/{userId}")
    Call<ShoppingList> getCurrentShoppingList(@Path("userId") int userId);
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
        @SerializedName("allergies")
        public List<String> allergies;
    }
    class InventoryItem {
        @SerializedName("id")
        public int id;
        @SerializedName("productName")
        public String productName;
        @SerializedName("ingredientId")
        public int ingredientId;
        @SerializedName("quantity")
        public double quantity;
        @SerializedName("unit")
        public String unit;
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
    class UpdateAllergiesDto {
        @SerializedName("Allergies")
        public List<String> allergies;
        public UpdateAllergiesDto(List<String> allergies) {
            this.allergies = allergies;
        }
    }
}