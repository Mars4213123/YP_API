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

public class UserViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.UserProfile> profileLiveData = new MutableLiveData<>();
    private MutableLiveData<List<String>> allergiesLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    public UserViewModel(@NonNull Application application) {
        super(application);
    }

    public void loadProfile() {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getUserProfile().enqueue(new Callback<ApiService.UserProfile>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.UserProfile> call, @NonNull Response<ApiService.UserProfile> response) {
                if (response.isSuccessful() && response.body() != null) {
                    profileLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки профиля: " + response.code());
                }
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
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getUserAllergies().enqueue(new Callback<List<String>>() {
            @Override
            public void onResponse(@NonNull Call<List<String>> call, @NonNull Response<List<String>> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    allergiesLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка загрузки аллергий");
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<String>> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка");
            }
        });
    }

    public LiveData<ApiService.UserProfile> getProfile() { return profileLiveData; }
    public LiveData<List<String>> getAllergies() { return allergiesLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }
}