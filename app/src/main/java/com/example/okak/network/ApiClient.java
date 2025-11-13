package com.example.okak.network;

import android.content.Context;
import java.io.IOException;
import okhttp3.Interceptor;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    public static final String BASE_URL = "http://10.0.2.2:5286";

    private static Retrofit retrofit = null;
    private static ApiService apiService = null;

    public static ApiService getApiService(Context context) {
        if (apiService == null) {
            apiService = getRetrofitInstance(context).create(ApiService.class);
        }
        return apiService;
    }

    public static Retrofit getRetrofitInstance(Context context) {
        if (retrofit == null) {
            // Создаем интерцептор для логирования
            HttpLoggingInterceptor loggingInterceptor = new HttpLoggingInterceptor();
            loggingInterceptor.setLevel(HttpLoggingInterceptor.Level.BODY); // Уровень логирования

            OkHttpClient.Builder httpClient = new OkHttpClient.Builder();

            // Добавляем интерцептор авторизации
            httpClient.addInterceptor(new Interceptor() {
                @Override
                public Response intercept(Chain chain) throws IOException {
                    String token = AuthTokenManager.getToken(context.getApplicationContext());
                    Request original = chain.request();
                    Request.Builder requestBuilder = original.newBuilder();
                    if (token != null && !token.isEmpty()) {
                        requestBuilder.header("Authorization", "Bearer " + token);
                    }
                    Request request = requestBuilder.build();
                    return chain.proceed(request);
                }
            });

            // Добавляем интерцептор логирования
            httpClient.addInterceptor(loggingInterceptor);

            OkHttpClient client = httpClient.build();

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .client(client)
                    .build();
        }
        return retrofit;
    }
}