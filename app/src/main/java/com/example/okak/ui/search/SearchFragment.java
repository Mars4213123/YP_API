package com.example.okak.ui.search;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
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

public class SearchFragment extends Fragment {
    private RecipeViewModel recipeViewModel;
    private EditText etSearch;
    private RecyclerView rvResults;
    private ProgressBar progressBar;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_search, container, false);
        etSearch = root.findViewById(R.id.etSearchQuery);
        rvResults = root.findViewById(R.id.rvSearchResults);
        progressBar = root.findViewById(R.id.progressBarSearch);
        recipeViewModel = new ViewModelProvider(this).get(RecipeViewModel.class);
        setupObservers();
        rvResults.setLayoutManager(new LinearLayoutManager(getContext()));
        etSearch.addTextChangedListener(new TextWatcher() {
            @Override
            public void afterTextChanged(Editable s) {
                recipeViewModel.searchRecipes(s.toString(), null, null, null, 1, 20);
            }
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}
            public void onTextChanged(CharSequence s, int start, int before, int count) {}
        });
        return root;
    }

    private void setupObservers() {
        recipeViewModel.getRecipes().observe(getViewLifecycleOwner(), recipes -> {
            if (recipes != null) {
                RecipeAdapter adapter = new RecipeAdapter(recipes);
                rvResults.setAdapter(adapter);
            }
        });
        recipeViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            rvResults.setVisibility(loading ? View.GONE : View.VISIBLE);
        });
        recipeViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }
}