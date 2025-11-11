package com.example.okak; // (твой package)

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import com.example.okak.network.AuthTokenManager;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private static final String TAG = "LoginActivity";

    private EditText etUsername;
    private EditText etPassword;
    private Button btnLogin;
    private TextView tvGoToRegister;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (AuthTokenManager.hasToken(getApplicationContext())) {
            goToMainActivity();
            return;
        }

        setContentView(R.layout.activity_login);

        apiService = ApiClient.getApiService(getApplicationContext());

        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);
        tvGoToRegister = findViewById(R.id.tvRegister);

        btnLogin.setOnClickListener(v -> attemptLogin());

        tvGoToRegister.setOnClickListener(v -> {
            startActivity(new Intent(LoginActivity.this, RegisterActivity.class));
        });
    }

    private void attemptLogin() {
        String username = etUsername.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (username.isEmpty() || password.isEmpty()) {
            Toast.makeText(this, "Пожалуйста, введите имя пользователя и пароль.", Toast.LENGTH_SHORT).show();
            return;
        }

        Call<ApiService.UserAuthResponse> call = apiService.login(username, password);

        call.enqueue(new Callback<ApiService.UserAuthResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.UserAuthResponse> call, @NonNull Response<ApiService.UserAuthResponse> response) {
                if (response.isSuccessful() && response.body() != null) {

                    // ==========================================================
                    //  *** ИСПРАВЛЕНИЕ ЗДЕСЬ ***
                    //  Было: response.body().getToken()
                    //  Стало: response.body().token (прямой доступ к полю)
                    String token = response.body().token;
                    // ==========================================================

                    AuthTokenManager.saveToken(getApplicationContext(), token);

                    Toast.makeText(LoginActivity.this, "Вход выполнен!", Toast.LENGTH_SHORT).show();

                    goToMainActivity();
                } else {
                    String errorMsg = "Ошибка входа. ";
                    try {
                        errorMsg += "Код: " + response.code() + ", Тело: " + response.errorBody().string();
                    } catch (Exception e) { e.printStackTrace(); }
                    Log.e(TAG, "Login failed: " + errorMsg);
                    Toast.makeText(LoginActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.UserAuthResponse> call, @NonNull Throwable t) {
                Log.e(TAG, "Network Error: " + t.getMessage(), t);
                Toast.makeText(LoginActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }

    private void goToMainActivity() {
        Intent intent = new Intent(LoginActivity.this, MainActivity.class);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(intent);
        finish();
    }
}