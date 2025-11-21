package com.example.okak;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
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

    private EditText etUsername, etPassword;
    private Button btnLogin;
    private TextView tvRegister;
    private ProgressBar progressBar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        // Если уже залогинены - пропускаем этот экран
        if (AuthTokenManager.isLoggedIn(this)) {
            navigateToMain();
            return;
        }

        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);
        btnLogin = findViewById(R.id.btnLogin);
        tvRegister = findViewById(R.id.tvRegister);
        progressBar = findViewById(R.id.progressBarLogin);

        btnLogin.setOnClickListener(v -> {
            String username = etUsername.getText().toString().trim();
            String password = etPassword.getText().toString().trim();

            if (username.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Заполните все поля", Toast.LENGTH_SHORT).show();
                return;
            }
            login(username, password);
        });

        tvRegister.setOnClickListener(v -> {
            Intent intent = new Intent(LoginActivity.this, RegisterActivity.class);
            startActivity(intent);
        });
    }

    private void login(String username, String password) {
        progressBar.setVisibility(View.VISIBLE);
        btnLogin.setEnabled(false);

        ApiService apiService = ApiClient.getApiService(this);

        apiService.login(username, password).enqueue(new Callback<ApiService.UserAuthResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.UserAuthResponse> call,
                                   @NonNull Response<ApiService.UserAuthResponse> response) {
                progressBar.setVisibility(View.GONE);
                btnLogin.setEnabled(true);

                if (response.isSuccessful() && response.body() != null) {
                    ApiService.UserAuthResponse result = response.body();

                    if (result.id > 0) {
                        // ИСПОЛЬЗУЕМ AuthTokenManager
                        AuthTokenManager.saveUserId(LoginActivity.this, result.id);
                        AuthTokenManager.saveUsername(LoginActivity.this, result.username);
                        // Если бэк присылает токен, сохраняем его здесь. Если нет, пропускаем.
                        // AuthTokenManager.saveToken(LoginActivity.this, result.token);

                        Toast.makeText(LoginActivity.this, "Добро пожаловать, " + result.username, Toast.LENGTH_SHORT).show();
                        navigateToMain();
                    } else {
                        Toast.makeText(LoginActivity.this, "Ошибка: Некорректный ID пользователя", Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(LoginActivity.this, "Неверный логин или пароль", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.UserAuthResponse> call, @NonNull Throwable t) {
                progressBar.setVisibility(View.GONE);
                btnLogin.setEnabled(true);
                Toast.makeText(LoginActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void navigateToMain() {
        Intent intent = new Intent(LoginActivity.this, MainActivity.class);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(intent);
        finish();
    }
}