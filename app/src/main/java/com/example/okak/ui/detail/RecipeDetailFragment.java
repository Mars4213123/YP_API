package com.example.okak.ui.detail;

import android.content.res.ColorStateList;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.bumptech.glide.Glide;
import com.example.okak.R;
import com.example.okak.adapters.IngredientAdapter;
import com.example.okak.network.AuthTokenManager;
import com.example.okak.viewmodel.RecipeViewModel;

public class RecipeDetailFragment extends Fragment {

    private RecipeViewModel recipeViewModel;
    private int recipeId = -1;
    private int userId = -1;

    private ImageView ivImage;
    private TextView tvTitle, tvDescription, tvInstructions, tvInfo;
    private RecyclerView rvIngredients;
    private Button btnFavorite;
    private ProgressBar progressBar;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_recipe_detail, container, false);

        ivImage = root.findViewById(R.id.ivRecipeImage);
        tvTitle = root.findViewById(R.id.tvRecipeTitle);
        tvDescription = root.findViewById(R.id.tvRecipeDescription);
        tvInstructions = root.findViewById(R.id.tvInstructions);
        rvIngredients = root.findViewById(R.id.rvIngredients);
        btnFavorite = root.findViewById(R.id.btnToggleFavorite);
        progressBar = root.findViewById(R.id.progressBarDetail);
        // Предположим, что у вас есть tvRecipeCalories, если нет - игнорируем
        // TextView tvCalories = root.findViewById(R.id.tvRecipeCalories);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);
        userId = AuthTokenManager.getUserId(requireContext());
        if (userId != -1) {
            recipeViewModel.setUserId(userId);
        }

        // Получаем ID рецепта из аргументов
        if (getArguments() != null) {
            recipeId = getArguments().getInt("recipeId", -1);
        }

        if (recipeId != -1) {
            recipeViewModel.loadRecipeDetail(recipeId);
        } else {
            Toast.makeText(getContext(), "Ошибка: ID рецепта не найден", Toast.LENGTH_SHORT).show();
        }

        setupObservers();

        rvIngredients.setLayoutManager(new LinearLayoutManager(getContext()));

        btnFavorite.setOnClickListener(v -> {
            if (userId == -1) {
                Toast.makeText(getContext(), "Сначала авторизуйтесь", Toast.LENGTH_SHORT).show();
                return;
            }
            recipeViewModel.toggleFavorite(recipeId);
        });

        return root;
    }

    private void setupObservers() {
        recipeViewModel.getRecipeDetail().observe(getViewLifecycleOwner(), detail -> {
            if (detail != null) {
                tvTitle.setText(detail.title);
                tvDescription.setText(detail.description != null ? detail.description : "Описание отсутствует");
                tvInstructions.setText(detail.instructions != null ? detail.instructions : "Инструкции отсутствуют");

                // Загрузка фото
                if (detail.imageUrl != null && !detail.imageUrl.isEmpty()) {
                    Glide.with(this)
                            .load(detail.imageUrl)
                            .placeholder(R.drawable.placeholder_recipe)
                            .error(R.drawable.placeholder_recipe)
                            .into(ivImage);
                } else {
                    ivImage.setImageResource(R.drawable.placeholder_recipe);
                }

                // Ингредиенты
                if (detail.ingredients != null) {
                    IngredientAdapter adapter = new IngredientAdapter(detail.ingredients);
                    rvIngredients.setAdapter(adapter);
                }

                // Кнопка избранного
                updateFavoriteButton(detail.isFavorite);
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        });

        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void updateFavoriteButton(boolean isFavorite) {
        btnFavorite.setText(isFavorite ? "Удалить из избранного" : "В избранное");
        int colorRes = isFavorite ? R.color.red : R.color.primary_green; // Убедитесь, что эти цвета есть в colors.xml
        // Fallback, если цветов нет, используем системные
        try {
            int color = ContextCompat.getColor(requireContext(), colorRes);
            btnFavorite.setBackgroundTintList(ColorStateList.valueOf(color));
        } catch (Exception e) {
            // Игнорируем, если цвет не найден
        }
    }
}