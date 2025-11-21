package com.example.okak.ui.favorites;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ProgressBar;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.Navigation;
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

        adapter = new RecipeAdapter(new ArrayList<>());
        rvFavorites.setLayoutManager(new LinearLayoutManager(getContext()));
        rvFavorites.setAdapter(adapter);

        // НАВИГАЦИЯ
        adapter.setOnRecipeClickListener(recipe -> {
            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);
            Navigation.findNavController(requireView())
                    .navigate(R.id.action_navigation_favorites_to_navigation_recipe_detail, bundle);
        });

        setupObservers();
        return root;
    }

    private void setupObservers() {
        recipeViewModel.getFavorites().observe(getViewLifecycleOwner(), favorites -> {
            if (favorites != null) {
                adapter.updateData(favorites);
                boolean isEmpty = favorites.isEmpty();
                rvFavorites.setVisibility(isEmpty ? View.GONE : View.VISIBLE);
                tvEmptyFavorites.setVisibility(isEmpty ? View.VISIBLE : View.GONE);
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        });
    }

    @Override
    public void onResume() {
        super.onResume();
        recipeViewModel.loadFavorites();
    }
}