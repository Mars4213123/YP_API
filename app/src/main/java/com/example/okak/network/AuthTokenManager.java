package com.example.okak.network;

import android.content.Context;
import android.content.SharedPreferences;

// Класс для управления токеном авторизации
public class AuthTokenManager {

    private static final String PREF_NAME = "AuthPrefs";
    private static final String KEY_AUTH_TOKEN = "authToken";

    private static SharedPreferences getPrefs(Context context) {
        return context.getApplicationContext().getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
    }

    public static void saveToken(Context context, String token) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.putString(KEY_AUTH_TOKEN, token);
        editor.apply();
    }

    public static String getToken(Context context) {
        return getPrefs(context).getString(KEY_AUTH_TOKEN, null);
    }

    public static void clearToken(Context context) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.remove(KEY_AUTH_TOKEN);
        editor.apply();
    }

    public static boolean hasToken(Context context) {
        return getToken(context) != null;
    }
}