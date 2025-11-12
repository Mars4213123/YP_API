package com.example.okak.ui.home;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.adapters.MenuDayAdapter;
import com.example.okak.viewmodel.MenuViewModel;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class HomeFragment extends Fragment {
    private MenuViewModel menuViewModel;
    private RecyclerView rvMenuDays;
    private ProgressBar progressBar;
    private Button btnGenerateMenu;
    private TextView tvEmptyMenu;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_home, container, false);

        rvMenuDays = root.findViewById(R.id.rvMenuDays);
        progressBar = root.findViewById(R.id.progressBarHome);
        btnGenerateMenu = root.findViewById(R.id.btnGenerateMenu);
        tvEmptyMenu = root.findViewById(R.id.tvEmptyMenu);

        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);

        setupObservers();
        rvMenuDays.setLayoutManager(new LinearLayoutManager(getContext()));

        btnGenerateMenu.setOnClickListener(v -> {
            int days = 7;
            Double targetCalories = 2000.0; // ИСПОЛЬЗУЕМ КОНКРЕТНОЕ ЗНАЧЕНИЕ вместо null
            List<String> mealTypes = Arrays.asList("breakfast", "lunch", "dinner");
            List<String> cuisines = new ArrayList<>();

            menuViewModel.generateMenu(days, targetCalories, cuisines, mealTypes, false);
        });

        menuViewModel.loadCurrentMenu();

        return root;
    }

    private void setupObservers() {
        menuViewModel.getCurrentMenu().observe(getViewLifecycleOwner(), menu -> {
            if (menu != null && menu.days != null && !menu.days.isEmpty()) {
                MenuDayAdapter adapter = new MenuDayAdapter(menu.days);
                rvMenuDays.setAdapter(adapter);
                rvMenuDays.setVisibility(View.VISIBLE);
                tvEmptyMenu.setVisibility(View.GONE);
            } else {
                MenuDayAdapter adapter = new MenuDayAdapter(new ArrayList<>());
                rvMenuDays.setAdapter(adapter);
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.VISIBLE);
            }
        });

        menuViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            btnGenerateMenu.setEnabled(!loading);
            if (loading) {
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.GONE);
            }
        });

        menuViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
                tvEmptyMenu.setVisibility(View.VISIBLE);
                rvMenuDays.setVisibility(View.GONE);
            }
        });
    }
}
