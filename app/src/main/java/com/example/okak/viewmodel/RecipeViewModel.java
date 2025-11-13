package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import com.example.okak.network.AuthTokenManager;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import java.util.Collections;
import java.util.List;

public class RecipeViewModel extends AndroidViewModel {
    private MutableLiveData<List<ApiService.RecipeShort>> recipesLiveData = new MutableLiveData<>();
    private MutableLiveData<ApiService.RecipeDetail> recipeDetailLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();
    private int currentUserId;
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
    public LiveData<Boolean> getLoading() {
        return loadingLiveData;
    }
    public LiveData<String> getError() {
        return errorLiveData;
    }
    public void loadFavorites() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не найден");
            return;
        }
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getFavorites(currentUserId).enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                   @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    recipesLiveData.setValue(response.body().data);
                } else {
                    errorLiveData.setValue("Ошибка загрузки: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }
    public void searchRecipes(String query, String cuisine, String difficulty, Integer maxCalories, int page, int size) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        List<String> cuisineTypes = cuisine != null ? Collections.singletonList(cuisine) : null;

        apiService.getRecipes(query, cuisineTypes, difficulty, null, null, maxCalories, "Title", false, page, size, currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<List<ApiService.RecipeShort>>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call,
                                           @NonNull Response<ApiService.ApiResponse<List<ApiService.RecipeShort>>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            recipesLiveData.setValue(response.body().data);
                        } else if (response.body() != null && !response.body().success) {
                            errorLiveData.setValue(response.body().message != null ? response.body().message : "Ошибка поиска рецептов");
                        } else {
                            errorLiveData.setValue("Ошибка соединения: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<List<ApiService.RecipeShort>>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка поиска: " + t.getMessage());
                    }
                });
    }
    public void loadRecipeDetail(int recipeId) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getRecipeDetail(recipeId, currentUserId).enqueue(new Callback<ApiService.ApiResponse<ApiService.RecipeDetail>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call,
                                   @NonNull Response<ApiService.ApiResponse<ApiService.RecipeDetail>> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    recipeDetailLiveData.setValue(response.body().data);
                } else {
                    errorLiveData.setValue("Ошибка загрузки: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.RecipeDetail>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }
    public void toggleFavorite(int recipeId) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Ошибка: не найден ID пользователя!");
            return;
        }
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.toggleFavorite(recipeId, currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> response) {
                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            // Сразу обновить детальный рецепт и избранные
                            if (recipeDetailLiveData.getValue() != null && recipeDetailLiveData.getValue().id == recipeId) {
                                ApiService.RecipeDetail detail = recipeDetailLiveData.getValue();
                                detail.isFavorite = response.body().data.isFavorite;
                                recipeDetailLiveData.setValue(detail);
                            }
                            loadFavorites();
                        } else if (response.body() != null) {
                            errorLiveData.setValue(response.body().message != null ? response.body().message : "Ошибка избранного");
                        } else {
                            errorLiveData.setValue("Ошибка соединения");
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.ToggleFavoriteResponse>> call, @NonNull Throwable t) {
                        errorLiveData.setValue("Ошибка избранного: " + t.getMessage());
                    }
                });
    }
}
