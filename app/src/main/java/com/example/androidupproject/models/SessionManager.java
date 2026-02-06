package com.example.androidupproject.models;
import android.content.Context;
import android.content.SharedPreferences;

public class SessionManager {
    private SharedPreferences prefs;

    public SessionManager(Context context) {
        prefs = context.getSharedPreferences("AppPrefs", Context.MODE_PRIVATE);
    }

    public void saveUser(int id, String token) {
        prefs.edit().putInt("USER_ID", id).putString("TOKEN", token).apply();
    }

    public int getUserId() {
        return prefs.getInt("USER_ID", 0);
    }

    public String getToken() {
        return prefs.getString("TOKEN", null);
    }
}