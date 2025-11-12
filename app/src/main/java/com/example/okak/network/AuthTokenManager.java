package com.example.okak.network;

import android.content.Context;
import android.content.SharedPreferences;

public class AuthTokenManager {
    private static final String PREF_NAME = "AuthPrefs";
    private static final String KEY_AUTH_TOKEN = "authToken";
    private static final String KEY_USER_ID = "userId"; // <-- ДОБАВЛЕНО

    private static SharedPreferences getPrefs(Context context) {
        return context.getApplicationContext().getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
    }

    // --- Методы для UserId ---
    public static void saveUserId(Context context, int userId) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.putInt(KEY_USER_ID, userId);
        editor.apply();
    }

    public static int getUserId(Context context) {
        // Возвращаем -1, если ID не найден
        return getPrefs(context).getInt(KEY_USER_ID, -1);
    }

    public static boolean hasUserId(Context context) {
        return getPrefs(context).contains(KEY_USER_ID);
    }

    public static void clearUserId(Context context) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.remove(KEY_USER_ID);
        editor.apply();
    }


    // --- Методы для Токена (остаются на случай, если вы почините API) ---
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
        // Также очищаем userId при выходе
        clearUserId(context);
    }

    public static boolean hasToken(Context context) {
        return getToken(context) != null;
    }
}