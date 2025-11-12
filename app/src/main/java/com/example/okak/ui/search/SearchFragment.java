package com.example.okak.ui.search;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
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

public class SearchFragment extends Fragment {
    private RecipeViewModel recipeViewModel;
    private EditText etSearch;
    private RecyclerView rvResults;
    private ProgressBar progressBar;
    private TextView tvEmptySearch;
    private RecipeAdapter adapter;

    private Handler searchHandler = new Handler(Looper.getMainLooper());
    private Runnable searchRunnable;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_search, container, false);

        etSearch = root.findViewById(R.id.etSearchQuery);
        rvResults = root.findViewById(R.id.rvSearchResults);
        progressBar = root.findViewById(R.id.progressBarSearch);
        tvEmptySearch = root.findViewById(R.id.tvEmptySearch);

        recipeViewModel = new ViewModelProvider(this).get(RecipeViewModel.class);

        setupRecyclerView();
        setupObservers();

        etSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void afterTextChanged(Editable s) {
                if (searchRunnable != null) {
                    searchHandler.removeCallbacks(searchRunnable);
                }

                searchRunnable = () -> recipeViewModel.searchRecipes(s.toString(), null, null, null, 1, 20);
                searchHandler.postDelayed(searchRunnable, 500);
            }

            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            public void onTextChanged(CharSequence s, int start, int before, int count) {}
        });

        // Загружаем все рецепты при открытии
        recipeViewModel.searchRecipes("", null, null, null, 1, 20);

        return root;
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new ArrayList<>());
        rvResults.setLayoutManager(new LinearLayoutManager(getContext()));
        rvResults.setAdapter(adapter);
    }

    private void setupObservers() {
        recipeViewModel.getRecipes().observe(getViewLifecycleOwner(), recipes -> {
            if (recipes != null) {
                adapter = new RecipeAdapter(recipes);
                rvResults.setAdapter(adapter);
                if (recipes.isEmpty()) {
                    rvResults.setVisibility(View.GONE);
                    tvEmptySearch.setVisibility(View.VISIBLE);
                } else {
                    rvResults.setVisibility(View.VISIBLE);
                    tvEmptySearch.setVisibility(View.GONE);
                }
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            if (loading) {
                rvResults.setVisibility(View.GONE);
                tvEmptySearch.setVisibility(View.GONE);
            }
        });

        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }
}
