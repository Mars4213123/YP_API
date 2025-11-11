package com.example.okak.ui.shopping;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.okak.R;
import com.example.okak.adapters.ShoppingItemAdapter;
import com.example.okak.network.ApiService;
import com.example.okak.viewmodel.MenuViewModel; // <-- Добавлен импорт
import com.example.okak.viewmodel.ShoppingViewModel;

import java.util.ArrayList; // <-- Добавлен импорт

// --- ИСПРАВЛЕНИЕ: Реализуем OnItemToggleListener ---
public class ShoppingFragment extends Fragment implements ShoppingItemAdapter.OnItemToggleListener {
    private ShoppingViewModel shoppingViewModel;
    private MenuViewModel menuViewModel; // <-- Добавлен MenuViewModel
    private RecyclerView rvItems;
    private ProgressBar progressBar;
    private Button btnGenerate;
    private ShoppingItemAdapter adapter; // <-- Добавили адаптер как поле

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_shopping, container, false);
        rvItems = root.findViewById(R.id.rvShoppingItems);
        progressBar = root.findViewById(R.id.progressBarShopping);
        btnGenerate = root.findViewById(R.id.btnGenerateShoppingList);

        shoppingViewModel = new ViewModelProvider(this).get(ShoppingViewModel.class);
        // Получаем общий MenuViewModel от Activity
        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);

        setupRecyclerView(); // <-- Настраиваем RecyclerView
        setupObservers();

        btnGenerate.setOnClickListener(v -> {
            // --- ИСПРАВЛЕНИЕ: Получаем ID из MenuViewModel ---
            ApiService.MenuDetail currentMenu = menuViewModel.getCurrentMenu().getValue();
            if (currentMenu != null) {
                shoppingViewModel.generateShoppingList(currentMenu.id);
            } else {
                Toast.makeText(getContext(), "Сначала сгенерируйте меню на главном экране", Toast.LENGTH_LONG).show();
            }
            // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
        });

        shoppingViewModel.loadCurrentShoppingList();
        return root;
    }

    private void setupRecyclerView() {
        // Инициализируем адаптер с 'this' (Fragment) в качестве Listener
        adapter = new ShoppingItemAdapter(new ArrayList<>(), this);
        rvItems.setLayoutManager(new LinearLayoutManager(getContext()));
        rvItems.setAdapter(adapter);
    }

    private void setupObservers() {
        shoppingViewModel.getShoppingList().observe(getViewLifecycleOwner(), list -> {
            if (list != null && list.items != null) {
                // Обновляем данные в существующем адаптере
                adapter = new ShoppingItemAdapter(list.items, this);
                rvItems.setAdapter(adapter);
            } else {
                // Очищаем список, если он null
                adapter = new ShoppingItemAdapter(new ArrayList<>(), this);
                rvItems.setAdapter(adapter);
            }
        });
        shoppingViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            rvItems.setVisibility(loading ? View.GONE : View.VISIBLE);
        });
        shoppingViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    // --- ИСПРАВЛЕНИЕ: Реализация метода Listener ---
    @Override
    public void onItemToggle(int itemId, boolean isChecked) {
        // Передаем вызов в ViewModel
        shoppingViewModel.toggleItem(itemId, isChecked);
    }
    // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
}