package com.example.okak.ui.favorites;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.adapters.RecipeAdapter;
import com.example.okak.viewmodel.RecipeViewModel;

public class FavoritesFragment extends Fragment {
    private RecipeViewModel recipeViewModel;
    private RecyclerView rvFavorites;
    private ProgressBar progressBar;
    private RecipeAdapter adapter;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_favorites, container, false);
        rvFavorites = root.findViewById(R.id.rvFavorites);
        progressBar = root.findViewById(R.id.progressBarFavorites);
        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);
        setupRecyclerView();
        setupObservers();
        return root;
    }

    @Override
    public void onResume() {
        super.onResume();
        recipeViewModel.loadFavorites();
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new java.util.ArrayList<>());
        rvFavorites.setLayoutManager(new LinearLayoutManager(getContext()));
        rvFavorites.setAdapter(adapter);
    }

    private void setupObservers() {
        recipeViewModel.getRecipes().observe(getViewLifecycleOwner(), recipes -> {
            if (recipes != null) {
                adapter = new RecipeAdapter(recipes);
                rvFavorites.setAdapter(adapter);
            }
        });
        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            rvFavorites.setVisibility(loading ? View.GONE : View.VISIBLE);
        });
        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }
}