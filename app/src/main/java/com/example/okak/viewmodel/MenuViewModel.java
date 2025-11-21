package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MenuViewModel extends AndroidViewModel {

    private final MutableLiveData<ApiService.MenuDetail> currentMenuLiveData = new MutableLiveData<>();
    private final MutableLiveData<List<ApiService.MenuShort>> menuHistoryLiveData = new MutableLiveData<>();
    private final MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private final MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    private int currentUserId = -1;

    public MenuViewModel(@NonNull Application application) {
        super(application);
    }

    public LiveData<ApiService.MenuDetail> getCurrentMenu() { return currentMenuLiveData; }
    public LiveData<List<ApiService.MenuShort>> getMenuHistory() { return menuHistoryLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }

    public void setUserId(int userId) {
        this.currentUserId = userId;
    }

    public void loadCurrentMenu() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не авторизован");
            return;
        }

        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getCurrentMenu(currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.MenuDetail>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.MenuDetail>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.MenuDetail>> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            currentMenuLiveData.setValue(response.body().data);
                        } else {
                            // It's acceptable to have no menu
                            errorLiveData.setValue("Меню не найдено");
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.MenuDetail>> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }

    public void generateMenu(int days, Double targetCalories, List<String> cuisines,
                             List<String> mealTypes, boolean useInventory) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Пользователь не авторизован");
            return;
        }

        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        // Handle nulls for safe API call
        List<String> safeCuisines = cuisines != null ? cuisines : new ArrayList<>();
        List<String> safeMealTypes = mealTypes != null ? mealTypes : new ArrayList<>();

        // Note: Passing null for targetCalories is fine for Retrofit Double objects, it sends null.

        apiService.generateMenu(currentUserId, days, targetCalories, safeCuisines, safeMealTypes, useInventory)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.MenuShort>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.MenuShort>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.MenuShort>> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            loadCurrentMenu();
                        } else {
                            String errMsg = "Ошибка генерации меню";
                            if (response.body() != null && response.body().message != null) {
                                errMsg = response.body().message;
                            } else if (response.errorBody() != null) {
                                errMsg += " (Code: " + response.code() + ")";
                            }
                            errorLiveData.setValue(errMsg);
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.MenuShort>> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }
}