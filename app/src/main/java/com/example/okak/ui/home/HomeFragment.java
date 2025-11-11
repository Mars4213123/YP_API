package com.example.okak.ui.home;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView; // <-- Добавлен импорт
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.okak.R;
import com.example.okak.adapters.MenuDayAdapter;
import com.example.okak.network.ApiService;
import com.example.okak.viewmodel.MenuViewModel;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public class HomeFragment extends Fragment {
    private MenuViewModel menuViewModel;
    private RecyclerView rvMenuDays;
    private ProgressBar progressBar;
    private Button btnGenerateMenu;
    private TextView tvEmptyMenu; // <-- Добавлено

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_home, container, false);
        rvMenuDays = root.findViewById(R.id.rvMenuDays);
        progressBar = root.findViewById(R.id.progressBarHome);
        btnGenerateMenu = root.findViewById(R.id.btnGenerateMenu);
        tvEmptyMenu = root.findViewById(R.id.tvEmptyMenu); // <-- Добавлено

        // Используем requireActivity() чтобы ViewModel был общим для всех фрагментов
        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);
        setupObservers();

        rvMenuDays.setLayoutManager(new LinearLayoutManager(getContext()));
        btnGenerateMenu.setOnClickListener(v -> {
            List<String> cuisines = Arrays.asList("Italian", "Russian"); // Пример, добавьте UI для выбора
            List<String> mealTypes = Arrays.asList("breakfast", "lunch", "dinner");
            menuViewModel.generateMenu(7, 2000.0, cuisines, mealTypes, false);
        });
        menuViewModel.loadCurrentMenu(); // Загрузка при старте
        return root;
    }

    private void setupObservers() {
        menuViewModel.getCurrentMenu().observe(getViewLifecycleOwner(), menu -> {
            // --- ИСПРАВЛЕНИЕ: Обработка пустого меню ---
            if (menu != null && menu.days != null && !menu.days.isEmpty()) {
                MenuDayAdapter adapter = new MenuDayAdapter(menu.days);
                rvMenuDays.setAdapter(adapter);
                rvMenuDays.setVisibility(View.VISIBLE);
                tvEmptyMenu.setVisibility(View.GONE);
            } else {
                // Показываем "пустое" состояние
                MenuDayAdapter adapter = new MenuDayAdapter(new ArrayList<>());
                rvMenuDays.setAdapter(adapter);
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.VISIBLE);
            }
            // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
        });
        menuViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            if (loading) {
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.GONE);
            }
        });
        menuViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
                // Если ошибка, тоже показываем пустое состояние
                tvEmptyMenu.setVisibility(View.VISIBLE);
                rvMenuDays.setVisibility(View.GONE);
            }
        });
    }
}