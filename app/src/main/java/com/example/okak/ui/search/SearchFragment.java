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
    private String lastQuery = ""; // ДОБАВЛЕНО - отслеживание последнего запроса

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_search, container, false);

        etSearch = root.findViewById(R.id.etSearchQuery);
        rvResults = root.findViewById(R.id.rvSearchResults);
        progressBar = root.findViewById(R.id.progressBarSearch);
        tvEmptySearch = root.findViewById(R.id.tvEmptySearch);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class); // ИЗМЕНЕНО на requireActivity()

        setupRecyclerView();
        setupObservers();
        setupSearchListener();

        // Загружаем все рецепты только при первом запуске
        if (savedInstanceState == null) {
            loadRecipes("");
        }

        return root;
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new ArrayList<>());
        rvResults.setLayoutManager(new LinearLayoutManager(getContext()));
        rvResults.setAdapter(adapter);

        // ДОБАВЛЕНО - обработчик кликов
        adapter.setOnRecipeClickListener(recipe -> {
            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);
            Navigation.findNavController(requireView())
                    .navigate(R.id.action_to_recipeDetail, bundle);
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
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
                tvEmptySearch.setVisibility(View.VISIBLE);
                tvEmptySearch.setText("Ошибка загрузки");
            }
        });
    }

    private void setupSearchListener() {
        etSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void afterTextChanged(Editable s) {
                // Отменяем предыдущий поиск
                if (searchRunnable != null) {
                    searchHandler.removeCallbacks(searchRunnable);
                }

                // Запускаем новый поиск с задержкой 500ms
                searchRunnable = () -> {
                    String query = s.toString().trim();

                    // ДОБАВЛЕНО - проверка на дубликат запроса
                    if (!query.equals(lastQuery)) {
                        lastQuery = query;
                        loadRecipes(query);
                    }
                };

                searchHandler.postDelayed(searchRunnable, 500);
            }

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
            }
        });
    }

    private void loadRecipes(String query) {
        recipeViewModel.searchRecipes(
                query.isEmpty() ? null : query,  // name
                null,                            // tags
                null,                            // excludedAllergens
                null,                            // cuisineTypes
                null,                            // maxPrepTime
                null,                            // maxCookTime
                null,                            // maxCalories
                null,                            // difficulty
                "Title",                         // sortBy
                false,                           // sortDescending
                1,                               // pageNumber
                20                               // pageSize
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