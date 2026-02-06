package com.example.androidupproject.network;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    // 10.0.2.2 - это localhost компьютера для эмулятора Android
    private static final String BASE_URL = "http://10.0.2.2:5286/";
    private static Retrofit retrofit = null;

    public static ApiService getService() {
        if (retrofit == null) {
            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
        }
        return retrofit.create(ApiService.class);
    }
}