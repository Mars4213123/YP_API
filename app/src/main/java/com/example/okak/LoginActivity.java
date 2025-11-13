package com.example.okak;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
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
        progressBar.setVisibility(android.view.View.VISIBLE);
        btnLogin.setEnabled(false);

        ApiService apiService = ApiClient.getApiService(this);

        apiService.login(username, password)
                .enqueue(new Callback<ApiService.UserAuthResponse>() {
                    @Override
                    public void onResponse(Call<ApiService.UserAuthResponse> call,
                                           Response<ApiService.UserAuthResponse> response) {
                        progressBar.setVisibility(android.view.View.GONE);
                        btnLogin.setEnabled(true);

                        if (response.isSuccessful() && response.body() != null) {
                            ApiService.UserAuthResponse result = response.body();

                            if (result.id > 0) {
                                // Сохраняем userId
                                SharedPreferences prefs = getSharedPreferences("user_prefs", MODE_PRIVATE);
                                prefs.edit().putInt("user_id", result.id).apply();
                                prefs.edit().putString("username", result.username).apply();

                                Toast.makeText(LoginActivity.this,
                                        "Добро пожаловать, " + result.username,
                                        Toast.LENGTH_SHORT).show();

                                Intent intent = new Intent(LoginActivity.this, MainActivity.class);
                                intent.putExtra("USER_ID", result.id);
                                startActivity(intent);
                                finish();
                            } else {
                                Toast.makeText(LoginActivity.this,
                                        "Ошибка авторизации",
                                        Toast.LENGTH_SHORT).show();
                            }
                        } else {
                            Toast.makeText(LoginActivity.this,
                                    "Неверный логин или пароль",
                                    Toast.LENGTH_SHORT).show();
                        }
                    }

                    @Override
                    public void onFailure(Call<ApiService.UserAuthResponse> call, Throwable t) {
                        progressBar.setVisibility(android.view.View.GONE);
                        btnLogin.setEnabled(true);

                        Toast.makeText(LoginActivity.this,
                                "Ошибка сети: " + t.getMessage(),
                                Toast.LENGTH_SHORT).show();
                    }
                });
    }
}