package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import com.example.okak.network.AuthTokenManager;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RecipeViewModel extends AndroidViewModel {
    // LiveData
    private final MutableLiveData<List<ApiService.RecipeShort>> recipesLiveData = new MutableLiveData<>();
    private final MutableLiveData<ApiService.RecipeDetail> recipeDetailLiveData = new MutableLiveData<>();
    private final MutableLiveData<List<ApiService.RecipeShort>> favoritesLiveData = new MutableLiveData<>();
    private final MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private final MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    private int currentUserId = -1;

    public RecipeViewModel(@NonNull Application application) {
        super(application);
        currentUserId = AuthTokenManager.getUserId(application);
    }

    // Getters
    public LiveData<List<ApiService.RecipeShort>> getRecipes() { return recipesLiveData; }
    public LiveData<ApiService.RecipeDetail> getRecipeDetail() { return recipeDetailLiveData; }
    public LiveData<List<ApiService.RecipeShort>> getFavorites() { return favoritesLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }

    public void setUserId(int userId) {
        this.currentUserId = userId;
    }

    // ============================================
    // ПОИСК
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

        // ИСПРАВЛЕНИЕ ДЛЯ ПОИСКА: Сервер требует строки не null
        String safeName = (name == null) ? "" : name;
        String safeDifficulty = (difficulty == null) ? "" : difficulty;

        apiService.getRecipes(
                safeName, tags, excludedAllergens, cuisineTypes,
                maxPrepTime, maxCookTime, maxCalories, safeDifficulty,
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
                                response.body().error : "Не удалось загрузить рецепты");
                    }
                } else {
                    // Логирование ошибки валидации (400)
                    if (response.code() == 400) {
                        errorLiveData.setValue("Ничего не найдено (400)");
                    } else {
                        errorLiveData.setValue("Ошибка сервера: " + response.code());
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                  @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }

    public void searchByName(String name) {
        searchRecipes(name, null, null, null, null, null, null, null,
                "Title", false, 1, 20);
    }

    public void loadAllRecipes() {
        searchRecipes(null, null, null, null, null, null, null, null,
                "Title", false, 1, 20);
    }

    // ============================================
    // ДЕТАЛИ РЕЦЕПТА
    // ============================================
    public void loadRecipeDetail(int recipeId) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        // Если ID пользователя есть, отправляем его для проверки "Избранного"
        Integer userIdParam = currentUserId != -1 ? currentUserId : null;

        apiService.getRecipeDetail(recipeId, userIdParam)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.RecipeDetail>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.RecipeDetail>> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().success) {
                                recipeDetailLiveData.setValue(response.body().data);
                            } else {
                                errorLiveData.setValue("Ошибка данных рецепта");
                            }
                        } else {
                            errorLiveData.setValue("Ошибка загрузки: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }

    // ============================================
    // ИЗБРАННОЕ
    // ============================================
    public void toggleFavorite(int recipeId) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Необходимо авторизоваться");
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
                                // Обновляем локально состояние в деталях
                                ApiService.RecipeDetail current = recipeDetailLiveData.getValue();
                                if (current != null && current.id == recipeId) {
                                    current.isFavorite = response.body().data.isFavorite;
                                    recipeDetailLiveData.setValue(current);
                                }
                                // Обновляем список избранного
                                loadFavorites();
                            }
                        } else {
                            errorLiveData.setValue("Не удалось изменить избранное");
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call,
                                          @NonNull Throwable t) {
                        errorLiveData.setValue("Ошибка сети");
                    }
                });
    }

    public void loadFavorites() {
        if (currentUserId == -1) return;

        // Не блокируем UI загрузкой при обновлении списка в фоне
        // loadingLiveData.setValue(true);

        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getFavorites(currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                           @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                        // loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().success) {
                                favoritesLiveData.setValue(response.body().data);
                            }
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                          @NonNull Throwable t) {
                        // loadingLiveData.setValue(false);
                    }
                });
    }
}