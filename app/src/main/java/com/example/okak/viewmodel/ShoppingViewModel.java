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

public class ShoppingViewModel extends AndroidViewModel {
    private MutableLiveData<ApiService.ShoppingList> shoppingListLiveData = new MutableLiveData<>();
    private MutableLiveData<Boolean> loadingLiveData = new MutableLiveData<>(false);
    private MutableLiveData<String> errorLiveData = new MutableLiveData<>();

    public ShoppingViewModel(@NonNull Application application) {
        super(application);
    }

    public void loadCurrentShoppingList() {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.getCurrentShoppingList().enqueue(new Callback<ApiService.ShoppingList>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ShoppingList> call, @NonNull Response<ApiService.ShoppingList> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    shoppingListLiveData.setValue(response.body());
                } else {
                    shoppingListLiveData.setValue(null);
                    errorLiveData.setValue("Ошибка загрузки списка");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ShoppingList> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка");
            }
        });
    }

    public void generateShoppingList(int menuId) {
        loadingLiveData.setValue(true);
        ApiService apiService = ApiClient.getApiService(getApplication());
        apiService.generateShoppingList(menuId).enqueue(new Callback<ApiService.ShoppingList>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.ShoppingList> call, @NonNull Response<ApiService.ShoppingList> response) {
                loadingLiveData.setValue(false);
                if (response.isSuccessful() && response.body() != null) {
                    shoppingListLiveData.setValue(response.body());
                } else {
                    errorLiveData.setValue("Ошибка генерации");
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.ShoppingList> call, @NonNull Throwable t) {
                loadingLiveData.setValue(false);
                errorLiveData.setValue("Сетевая ошибка");
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
            public void onResponse(@NonNull Call<ApiService.BaseResponse> call, @NonNull Response<ApiService.BaseResponse> response) {
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
                errorLiveData.setValue("Сетевая ошибка");
                loadCurrentShoppingList();
            }
        });
    }

    public LiveData<ApiService.ShoppingList> getShoppingList() { return shoppingListLiveData; }
    public LiveData<Boolean> getLoading() { return loadingLiveData; }
    public LiveData<String> getError() { return errorLiveData; }
}