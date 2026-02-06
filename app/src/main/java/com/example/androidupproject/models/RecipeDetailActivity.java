package com.example.androidupproject;

import android.os.Bundle;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.bumptech.glide.Glide; // Библиотека для картинок
import com.example.androidupproject.models.IngredientDto;
import com.example.androidupproject.models.RecipeDto;
import com.example.androidupproject.network.ApiClient;
import com.example.androidupproject.network.ApiResponse;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RecipeDetailActivity extends AppCompatActivity {

    private int recipeId;
    private TextView tvTitle, tvDesc, tvIngredients, tvInstructions;
    private ImageView ivImage;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_recipe_detail);

        recipeId = getIntent().getIntExtra("RECIPE_ID", 0);

        tvTitle = findViewById(R.id.tvRecipeTitle);
        tvDesc = findViewById(R.id.tvDescription);
        tvIngredients = findViewById(R.id.tvIngredients);
        tvInstructions = findViewById(R.id.tvInstructions);
        ivImage = findViewById(R.id.ivRecipeImage);

        loadRecipe();
    }

    private void loadRecipe() {
        ApiClient.getService().getRecipe(recipeId).enqueue(new Callback<ApiResponse<RecipeDto>>() {
            @Override
            public void onResponse(Call<ApiResponse<RecipeDto>> call, Response<ApiResponse<RecipeDto>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().data != null) {
                    displayRecipe(response.body().data);
                } else {
                    Toast.makeText(RecipeDetailActivity.this, "Ошибка загрузки", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<RecipeDto>> call, Throwable t) {
                Toast.makeText(RecipeDetailActivity.this, "Ошибка сети", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void displayRecipe(RecipeDto recipe) {
        tvTitle.setText(recipe.title);
        tvDesc.setText(recipe.description);
        tvInstructions.setText(recipe.instructions);

        // Формируем список ингредиентов
        StringBuilder sb = new StringBuilder();
        if (recipe.ingredients != null) {
            for (IngredientDto ing : recipe.ingredients) {
                sb.append("• ").append(ing.name).append(" - ").append(ing.unit).append("\n");
            }
        }
        tvIngredients.setText(sb.toString());

        // Загрузка картинки
        if (recipe.imageUrl != null && !recipe.imageUrl.isEmpty()) {
            Glide.with(this).load(recipe.imageUrl).into(ivImage);
        }
    }
}