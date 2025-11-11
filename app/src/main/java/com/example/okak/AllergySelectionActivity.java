package com.example.okak;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.LinearLayout;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.okak.network.ApiClient;
import com.example.okak.network.ApiService;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import com.example.okak.network.ApiService;

public class AllergySelectionActivity extends AppCompatActivity {
    private LinearLayout allergiesContainer;
    private Button saveButton;
    private final List<String> availableAllergies = Arrays.asList(
            "Глютен", "Молоко", "Орехи", "Арахис", "Яйца", "Рыба", "Соя", "Моллюски"
    );

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_allergy_selection);
        allergiesContainer = findViewById(R.id.allergies_container);
        saveButton = findViewById(R.id.btn_save_allergies);
        createAllergyCheckboxes();
        saveButton.setOnClickListener(v -> saveAllergies());
    }

    private void createAllergyCheckboxes() {
        for (String allergy : availableAllergies) {
            CheckBox checkBox = new CheckBox(this);
            checkBox.setText(allergy);
            checkBox.setTag(allergy);
            allergiesContainer.addView(checkBox);
        }
    }

    private void saveAllergies() {
        List<String> selectedAllergies = new ArrayList<>();
        for (int i = 0; i < allergiesContainer.getChildCount(); i++) {
            if (allergiesContainer.getChildAt(i) instanceof CheckBox) {
                CheckBox checkBox = (CheckBox) allergiesContainer.getChildAt(i);
                if (checkBox.isChecked()) {
                    selectedAllergies.add(checkBox.getTag().toString());
                }
            }
        }
        sendAllergiesToApi(selectedAllergies);
    }

    private void sendAllergiesToApi(List<String> allergies) {
        ApiService apiService = ApiClient.getApiService(this);

        ApiService.UpdateAllergiesDto allergiesDto = new ApiService.UpdateAllergiesDto(allergies);

        apiService.updateUserAllergies(allergiesDto).enqueue(new Callback<ApiService.BaseResponse>() {
            @Override
            public void onResponse(Call<ApiService.BaseResponse> call, Response<ApiService.BaseResponse> response) {
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    Toast.makeText(AllergySelectionActivity.this, "Аллергии сохранены!", Toast.LENGTH_SHORT).show();
                    navigateToNextScreen();
                } else {
                    Toast.makeText(AllergySelectionActivity.this, "Ошибка сохранения", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiService.BaseResponse> call, Throwable t) {
                Toast.makeText(AllergySelectionActivity.this, "Сетевая ошибка: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void navigateToNextScreen() {
        boolean fromRegistration = getIntent().getBooleanExtra("FROM_REGISTRATION", false);

        if (fromRegistration) {
            Intent intent = new Intent(AllergySelectionActivity.this, MainActivity.class);
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(intent);
        }
        finish();
    }
}