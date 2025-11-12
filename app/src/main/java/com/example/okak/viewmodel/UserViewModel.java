package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import com.example.okak.network.AuthTokenManager; // <-- ИМПОРТ
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import java.util.List;

public class UserViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.UserProfile> profileLiveData = new MutableLiveData<>();
    private MutableLiveData<List<String>> allergiesLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();
    private int currentUserId; // <-- ДОБАВЛЕНО

    public UserViewModel(@NonNull Application application) {
        super(application);
        // Получаем ID пользователя при создании ViewModel
        currentUserId = AuthTokenManager.getUserId(application);
    }

    public void loadProfile() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Ошибка: Пользователь не авторизован");
            return;
        }
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        // ИСПРАВЛЕНО: Используем сохраненный userId
        apiService.getUserProfile(currentUserId).enqueue(new Callback<ApiService.UserProfile>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.UserProfile> call, @NonNull Response<ApiService.UserProfile> response) {
                if (response.isSuccessful() && response.body() != null) {
                    profileLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки профиля: " + response.code());
                }
                // Загружаем аллергии *после* профиля
                loadAllergies();
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.UserProfile> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка: " + t.getMessage());
            }
        });
    }

    private void loadAllergies() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Ошибка: Пользователь не авторизован");
            loadingLiveData.setValue(false);
            return;
        }
        ApiService apiService = ApiClient.getApiService(getApplication());

        // ИСПРАВЛЕНО: Используем сохраненный userId
        apiService.getUserAllergies(currentUserId).enqueue(new Callback<List<String>>() {
            @Override
            public void onResponse(@NonNull Call<List<String>> call, @NonNull Response<List<String>> response) {
                loadingLiveData.setValue(false); // Завершаем загрузку здесь
                if (response.isSuccessful() && response.body() != null) {
                    allergiesLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки аллергий: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<String>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка: " + t.getMessage());
            }
        });
    }

    public LiveData<ApiService.UserProfile> getProfile() { return profileLiveData; }
    public LiveData<List<String>> getAllergies() { return allergiesLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }
}