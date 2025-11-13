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
    // LiveData для списка рецептов
    private final MutableLiveData<List<ApiService.RecipeShort>> recipesLiveData = new MutableLiveData<>();

    // LiveData для детальной информации о рецепте
    private final MutableLiveData<ApiService.RecipeDetail> recipeDetailLiveData = new MutableLiveData<>();

    // LiveData для избранных рецептов
    private final MutableLiveData<List<ApiService.RecipeShort>> favoritesLiveData = new MutableLiveData<>();

    // Общие LiveData
    private final MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private final MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    private int currentUserId = -1;

    public RecipeViewModel(@NonNull Application application) {
        super(application);
    }

// ============================================
// Getters для LiveData
// ============================================

    public LiveData<List<ApiService.RecipeShort>> getRecipes() {
        return recipesLiveData;
    }

    public LiveData<ApiService.RecipeDetail> getRecipeDetail() {
        return recipeDetailLiveData;
    }

    public LiveData<List<ApiService.RecipeShort>> getFavorites() {
        return favoritesLiveData;
    }

    public LiveData<Boolean> getLoading() {
        return loadingLiveData;
    }

    public LiveData<String> getError() {
        return errorLiveData;
    }

    public void setUserId(int userId) {
        this.currentUserId = userId;
    }

// ============================================
// Поиск рецептов
// ============================================

    public void searchRecipes(
            String name,
            List<String> tags,
            List<String> excludedAllergens,
            List<String> cuisineTypes,
            Integer maxPrepTime,
            Integer maxCookTime,
            Double maxCalories,
            String difficulty,
            String sortBy,
            Boolean sortDescending,
            Integer pageNumber,
            Integer pageSize) {

        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getRecipes(
                name, tags, excludedAllergens, cuisineTypes,
                maxPrepTime, maxCookTime, maxCalories, difficulty,
                sortBy, sortDescending, pageNumber, pageSize
        ).enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                   @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                loadingLiveData.setValue(false);

                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().success) {
                        recipesLiveData.setValue(response.body().data);
                    } else {
                        errorLiveData.setValue(response.body().error != null ?
                                response.body().error : "Failed to load recipes");
                    }
                } else {
                    errorLiveData.setValue("Error: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                  @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Network error: " + t.getMessage());
            }
        });
    }

    /**
     * Упрощенный поиск по имени
     */
    public void searchByName(String name) {
        searchRecipes(name, null, null, null, null, null, null, null,
                "Title", false, 1, 20);
    }

    /**
     * Загрузить все рецепты
     */
    public void loadAllRecipes() {
        searchRecipes(null, null, null, null, null, null, null, null,
                "Title", false, 1, 20);
    }

// ============================================
// Детальная информация о рецепте
// ============================================

    public void loadRecipeDetail(int recipeId) {
        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        Integer userId = currentUserId == -1 ? null : currentUserId;

        apiService.getRecipeDetail(recipeId, userId)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.RecipeDetail>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.RecipeDetail>> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().success) {
                                recipeDetailLiveData.setValue(response.body().data);
                            } else {
                                errorLiveData.setValue(response.body().error != null ?
                                        response.body().error : "Failed to load recipe details");
                            }
                        } else {
                            errorLiveData.setValue("Error: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Network error: " + t.getMessage());
                    }
                });
    }

// ============================================
// Избранное
// ============================================

    public void toggleFavorite(int recipeId) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не авторизован");
            return;
        }

        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.toggleFavorite(recipeId, currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> response) {
                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().success) {
                                // Обновить статус избранного в текущем рецепте
                                ApiService.RecipeDetail currentDetail = recipeDetailLiveData.getValue();
                                if (currentDetail != null) {
                                    currentDetail.isFavorite = response.body().data.isFavorite;
                                    recipeDetailLiveData.setValue(currentDetail);
                                }

                                // Перезагрузить избранное
                                loadFavorites();
                            } else {
                                errorLiveData.setValue(response.body().error != null ?
                                        response.body().error : "Failed to toggle favorite");
                            }
                        } else {
                            errorLiveData.setValue("Error: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call,
                                          @NonNull Throwable t) {
                        errorLiveData.setValue("Network error: " + t.getMessage());
                    }
                });
    }

    public void loadFavorites() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не авторизован");
            return;
        }

        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getFavorites(currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                           @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().success) {
                                favoritesLiveData.setValue(response.body().data);
                            } else {
                                errorLiveData.setValue(response.body().error != null ?
                                        response.body().error : "Failed to load favorites");
                            }
                        } else {
                            errorLiveData.setValue("Error: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Network error: " + t.getMessage());
                    }
                });
    }

}