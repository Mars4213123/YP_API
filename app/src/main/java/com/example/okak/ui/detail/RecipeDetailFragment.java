package com.example.okak.ui.detail;

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
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide; // Теперь этот импорт будет работать
import com.example.okak.R;
import com.example.okak.adapters.IngredientAdapter;
import com.example.okak.viewmodel.RecipeViewModel;

public class RecipeDetailFragment extends Fragment {
    private RecipeViewModel recipeViewModel;
    private int recipeId;
    private ImageView ivImage;
    private TextView tvTitle, tvDescription, tvInstructions;
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
        progressBar = root.findViewById(R.id.progressBarDetail); // Теперь этот ID существует

        recipeViewModel = new ViewModelProvider(this).get(RecipeViewModel.class);

        // --- ИСПРАВЛЕНИЕ: Убираем RecipeDetailFragmentArgs ---
        // Будем получать "recipeId" вручную из Bundle
        if (getArguments() != null) {
            recipeId = getArguments().getInt("recipeId");
        }
        // --- КОНЕЦ ИСПРАВЛЕНИЯ ---

        recipeViewModel.loadRecipeDetail(recipeId);
        setupObservers();

        rvIngredients.setLayoutManager(new LinearLayoutManager(getContext()));

        btnFavorite.setOnClickListener(v -> recipeViewModel.toggleFavorite(recipeId));

        return root;
    }

    private void setupObservers() {
        recipeViewModel.getRecipeDetail().observe(getViewLifecycleOwner(), detail -> {
            if (detail != null) {
                tvTitle.setText(detail.title);
                tvDescription.setText(detail.description);
                tvInstructions.setText(detail.instructions);

                // Glide для загрузки изображения
                Glide.with(this).load(detail.imageUrl).into(ivImage);

                // Адаптер для ингредиентов
                IngredientAdapter adapter = new IngredientAdapter(detail.ingredients);
                rvIngredients.setAdapter(adapter);
                btnFavorite.setText(detail.isFavorite ? "Убрать из избранного" : "В избранное");
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
}