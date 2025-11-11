package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import java.util.List;

public class MenuViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.MenuDetail> currentMenuLiveData = new MutableLiveData<>();
    private MutableLiveData<List<ApiService.MenuShort>> historyLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    public MenuViewModel(@NonNull Application application) {
        super(application);
    }

    public void loadCurrentMenu() {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getCurrentMenu().enqueue(new Callback<ApiService.MenuDetail>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.MenuDetail> call, @NonNull Response<ApiService.MenuDetail> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    currentMenuLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки меню");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.MenuDetail> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка");
            }
        });
    }

    public void generateMenu(int days, Double targetCalories, List<String> cuisines, List<String> mealTypes, boolean useInventory) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.generateMenu(days, targetCalories, cuisines, mealTypes, useInventory).enqueue(new Callback<ApiService.BaseResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.BaseResponse> call, @NonNull Response<ApiService.BaseResponse> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful()) {
                    loadCurrentMenu();
                } else {
                    errorLiveData.setValue("Ошибка генерации");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.BaseResponse> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка");
            }
        });
    }

    public LiveData<ApiService.MenuDetail> getCurrentMenu() { return currentMenuLiveData; }
    public LiveData<List<ApiService.MenuShort>> getHistory() { return historyLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }
}