package com.example.okak.ui.detail;

import android.content.SharedPreferences;
import android.content.res.ColorStateList;
import android.graphics.Color;
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
import com.example.okak.viewmodel.RecipeViewModel;

public class RecipeDetailFragment extends Fragment {

    private RecipeViewModel recipeViewModel;
    private int recipeId;
    private int userId = -1;

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
        progressBar = root.findViewById(R.id.progressBarDetail);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);

        // Получаем userId
        SharedPreferences prefs = requireContext().getSharedPreferences("user_prefs", requireContext().MODE_PRIVATE);
        userId = prefs.getInt("user_id", -1);

        // Устанавливаем userId в ViewModel
        if (userId != -1) {
            recipeViewModel.setUserId(userId);
        }

        if (getArguments() != null) {
            recipeId = getArguments().getInt("recipeId");
        }

        recipeViewModel.loadRecipeDetail(recipeId);
        setupObservers();

        rvIngredients.setLayoutManager(new LinearLayoutManager(getContext()));

        btnFavorite.setOnClickListener(v -> {
            if (userId == -1) {
                Toast.makeText(getContext(), "Необходимо авторизоваться", Toast.LENGTH_SHORT).show();
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
                tvDescription.setText(detail.description);
                tvInstructions.setText(detail.instructions);

                // ИСПРАВЛЕНО - загрузка изображения
                Glide.with(this)
                        .load(detail.imageUrl)
                        .placeholder(R.drawable.placeholder_recipe) // ИСПРАВЛЕНО
                        .error(R.drawable.placeholder_recipe)       // ИСПРАВЛЕНО
                        .into(ivImage);

                IngredientAdapter adapter = new IngredientAdapter(detail.ingredients);
                rvIngredients.setAdapter(adapter);

                // ИСПРАВЛЕНО - установка цвета кнопки
                btnFavorite.setText(detail.isFavorite ? "Удалить из избранного" : "В избранное");

                int color = detail.isFavorite
                        ? ContextCompat.getColor(requireContext(), R.color.red)
                        : ContextCompat.getColor(requireContext(), R.color.primary_green);

                btnFavorite.setBackgroundTintList(ColorStateList.valueOf(color)); // ИСПРАВЛЕНО
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