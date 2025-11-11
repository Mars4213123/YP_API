package com.example.okak.viewmodel;

import android.app.Application;

import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;

import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;

import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RecipeViewModel extends AndroidViewModel {
    private MutableLiveData<List<ApiService.RecipeShort>> recipesLiveData = new MutableLiveData<>();
    private MutableLiveData<ApiService.RecipeDetail> recipeDetailLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    public RecipeViewModel(@NonNull Application application) {
        super(application);
    }

    public LiveData<List<ApiService.RecipeShort>> getRecipes() {
        return recipesLiveData;
    }

    public LiveData<ApiService.RecipeDetail> getRecipeDetail() {
        return recipeDetailLiveData;
    }

    // --- ИСПРАВЛЕНИЕ: Добавлен новый метод ---
    public void loadFavorites() {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getFavorites(1, 50) // Загружаем до 50 избранных
                .enqueue(new Callback<ApiService.PagedResult<ApiService.RecipeShort>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.PagedResult<ApiService.RecipeShort>> call, @NonNull Response<ApiService.PagedResult<ApiService.RecipeShort>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null) {
                            recipesLiveData.setValue(response.body().items);
                        } else {
                            errorLiveData.setValue("Ошибка загрузки избранного: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.PagedResult<ApiService.RecipeShort>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Сетевая ошибка: " + t.getMessage());
                    }
                });
    }
    // --- КОНЕЦ ИСПРАВЛЕНИЯ ---

    public void searchRecipes(String query, String cuisine, String difficulty, Integer maxCalories, int page, int size) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getRecipes(query, cuisine, difficulty, null, null, maxCalories, "Title", true, page, size)
                .enqueue(new Callback<ApiService.PagedResult<ApiService.RecipeShort>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.PagedResult<ApiService.RecipeShort>> call, @NonNull Response<ApiService.PagedResult<ApiService.RecipeShort>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null) {
                            recipesLiveData.setValue(response.body().items);
                        } else {
                            errorLiveData.setValue("Ошибка поиска: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.PagedResult<ApiService.RecipeShort>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Сетевая ошибка: " + t.getMessage());
                    }
                });
    }

    public void loadRecipeDetail(int recipeId) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getRecipeDetail(recipeId).enqueue(new Callback<ApiService.RecipeDetail>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.RecipeDetail> call, @NonNull Response<ApiService.RecipeDetail> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    recipeDetailLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.RecipeDetail> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка: " + t.getMessage());
            }
        });
    }

    public void toggleFavorite(int recipeId) {
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.toggleFavorite(recipeId).enqueue(new Callback<ApiService.BaseResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.BaseResponse> call, @NonNull Response<ApiService.BaseResponse> response) {
                if (response.isSuccessful()) {
                    // Обновить текущий детальный рецепт, если он загружен
                    if (recipeDetailLiveData.getValue() != null && recipeDetailLiveData.getValue().id == recipeId) {
                        ApiService.RecipeDetail detail = recipeDetailLiveData.getValue();
                        detail.isFavorite = !detail.isFavorite;
                        recipeDetailLiveData.setValue(detail);
                    }
                    // Обновить список избранного
                    loadFavorites();
                } else {
                    errorLiveData.setValue("Ошибка избранного");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.BaseResponse> call, @NonNull Throwable t) {
                errorLiveData.setValue("Сетевая ошибка");
            }
        });
    }

    // Геттеры для loading и error
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }
}