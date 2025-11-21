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
import androidx.navigation.Navigation;
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
    private String lastQuery = "";

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_search, container, false);

        etSearch = root.findViewById(R.id.etSearchQuery);
        rvResults = root.findViewById(R.id.rvSearchResults);
        progressBar = root.findViewById(R.id.progressBarSearch);
        tvEmptySearch = root.findViewById(R.id.tvEmptySearch);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);

        setupRecyclerView();
        setupObservers();
        setupSearchListener();

        // Грузим все рецепты при первом запуске
        if (savedInstanceState == null) {
            loadRecipes("");
        }

        return root;
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new ArrayList<>());
        rvResults.setLayoutManager(new LinearLayoutManager(getContext()));
        rvResults.setAdapter(adapter);

        // Навигация при клике
        adapter.setOnRecipeClickListener(recipe -> {
            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);
            // Переход по ID фрагмента
            Navigation.findNavController(requireView())
                    .navigate(R.id.recipeDetailFragment, bundle);
        });
    }

    private void setupObservers() {
        recipeViewModel.getRecipes().observe(getViewLifecycleOwner(), recipes -> {
            if (recipes != null) {
                adapter.updateData(recipes);

                if (recipes.isEmpty()) {
                    rvResults.setVisibility(View.GONE);
                    tvEmptySearch.setVisibility(View.VISIBLE);
                    tvEmptySearch.setText("Рецепты не найдены");
                } else {
                    rvResults.setVisibility(View.VISIBLE);
                    tvEmptySearch.setVisibility(View.GONE);
                }
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        });

        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null && !error.isEmpty()) {
                // Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
                // Не показываем тост, если это просто "ничего не найдено"
                if (adapter.getItemCount() == 0) {
                    tvEmptySearch.setVisibility(View.VISIBLE);
                    tvEmptySearch.setText(error);
                }
            }
        });
    }

    private void setupSearchListener() {
        etSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void afterTextChanged(Editable s) {
                if (searchRunnable != null) {
                    searchHandler.removeCallbacks(searchRunnable);
                }

                searchRunnable = () -> {
                    String query = s.toString().trim();
                    if (!query.equals(lastQuery)) {
                        lastQuery = query;
                        loadRecipes(query);
                    }
                };
                // Задержка 600мс перед поиском
                searchHandler.postDelayed(searchRunnable, 600);
            }

            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            @Override public void onTextChanged(CharSequence s, int start, int before, int count) {}
        });
    }

    private void loadRecipes(String query) {
        // Передаем пустые строки вместо null там, где сервер может ругаться
        recipeViewModel.searchRecipes(
                query,
                null,
                null,
                null,
                null,
                null,
                null,
                "easy",
                "Title",
                false,
                1,
                20
        );
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        if (searchHandler != null && searchRunnable != null) {
            searchHandler.removeCallbacks(searchRunnable);
        }
    }
}