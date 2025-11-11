package com.example.okak; // (твой package)

import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
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

import java.util.ArrayList; // <-- Добавить импорт

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RegisterActivity extends AppCompatActivity {

    private static final String TAG = "RegisterActivity";
    private EditText etUsername, etFullName, etEmail, etPassword;
    private Button btnRegister;
    private TextView tvGoToLogin;
    private ApiService apiService;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        apiService = ApiClient.getApiService(getApplicationContext());

        etUsername = findViewById(R.id.etUsername);
        etFullName = findViewById(R.id.etFullName);
        etEmail = findViewById(R.id.etEmail);
        etPassword = findViewById(R.id.etPassword);
        btnRegister = findViewById(R.id.btnRegister);
        tvGoToLogin = findViewById(R.id.tvGoToLogin);

        btnRegister.setOnClickListener(v -> registerUser());

        tvGoToLogin.setOnClickListener(v -> {
            finish();
        });
    }

    private void registerUser() {
        String username = etUsername.getText().toString().trim();
        String fullName = etFullName.getText().toString().trim();
        String email = etEmail.getText().toString().trim();
        String password = etPassword.getText().toString().trim();

        if (TextUtils.isEmpty(username) || TextUtils.isEmpty(fullName) || TextUtils.isEmpty(email) || TextUtils.isEmpty(password)) {
            Toast.makeText(this, "Все поля обязательны", Toast.LENGTH_SHORT).show();
            return;
        }

        // ==========================================================
        //  *** ИСПРАВЛЕНИЕ ЗДЕСЬ (Ошибка 1) ***
        //  Мы должны передать 5-й аргумент (allergies),
        //  так как в твоей форме его нет, передаем пустой список.
        Call<ApiService.UserAuthResponse> call = apiService.register(
                username,
                email,
                password,
                fullName,
                new ArrayList<>() // <-- Добавлен пустой список для allergies
        );
        // ==========================================================

        call.enqueue(new Callback<ApiService.UserAuthResponse>() {
            @Override
            public void onResponse(@NonNull Call<ApiService.UserAuthResponse> call, @NonNull Response<ApiService.UserAuthResponse> response) {
                if (response.isSuccessful() && response.body() != null) {

                    // ==========================================================
                    //  *** ИСПРАВЛЕНИЕ ЗДЕСЬ (Ошибка 2) ***
                    //  Было: response.body().getToken()
                    //  Стало: response.body().token (прямой доступ к полю)
                    String token = response.body().token;
                    // ==========================================================

                    AuthTokenManager.saveToken(getApplicationContext(), token);

                    Toast.makeText(RegisterActivity.this, "Регистрация успешна!", Toast.LENGTH_LONG).show();

                    Intent intent = new Intent(RegisterActivity.this, AllergySelectionActivity.class);
                    intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                    startActivity(intent);
                    finish();
                } else {
                    String errorMsg = "Ошибка регистрации. ";
                    try {
                        errorMsg += "Код: " + response.code() + ", Тело: " + response.errorBody().string();
                    } catch (Exception e) { e.printStackTrace(); }
                    Log.e(TAG, "Register failed: " + errorMsg);
                    Toast.makeText(RegisterActivity.this, errorMsg, Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<ApiService.UserAuthResponse> call, @NonNull Throwable t) {
                Log.e(TAG, "Network Error: " + t.getMessage(), t);
                Toast.makeText(RegisterActivity.this, "Ошибка сети: " + t.getMessage(), Toast.LENGTH_LONG).show();
            }
        });
    }
}