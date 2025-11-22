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
        userId = AuthTokenManager.getUserId(requireContext());
        if (userId != -1) {
            recipeViewModel.setUserId(userId);
        }

        if (getArguments() != null) {
            recipeId = getArguments().getInt("recipeId", -1);
        }

        if (recipeId != -1) {
            recipeViewModel.loadRecipeDetail(recipeId);
        }

        setupObservers();
        rvIngredients.setLayoutManager(new LinearLayoutManager(getContext()));

        btnFavorite.setOnClickListener(v -> {
            if (userId != -1) {
                recipeViewModel.toggleFavorite(recipeId);
            } else {
                Toast.makeText(getContext(), "Авторизуйтесь", Toast.LENGTH_SHORT).show();
            }
        });

        return root;
    }

    private void setupObservers() {
        recipeViewModel.getRecipeDetail().observe(getViewLifecycleOwner(), detail -> {
            if (detail != null && detail.id == recipeId) {
                tvTitle.setText(detail.title);
                tvDescription.setText(detail.description != null ? detail.description : "");
                tvInstructions.setText(detail.instructions != null ? detail.instructions : "");

                if (detail.imageUrl != null && !detail.imageUrl.isEmpty()) {
                    Glide.with(this).load(detail.imageUrl).placeholder(R.drawable.placeholder_recipe).into(ivImage);
                } else {
                    ivImage.setImageResource(R.drawable.placeholder_recipe);
                }

                if (detail.ingredients != null) {
                    rvIngredients.setAdapter(new IngredientAdapter(detail.ingredients));
                }

                btnFavorite.setText(detail.isFavorite ? "В избранном" : "В избранное");
                int color = detail.isFavorite ? R.color.red : R.color.primary_green;
                try {
                    btnFavorite.setBackgroundTintList(ColorStateList.valueOf(ContextCompat.getColor(requireContext(), color)));
                } catch (Exception e) {}
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        });
    }
}