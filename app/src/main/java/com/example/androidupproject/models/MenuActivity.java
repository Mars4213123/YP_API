package com.example.androidupproject.models;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.example.androidupproject.R;
import com.example.androidupproject.models.MenuDto;
import com.example.androidupproject.models.MenuItemDto;
import com.example.androidupproject.models.SessionManager;
import com.example.androidupproject.network.ApiClient;
import com.example.androidupproject.network.ApiResponse;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MenuActivity extends AppCompatActivity {

    private List<MenuItemDto> menuItemsList = new ArrayList<>();
    private RecyclerView recyclerView;
    private MenuAdapter adapter;
    private SessionManager sessionManager;
    private int currentMenuId = 0;
    private TextView tvMenuTitle;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_menu);

        sessionManager = new SessionManager(this);
        tvMenuTitle = findViewById(R.id.tvMenuTitle);
        recyclerView = findViewById(R.id.rvMenu);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        loadMenuIntoList(sessionManager.getUserId(), menuItemsList, adapter);


        adapter = new MenuAdapter();
        recyclerView.setAdapter(adapter);

        findViewById(R.id.btnGenerateShop).setOnClickListener(v -> generateShoppingList());

        loadMenu();
    }
    private void refreshMenu() {
        loadMenuIntoList(sessionManager.getUserId(), menuItemsList, adapter);
    }
    private void loadMenu() {
        ApiClient.getService().getCurrentMenu(sessionManager.getUserId()).enqueue(new Callback<ApiResponse<MenuDto>>() {
            @Override
            public void onResponse(Call<ApiResponse<MenuDto>> call, Response<ApiResponse<MenuDto>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().data != null) {
                    MenuDto menu = response.body().data;
                    currentMenuId = menu.id;
                    adapter.setItems(menu.items);
                } else {
                    Toast.makeText(MenuActivity.this, "Меню не найдено. Сгенерируйте новое.", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<MenuDto>> call, Throwable t) {
                Toast.makeText(MenuActivity.this, "Ошибка сети", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void generateShoppingList() {
        if (currentMenuId == 1) return;

        ApiClient.getService().generateShoppingList(currentMenuId, sessionManager.getUserId()).enqueue(new Callback<ApiResponse<Void>>() {
            @Override
            public void onResponse(Call<ApiResponse<Void>> call, Response<ApiResponse<Void>> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(MenuActivity.this, "Список покупок создан!", Toast.LENGTH_SHORT).show();
                }
            }
            @Override
            public void onFailure(Call<ApiResponse<Void>> call, Throwable t) {}
        });
    }
    private void loadMenuIntoList(int userId, final List<MenuItemDto> menuItemsList, final MenuAdapter adapter) {
        // Очищаем существующие данные
        if (menuItemsList != null) {
            menuItemsList.clear();
        }

        // Выполняем запрос к API
        ApiClient.getService().getCurrentMenu(userId).enqueue(new Callback<ApiResponse<MenuDto>>() {
            @Override
            public void onResponse(Call<ApiResponse<MenuDto>> call, Response<ApiResponse<MenuDto>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().data != null) {
                    MenuDto menu = response.body().data;

                    // Заполняем локальный список
                    if (menu.items != null && menuItemsList != null) {
                        menuItemsList.addAll(menu.items);
                    }

                    // Обновляем адаптер, если он передан
                    if (adapter != null) {
                        adapter.setItems(menu.items != null ? menu.items : new ArrayList<>());
                    }

                    // Обновляем информацию о меню в UI
                    runOnUiThread(() -> {
                        tvMenuTitle.setText(menu.name != null ? menu.name : "Меню на неделю");
                        Toast.makeText(MenuActivity.this,
                                "Загружено " + (menu.items != null ? menu.items.size() : 0) + " блюд",
                                Toast.LENGTH_SHORT).show();
                    });

                    System.out.println("[loadMenuIntoList] Успешно загружено " +
                            (menu.items != null ? menu.items.size() : 0) + " элементов меню");
                } else {
                    runOnUiThread(() -> {
                        Toast.makeText(MenuActivity.this,
                                "Меню не найдено. Сгенерируйте новое.",
                                Toast.LENGTH_LONG).show();
                    });
                    System.out.println("[loadMenuIntoList] Меню не найдено или пустое");
                }
            }

            @Override
            public void onFailure(Call<ApiResponse<MenuDto>> call, Throwable t) {
                runOnUiThread(() -> {
                    Toast.makeText(MenuActivity.this,
                            "Ошибка загрузки меню: " + t.getMessage(),
                            Toast.LENGTH_SHORT).show();
                });
                System.err.println("[loadMenuIntoList] Ошибка сети: " + t.getMessage());
                t.printStackTrace();
            }
        });
    }
    // Адаптер
    class MenuAdapter extends RecyclerView.Adapter<MenuAdapter.ViewHolder> {
        // 1. Инициализируем пустым списком сразу, чтобы не было null при старте
        private List<MenuItemDto> items = new ArrayList<>();

        public void setItems(List<MenuItemDto> items) {
            // 2. ЗАЩИТА: Если сервер прислал null, используем пустой список
            if (items != null) {
                this.items = items;
            } else {
                this.items = new ArrayList<>();
            }
            notifyDataSetChanged();
        }

        @NonNull
        @Override
        public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_menu_meal, parent, false);
            return new ViewHolder(view);
        }

        @Override
        public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
            // Добавим проверку границ, на всякий случай
            if (items == null || position >= items.size()) return;

            MenuItemDto item = items.get(position);
            holder.tvDateMeal.setText(item.date + " (" + item.mealType + ")");
            holder.tvRecipe.setText(item.recipeTitle);

            holder.btnOpen.setOnClickListener(v -> {
                Intent intent = new Intent(MenuActivity.this, com.example.androidupproject.RecipeDetailActivity.class);
                intent.putExtra("RECIPE_ID", item.recipeId);
                startActivity(intent);
            });
        }

        @Override
        public int getItemCount() {
            // 3. ЗАЩИТА: Если список все-таки null, возвращаем 0
            return (items != null) ? items.size() : 0;
        }

        class ViewHolder extends RecyclerView.ViewHolder {
            TextView tvDateMeal, tvRecipe;
            Button btnOpen;
            public ViewHolder(View itemView) {
                super(itemView);
                tvDateMeal = itemView.findViewById(R.id.tvDateMeal);
                tvRecipe = itemView.findViewById(R.id.tvRecipeName);
                btnOpen = itemView.findViewById(R.id.btnOpenRecipe);
            }
        }
    }
}