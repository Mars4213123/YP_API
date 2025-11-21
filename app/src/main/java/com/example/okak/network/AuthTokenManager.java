package com.example.okak.network;

import android.content.Context;
import android.content.SharedPreferences;

public class AuthTokenManager {
    private static final String PREF_NAME = "YP_APP_PREFS"; // Единое имя файла
    private static final String KEY_AUTH_TOKEN = "authToken";
    private static final String KEY_USER_ID = "userId";
    private static final String KEY_USERNAME = "username";

    private static SharedPreferences getPrefs(Context context) {
        return context.getApplicationContext().getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
    }

    public static void saveUserId(Context context, int userId) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.putInt(KEY_USER_ID, userId);
        editor.apply();
    }

    public static int getUserId(Context context) {
        return getPrefs(context).getInt(KEY_USER_ID, -1);
    }

    public static void saveUsername(Context context, String username) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.putString(KEY_USERNAME, username);
        editor.apply();
    }

    public static String getUsername(Context context) {
        return getPrefs(context).getString(KEY_USERNAME, null);
    }

    public static void saveToken(Context context, String token) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.putString(KEY_AUTH_TOKEN, token);
        editor.apply();
    }

    public static String getToken(Context context) {
        return getPrefs(context).getString(KEY_AUTH_TOKEN, null);
    }

    public static void clearData(Context context) {
        SharedPreferences.Editor editor = getPrefs(context).edit();
        editor.clear(); // Удаляем всё сразу
        editor.apply();
    }

    public static boolean isLoggedIn(Context context) {
        return getUserId(context) != -1;
    }
}