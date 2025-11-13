package com.example.okak.ui.favorites;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.adapters.RecipeAdapter;
import com.example.okak.viewmodel.RecipeViewModel;
import java.util.ArrayList;

public class FavoritesFragment extends Fragment {

    private RecipeViewModel recipeViewModel;
    private RecyclerView rvFavorites;
    private ProgressBar progressBar;
    private TextView tvEmptyFavorites;
    private RecipeAdapter adapter;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_favorites, container, false);

        rvFavorites = root.findViewById(R.id.rvFavorites);
        progressBar = root.findViewById(R.id.progressBarFavorites);
        tvEmptyFavorites = root.findViewById(R.id.tvEmptyFavorites);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);

        setupRecyclerView();
        setupObservers();

        // Загружаем избранное
        recipeViewModel.loadFavorites();

        return root;
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new ArrayList<>());
        rvFavorites.setLayoutManager(new LinearLayoutManager(getContext()));
        rvFavorites.setAdapter(adapter);
    }

    private void setupObservers() {
        recipeViewModel.getFavorites().observe(getViewLifecycleOwner(), favorites -> {
            if (favorites != null) {
                adapter.updateData(favorites);

                if (favorites.isEmpty()) {
                    rvFavorites.setVisibility(View.GONE);
                    tvEmptyFavorites.setVisibility(View.VISIBLE);
                } else {
                    rvFavorites.setVisibility(View.VISIBLE);
                    tvEmptyFavorites.setVisibility(View.GONE);
                }
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);

            if (loading) {
                rvFavorites.setVisibility(View.GONE);
                tvEmptyFavorites.setVisibility(View.GONE);
            }
        });

        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null && !error.isEmpty()) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    @Override
    public void onResume() {
        super.onResume();
        // Перезагружаем избранное при возврате на фрагмент
        recipeViewModel.loadFavorites();
    }
}