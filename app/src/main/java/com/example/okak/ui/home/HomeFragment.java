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
    public View onCreateView(@NonNull LayoutInflater inflater,
                             ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_home, container, false);
        rvMenuDays = root.findViewById(R.id.rvMenuDays);
        progressBar = root.findViewById(R.id.progressBarHome);
        btnGenerateMenu = root.findViewById(R.id.btnGenerateMenu);
        tvEmptyMenu = root.findViewById(R.id.tvEmptyMenu);

        menuViewModel = new ViewModelProvider(requireActivity()).get(MenuViewModel.class);

        // Настраиваем адаптер с обработчиком нажатия
        setupRecyclerView();
        setupObservers();

        btnGenerateMenu.setOnClickListener(v -> {
            int days = 7;
            // Генерируем меню
            menuViewModel.generateMenu(
                    days,
                    null,
                    new ArrayList<>(),
                    Arrays.asList("breakfast", "lunch", "dinner"),
                    false
            );
        });

        // Загружаем меню при открытии
        menuViewModel.loadCurrentMenu();

        return root;
    }

    private void setupRecyclerView() {
        // Создаем адаптер и передаем обработчик клика (Lambda)
        adapter = new MenuDayAdapter(new ArrayList<>(), recipe -> {
            // Логика перехода на экран деталей
            if (recipe != null && recipe.id > 0) {
                Bundle bundle = new Bundle();
                bundle.putInt("recipeId", recipe.id);
                // Используем ID фрагмента назначения напрямую для надежности
                Navigation.findNavController(requireView())
                        .navigate(R.id.recipeDetailFragment, bundle);
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
                adapter.updateData(new ArrayList<>());
                rvMenuDays.setVisibility(View.GONE);
                tvEmptyMenu.setVisibility(View.VISIBLE);
            }
        });

        menuViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
            btnGenerateMenu.setEnabled(!loading);

            // Скрываем список только если он пуст во время загрузки
            if (loading && adapter.getItemCount() == 0) {
                rvMenuDays.setVisibility(View.GONE);
            } else if (!loading && adapter.getItemCount() > 0) {
                rvMenuDays.setVisibility(View.VISIBLE);
            }
        });

        menuViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                // Показываем ошибку только тостом, не скрывая контент если он есть
                if (!error.equals("Меню не найдено")) {
                    Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
                }

                if (adapter.getItemCount() == 0) {
                    tvEmptyMenu.setVisibility(View.VISIBLE);
                    rvMenuDays.setVisibility(View.GONE);
                }
            }
        });
    }
}