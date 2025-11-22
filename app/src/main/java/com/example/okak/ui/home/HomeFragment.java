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
import androidx.navigation.NavController;
import androidx.navigation.Navigation;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.adapters.MenuDayAdapter;
import com.example.okak.viewmodel.MenuViewModel;
import java.util.ArrayList;
import java.util.Arrays;

public class HomeFragment extends Fragment {

    private MenuViewModel menuViewModel;
    private RecyclerView rvMenuDays;
    private ProgressBar progressBar;
    private Button btnGenerateMenu;
    private TextView tvEmptyMenu;
    private MenuDayAdapter adapter;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_home, container, false);

        rvMenuDays = root.findViewById(R.id.rvMenuDays);
        progressBar = root.findViewById(R.id.progressBarHome);
        btnGenerateMenu = root.findViewById(R.id.btnGenerateMenu);
        tvEmptyMenu = root.findViewById(R.id.tvEmptyMenu);

        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);

        setupRecyclerView();
        setupObservers();

        btnGenerateMenu.setOnClickListener(v -> {
            menuViewModel.generateMenu(7, null, new ArrayList<>(), Arrays.asList("breakfast", "lunch", "dinner"), false);
            menuViewModel.loadCurrentMenu();
        });

        menuViewModel.loadCurrentMenu();

        return root;
    }

    private void setupRecyclerView() {
        adapter = new MenuDayAdapter(new ArrayList<>(), recipe -> {
            if (recipe != null && recipe.id > 0) {
                Bundle bundle = new Bundle();
                bundle.putInt("recipeId", recipe.id);
                NavController navController = Navigation.findNavController(requireView());
                try {
                    // ИСПРАВЛЕНО: правильный ID навигации
                    navController.navigate(R.id.navigationrecipedetail, bundle);
                } catch (IllegalArgumentException e) {
                    Toast.makeText(getContext(), "Ошибка навигации", Toast.LENGTH_SHORT).show();
                }
            }
        });
        rvMenuDays.setLayoutManager(new LinearLayoutManager(getContext()));
        rvMenuDays.setAdapter(adapter);
    }

    private void setupObservers() {
        menuViewModel.getCurrentMenu().observe(getViewLifecycleOwner(), menu -> {
            if (menu != null && menu.days != null && !menu.days.isEmpty()) {
                adapter.updateData(menu.days);
                rvMenuDays.setVisibility(View.VISIBLE);
                tvEmptyMenu.setVisibility(View.GONE);
            } else {
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.VISIBLE);
            }
        });

        menuViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            btnGenerateMenu.setEnabled(!loading);
        });

        menuViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null && !error.equals("")) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }
}
