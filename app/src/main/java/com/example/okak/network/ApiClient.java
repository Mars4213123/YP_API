package com.example.okak.network; // (замени на свой package)

import android.content.Context;

import java.io.IOException;


import okhttp3.Interceptor;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {

    // *** ИСПРАВЛЕНИЕ #1: Используем HTTP и порт .NET ***
    // 10.0.2.2 - это "localhost" для эмулятора Android
    public static final String BASE_URL = "http://10.0.2.2:5286/";

    private static Retrofit retrofit = null;
    private static ApiService apiService = null;

    // Получаем экземпляр ApiService
    public static ApiService getApiService(Context context) {
        if (apiService == null) {
            apiService = getRetrofitInstance(context).create(ApiService.class);
        }
        return apiService;
    }

    // Создаем экземпляр Retrofit с Interceptor'ом для токенов
    public static Retrofit getRetrofitInstance(Context context) {
        if (retrofit == null) {

            // *** ИСПРАВЛЕНИЕ #2: Создаем OkHttpClient с Interceptor'ом ***
            OkHttpClient.Builder httpClient = new OkHttpClient.Builder();

            httpClient.addInterceptor(new Interceptor() {
                @Override
                public Response intercept(Chain chain) throws IOException {
                    // Получаем токен из SharedPreferences
                    String token = AuthTokenManager.getToken(context.getApplicationContext());

                    Request original = chain.request();
                    Request.Builder requestBuilder = original.newBuilder();

                    // Если токен есть, добавляем заголовок Authorization
                    if (token != null && !token.isEmpty()) {
                        requestBuilder.header("Authorization", "Bearer " + token);
                    }

                    Request request = requestBuilder.build();
                    return chain.proceed(request);
                }
            });

            OkHttpClient client = httpClient.build();

            // Собираем Retrofit с нашим кастомным OkHttpClient
            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .client(client) // <-- Применяем клиент
                    .build();
        }
        return retrofit;
    }
}