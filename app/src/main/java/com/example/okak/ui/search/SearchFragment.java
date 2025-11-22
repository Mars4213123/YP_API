package com.example.okak.ui.search;

import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.NavController;
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
    private Spinner spinnerDifficulty;
    private RecyclerView rvResults;
    private ProgressBar progressBar;
    private TextView tvEmptySearch;
    private RecipeAdapter adapter;

    private final Handler searchHandler = new Handler(Looper.getMainLooper());
    private Runnable searchRunnable;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_search, container, false);

        etSearch = root.findViewById(R.id.etSearchQuery);
        spinnerDifficulty = root.findViewById(R.id.spinnerDifficulty);
        rvResults = root.findViewById(R.id.rvSearchResults);
        progressBar = root.findViewById(R.id.progressBarSearch);
        tvEmptySearch = root.findViewById(R.id.tvEmptySearch);

        recipeViewModel = new ViewModelProvider(requireActivity()).get(RecipeViewModel.class);

        setupRecyclerView();
        setupDifficultySpinner();
        setupObservers();
        setupSearchListener();

        if (savedInstanceState == null) {
            recipeViewModel.loadAllRecipes();
        }

        return root;
    }

    private void setupDifficultySpinner() {
        if (spinnerDifficulty == null) return;

        // ИСПРАВЛЕНО: правильные значения для спиннера
        String[] displayItems = {"Все", "Легко", "Средне", "Сложно"};
        String[] valueItems = {"", "easy", "medium", "hard"};

        ArrayAdapter<String> spinnerAdapter = new ArrayAdapter<>(
                getContext(),
                android.R.layout.simple_spinner_item,
                displayItems
        );
        spinnerAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerDifficulty.setAdapter(spinnerAdapter);

        spinnerDifficulty.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                String selectedValue = valueItems[position];
                recipeViewModel.setDifficultyFilter(selectedValue);
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {}
        });
    }

    private void setupRecyclerView() {
        adapter = new RecipeAdapter(new ArrayList<>());
        rvResults.setLayoutManager(new LinearLayoutManager(getContext()));
        rvResults.setAdapter(adapter);

        adapter.setOnRecipeClickListener(recipe -> {
            if (recipe == null) return;
            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);
            NavController navController = Navigation.findNavController(requireView());
            try {
                // ИСПРАВЛЕНО: правильный ID навигации
                navController.navigate(R.id.navigationrecipedetail, bundle);
            } catch (IllegalArgumentException e) {
                Toast.makeText(getContext(), "Ошибка навигации", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void setupObservers() {
        recipeViewModel.getRecipes().observe(getViewLifecycleOwner(), recipes -> {
            if (recipes != null) {
                adapter.updateData(recipes);
                boolean isEmpty = recipes.isEmpty();
                rvResults.setVisibility(isEmpty ? View.GONE : View.VISIBLE);
                tvEmptySearch.setVisibility(isEmpty ? View.VISIBLE : View.GONE);
                if (isEmpty) {
                    tvEmptySearch.setText("Рецепты не найдены");
                }
            } else {
                adapter.updateData(new ArrayList<>());
                rvResults.setVisibility(View.GONE);
                tvEmptySearch.setVisibility(View.VISIBLE);
            }
        });

        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            // ИСПРАВЛЕНО: ProgressBar скрывает список при загрузке
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            rvResults.setVisibility(loading ? View.GONE : View.VISIBLE);
            tvEmptySearch.setVisibility(loading ? View.GONE : tvEmptySearch.getVisibility());
        });

        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null && !error.isEmpty()) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
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
                    recipeViewModel.searchByName(query);
                };

                searchHandler.postDelayed(searchRunnable, 600);
            }

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {}
        });
    }
}
