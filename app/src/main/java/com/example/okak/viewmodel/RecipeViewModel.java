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
    private final MutableLiveData<List<ApiService.RecipeShort>> recipesLiveData = new MutableLiveData<>();
    private final MutableLiveData<ApiService.RecipeDetail> recipeDetailLiveData = new MutableLiveData<>();
    private final MutableLiveData<List<ApiService.RecipeShort>> favoritesLiveData = new MutableLiveData<>();
    private final MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private final MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    private String currentSearchQuery;
    private String currentDifficulty;
    private int currentUserId = -1;

    public RecipeViewModel(@NonNull Application application) {
        super(application);
        currentUserId = AuthTokenManager.getUserId(application);
    }

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

    public void setDifficultyFilter(String difficulty) {
        // ИСПРАВЛЕНО: правильная обработка "Все" и пустых значений
        if (difficulty == null || difficulty.isEmpty() || difficulty.equals("Все") || difficulty.equals("All")) {
            this.currentDifficulty = null;
        } else {
            this.currentDifficulty = difficulty;
        }
        searchByName(currentSearchQuery);
    }

    public void searchByName(String name) {
        this.currentSearchQuery = (name != null && !name.trim().isEmpty()) ? name.trim() : null;
        performSearch();
    }

    public void loadAllRecipes() {
        this.currentSearchQuery = null;
        this.currentDifficulty = null;
        performSearch();
    }

    private void performSearch() {
        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        // ИСПРАВЛЕНО: отправляем null вместо пустых строк
        String safeName = (currentSearchQuery != null && !currentSearchQuery.isEmpty()) ? currentSearchQuery : null;
        String safeDifficulty = (currentDifficulty != null && !currentDifficulty.isEmpty()) ? currentDifficulty : null;

        apiService.getRecipes(
                safeName,
                null,
                null,
                null,
                null,
                null,
                null,
                safeDifficulty,
                "Title",
                false,
                1,
                20
        ).enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                   @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    if (response.body().success) {
                        recipesLiveData.setValue(response.body().data);
                    } else {
                        errorLiveData.setValue(response.body().error);
                    }
                } else {
                    errorLiveData.setValue("Ошибка загрузки рецептов");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }

    public void loadRecipeDetail(int recipeId) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        Integer userIdParam = (currentUserId != -1) ? currentUserId : null;

        apiService.getRecipeDetail(recipeId, userIdParam)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.RecipeDetail>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.RecipeDetail>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            recipeDetailLiveData.setValue(response.body().data);
                        } else {
                            errorLiveData.setValue("Ошибка загрузки деталей");
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети");
                    }
                });
    }

    public void toggleFavorite(int recipeId) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Нужна авторизация");
            return;
        }
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.toggleFavorite(recipeId, currentUserId).enqueue(new Callback<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call, @NonNull Response<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    ApiService.RecipeDetail current = recipeDetailLiveData.getValue();
                    if (current != null && current.id == recipeId) {
                        current.isFavorite = response.body().data.isFavorite;
                        recipeDetailLiveData.setValue(current);
                    }
                    loadFavorites();
                }
            }
            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call, @NonNull Throwable t) {}
        });
    }

    public void loadFavorites() {
        if (currentUserId == -1) return;
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getFavorites(currentUserId).enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call, @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    favoritesLiveData.setValue(response.body().data);
                }
            }
            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call, @NonNull Throwable t) {}
        });
    }
}
