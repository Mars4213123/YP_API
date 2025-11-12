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
import java.util.List;

public class MenuViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.MenuDetail> currentMenuLiveData = new MutableLiveData<>();
    private MutableLiveData<List<ApiService.MenuShort>> historyLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();
    private int currentUserId = -1;

    public MenuViewModel(@NonNull Application application) {
        super(application);
        currentUserId = AuthTokenManager.getUserId(application);
    }

    public void loadCurrentMenu() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не найден");
            return;
        }
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getCurrentMenu(currentUserId).enqueue(new Callback<ApiService.ApiResponse<ApiService.MenuDetail>>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.MenuDetail>> call,
                                   @NonNull Response<ApiService.ApiResponse<ApiService.MenuDetail>> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    currentMenuLiveData.setValue(response.body().data);
                } else if (response.body() != null && !response.body().success) {
                    currentMenuLiveData.setValue(null);
                } else {
                    errorLiveData.setValue("Ошибка загрузки меню");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.MenuDetail>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }

    public void generateMenu(int days, Double targetCalories, List<String> cuisines, List<String> mealTypes, boolean useInventory) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не найден");
            return;
        }

        if (Boolean.TRUE.equals(loadingLiveData.getValue())) {
            return;
        }

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        // Важно: передаём targetCalories как есть (может быть null), Retrofit корректно обработает
        apiService.generateMenu(currentUserId, days, targetCalories, cuisines, mealTypes, useInventory)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.MenuShort>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.MenuShort>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.MenuShort>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            loadCurrentMenu();
                        } else if (response.body() != null && !response.body().success) {
                            errorLiveData.setValue(response.body().message != null ? response.body().message : "Ошибка генерации меню");
                        } else {
                            errorLiveData.setValue("Ошибка соединения: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.MenuShort>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка генерации меню: " + t.getMessage());
                    }
                });
    }

    public LiveData<ApiService.MenuDetail> getCurrentMenu() {
        return currentMenuLiveData;
    }

    public LiveData<List<ApiService.MenuShort>> getHistory() {
        return historyLiveData;
    }

    public LiveData<Boolean> getLoading() {
        return loadingLiveData;
    }

    public LiveData<String> getError() {
        return errorLiveData;
    }
}
