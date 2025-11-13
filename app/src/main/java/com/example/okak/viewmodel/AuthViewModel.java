package com.example.okak.viewmodel;

import android.app.Application;
import androidx.annotation.NonNull;
import androidx.lifecycle.AndroidViewModel;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import java.util.ArrayList;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class AuthViewModel extends AndroidViewModel {


    private final MutableLiveData<ApiService.UserAuthResponse> loginResultLiveData = new MutableLiveData<>();
    private final MutableLiveData<ApiService.UserAuthResponse> registerResultLiveData = new MutableLiveData<>();
    private final MutableLiveData<String> errorLiveData = new MutableLiveData<>();
    private final MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);

    public AuthViewModel(@NonNull Application application) {
        super(application);
    }

    public LiveData<ApiService.UserAuthResponse> getLoginResult() {
        return loginResultLiveData;
    }

    public LiveData<ApiService.UserAuthResponse> getRegisterResult() {
        return registerResultLiveData;
    }

    public LiveData<String> getError() {
        return errorLiveData;
    }

    public LiveData<Boolean> getLoading() {
        return loadingLiveData;
    }

    /**
     * Авторизация пользователя
     */
    public void login(String username, String password) {
        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.login(username, password)
                .enqueue(new Callback<ApiService.UserAuthResponse>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.UserAuthResponse> call,
                                           @NonNull Response<ApiService.UserAuthResponse> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().id > 0) {
                                loginResultLiveData.setValue(response.body());
                            } else {
                                errorLiveData.setValue(response.body().message != null ?
                                        response.body().message : "Ошибка авторизации");
                            }
                        } else {
                            errorLiveData.setValue("Неверный логин или пароль");
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.UserAuthResponse> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }

    /**
     * Регистрация пользователя
     */
    public void register(String username, String email, String password, String fullName) {
        if (Boolean.TRUE.equals(loadingLiveData.getValue())) return;

        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.register(username, email, password, fullName, new ArrayList<>())
                .enqueue(new Callback<ApiService.UserAuthResponse>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.UserAuthResponse> call,
                                           @NonNull Response<ApiService.UserAuthResponse> response) {
                        loadingLiveData.setValue(false);

                        if (response.isSuccessful() && response.body() != null) {
                            if (response.body().id > 0) {
                                registerResultLiveData.setValue(response.body());
                            } else {
                                errorLiveData.setValue(response.body().message != null ?
                                        response.body().message : "Ошибка регистрации");
                            }
                        } else {
                            if (response.code() == 400) {
                                errorLiveData.setValue("Пользователь с таким именем уже существует");
                            } else {
                                errorLiveData.setValue("Ошибка регистрации: " + response.code());
                            }
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.UserAuthResponse> call,
                                          @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }
}