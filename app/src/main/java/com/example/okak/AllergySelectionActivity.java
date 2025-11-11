package com.example.okak;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.LinearLayout;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class AllergySelectionActivity extends AppCompatActivity {

    private LinearLayout allergiesContainer;
    private Button saveButton;
    private final List<String> availableAllergies = Arrays.asList(
            "Глютен", "Молоко", "Орехи", "Арахис", "Яйца", "Рыба", "Соя", "Моллюски"
            // Добавьте больше аллергенов по необходимости
    );

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_allergy_selection); // Создайте этот layout

        allergiesContainer = findViewById(R.id.allergies_container);
        saveButton = findViewById(R.id.btn_save_allergies);

        createAllergyCheckboxes();

        saveButton.setOnClickListener(v -> saveAllergies());
    }

    private void createAllergyCheckboxes() {
        for (String allergy : availableAllergies) {
            CheckBox checkBox = new CheckBox(this);
            checkBox.setText(allergy);
            checkBox.setTag(allergy); // Используем тег для получения названия при сохранении
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

        // TODO: Отправьте selectedAllergies на ваш C# API (см. п. 1)
        sendAllergiesToApi(selectedAllergies);
    }

    // Псевдокод для отправки на API
    private void sendAllergiesToApi(List<String> allergies) {
        // Здесь должен быть ваш код для вызова API (например, с использованием Retrofit)

        // В случае успеха:
        Toast.makeText(this, "Аллергии сохранены!", Toast.LENGTH_SHORT).show();

        // Переход на следующий экран (MainActivity)
        navigateToNextScreen();
    }

    private void navigateToNextScreen() {
        // Если Activity была вызвана после регистрации
        if (getIntent().getBooleanExtra("FROM_REGISTRATION", false)) {
            Intent intent = new Intent(AllergySelectionActivity.this, MainActivity.class);
            // Очищаем стек Activity, чтобы пользователь не мог вернуться к регистрации по кнопке "Назад"
            intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(intent);
        }
        finish();
    }
}