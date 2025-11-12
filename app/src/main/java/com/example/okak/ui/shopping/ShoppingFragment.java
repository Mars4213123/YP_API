package com.example.okak.ui.shopping;

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
import com.example.okak.adapters.ShoppingItemAdapter;
import com.example.okak.network.ApiService;
import com.example.okak.viewmodel.MenuViewModel;
import com.example.okak.viewmodel.ShoppingViewModel;
public class ShoppingFragment extends Fragment implements ShoppingItemAdapter.OnItemToggleListener {
    private ShoppingViewModel shoppingViewModel;
    private MenuViewModel menuViewModel;
    private RecyclerView rvItems;
    private ProgressBar progressBar;
    private Button btnGenerate;
    private TextView tvEmptyShopping; // ИСПРАВЛЕНИЕ: Добавлено
    private ShoppingItemAdapter adapter;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_shopping, container, false);
        rvItems = root.findViewById(R.id.rvShoppingItems);
        progressBar = root.findViewById(R.id.progressBarShopping);
        btnGenerate = root.findViewById(R.id.btnGenerateShoppingList);
        tvEmptyShopping = root.findViewById(R.id.tvEmptyShopping); // ИСПРАВЛЕНИЕ: Найдено

        shoppingViewModel = new ViewModelProvider(this).get(ShoppingViewModel.class);
        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);
        setupRecyclerView();
        setupObservers();
        btnGenerate.setOnClickListener(v -> {
            ApiService.MenuDetail currentMenu = menuViewModel.getCurrentMenu().getValue();
            if (currentMenu != null) {
                shoppingViewModel.generateShoppingList(currentMenu.id);
            } else {
                Toast.makeText(getContext(), "Сначала сгенерируйте меню на главном экране", Toast.LENGTH_LONG).show();
            }
        });
        shoppingViewModel.loadCurrentShoppingList();
        return root;
    }

    private void setupRecyclerView() {
        adapter = new ShoppingItemAdapter(new java.util.ArrayList<>(), this);
        rvItems.setLayoutManager(new LinearLayoutManager(getContext()));
        rvItems.setAdapter(adapter);
    }

    private void setupObservers() {
        shoppingViewModel.getShoppingList().observe(getViewLifecycleOwner(), list -> {
            if (list != null && list.items != null && !list.items.isEmpty()) {
                adapter = new ShoppingItemAdapter(list.items, this);
                rvItems.setAdapter(adapter);
                rvItems.setVisibility(View.VISIBLE);
                tvEmptyShopping.setVisibility(View.GONE);
            } else {
                adapter = new ShoppingItemAdapter(new java.util.ArrayList<>(), this);
                rvItems.setAdapter(adapter);
                rvItems.setVisibility(View.GONE);
                tvEmptyShopping.setVisibility(View.VISIBLE);
            }
        });
        shoppingViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            rvItems.setVisibility(loading ? View.GONE : View.VISIBLE);
            tvEmptyShopping.setVisibility(loading ? View.GONE : View.VISIBLE);
        });
        shoppingViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();

                rvItems.setVisibility(View.GONE);
                tvEmptyShopping.setVisibility(View.VISIBLE);
            }
        });
    }

    @Override
    public void onItemToggle(int itemId, boolean isChecked) {
        shoppingViewModel.toggleItem(itemId, isChecked);
    }
}