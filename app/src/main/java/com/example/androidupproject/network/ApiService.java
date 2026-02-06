package com.example.androidupproject.network;

import com.example.androidupproject.models.IngredientDto;
import com.example.androidupproject.models.LoginResponse;
import com.example.androidupproject.models.MenuDto;
import com.example.androidupproject.models.RecipeDto;

import java.util.List;
import retrofit2.Call;
import retrofit2.http.*;

public interface ApiService {

    @FormUrlEncoded
    @POST("api/Auth/login")
    Call<LoginResponse> login(
            @Field("username") String username,
            @Field("password") String password
    );

    @GET("api/Ingredients/fridge/{userId}")
    Call<ApiResponse<List<IngredientDto>>> getFridge(@Path("userId") int userId);

    // --- ИСПРАВЛЕННЫЙ МЕТОД ---
    // @Body отправляет данные как JSON
    @POST("api/Inventory/FridgeItem/add/{userId}")
    Call<ApiResponse<Void>> addToFridge(
            @Path("userId") int userId,
            @Body IngredientDto ingredient
    );
    // ---------------------------

    @DELETE("api/Inventory/remove/{userId}/{ingredientId}")
    Call<ApiResponse<Void>> removeFromFridge(
            @Path("userId") int userId,
            @Path("ingredientId") int ingredientId
    );

    @GET("api/menu/user/{userId}/current")
    Call<ApiResponse<MenuDto>> getCurrentMenu(@Path("userId") int userId);

    // Получить детали рецепта
    @GET("api/recipes/{id}")
    Call<ApiResponse<RecipeDto>> getRecipe(@Path("id") int id);

    // Сгенерировать меню (если его нет)
    @POST("api/menu/generate-week/{userId}")
    Call<ApiResponse<Void>> generateMenu(@Path("userId") int userId);

    // Добавить в список покупок из меню
    @POST("api/shoppinglist/generate-from-menu/{menuId}/{userId}")
    Call<ApiResponse<Void>> generateShoppingList(@Path("menuId") int menuId, @Path("userId") int userId);

}