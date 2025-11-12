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

public class ShoppingViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.ShoppingList> shoppingListLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();
    private int currentUserId = -1;

    public ShoppingViewModel(@NonNull Application application) {
        super(application);
        currentUserId = AuthTokenManager.getUserId(application);
    }

    public void loadCurrentShoppingList() {
        if (currentUserId == -1) {
            errorLiveData.setValue("Ошибка: не найден ID пользователя!");
            return;
        }
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.getCurrentShoppingList(currentUserId).enqueue(new Callback<ApiService.ShoppingList>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ShoppingList> call,
                                   @NonNull Response<ApiService.ShoppingList> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    shoppingListLiveData.setValue(response.body());
                } else {
                    shoppingListLiveData.setValue(null);
                    if (response.code() != 404) {
                        errorLiveData.setValue("Ошибка загрузки списка");
                    }
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ShoppingList> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
            }
        });
    }

    public void generateShoppingList(int menuId) {
        if (currentUserId == -1) {
            errorLiveData.setValue("Ошибка: не найден ID пользователя!");
            return;
        }
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.generateShoppingList(menuId, currentUserId)
                .enqueue(new Callback<ApiService.ApiResponse<ApiService.GenerateShoppingListResponse>>() {
                    @Override
                    public void onResponse(@NonNull Call<ApiService.ApiResponse<ApiService.GenerateShoppingListResponse>> call,
                                           @NonNull Response<ApiService.ApiResponse<ApiService.GenerateShoppingListResponse>> response) {
                        loadingLiveData.setValue(false);
                        if (response.isSuccessful() && response.body() != null && response.body().success) {
                            loadCurrentShoppingList();
                        } else if (response.body() != null) {
                            errorLiveData.setValue(response.body().message != null ? response.body().message : "Ошибка генерации");
                        } else {
                            errorLiveData.setValue("Ошибка соединения: " + response.code());
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<ApiService.ApiResponse<ApiService.GenerateShoppingListResponse>> call, @NonNull Throwable t) {
                        loadingLiveData.setValue(false);
                        errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                    }
                });
    }

    public void toggleItem(int itemId, boolean isChecked) {
        if (shoppingListLiveData.getValue() == null) {
            errorLiveData.setValue("Список покупок не загружен");
            return;
        }

        int listId = shoppingListLiveData.getValue().id;
        ApiService apiService = ApiClient.getApiService(getApplication());

        apiService.toggleShoppingListItem(listId, itemId, isChecked).enqueue(new Callback<ApiService.BaseResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.BaseResponse> call,
                                   @NonNull Response<ApiService.BaseResponse> response) {
                if (response.isSuccessful()) {
                    ApiService.ShoppingList currentList = shoppingListLiveData.getValue();
                    if (currentList != null && currentList.items != null) {
                        for (ApiService.ShoppingListItem item : currentList.items) {
                            if (item.id == itemId) {
                                item.isBought = isChecked;
                                break;
                            }
                        }
                        shoppingListLiveData.setValue(currentList);
                    }
                } else {
                    errorLiveData.setValue("Ошибка обновления");
                    loadCurrentShoppingList();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.BaseResponse> call, @NonNull Throwable t) {
                errorLiveData.setValue("Ошибка сети: " + t.getMessage());
                loadCurrentShoppingList();
            }
        });
    }

    public LiveData<ApiService.ShoppingList> getShoppingList() {
        return shoppingListLiveData;
    }

    public LiveData<Boolean> getLoading() {
        return loadingLiveData;
    }

    public LiveData<String> getError() {
        return errorLiveData;
    }
}
