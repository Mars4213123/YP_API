package com.example.okak.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.network.ApiService;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public class MenuDayAdapter extends RecyclerView.Adapter<MenuDayAdapter.ViewHolder> {

    private List<ApiService.MenuDay> days;
    // Добавляем слушатель для передачи клика из вложенного адаптера наружу
    private final RecipeAdapter.OnRecipeClickListener recipeClickListener;

    public MenuDayAdapter(List<ApiService.MenuDay> days, RecipeAdapter.OnRecipeClickListener recipeClickListener) {
        this.days = days;
        this.recipeClickListener = recipeClickListener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_menu_day, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        ApiService.MenuDay day = days.get(position);

        // Форматируем дату (просто обрезаем время, если оно есть)
        String dateStr = day.date;
        if (dateStr != null && dateStr.length() > 10) {
            dateStr = dateStr.substring(0, 10);
        }
        holder.tvDate.setText(dateStr);

        // Считаем калории
        double dailyCalories = 0;
        List<ApiService.RecipeShort> mealsAsRecipes = new ArrayList<>();

        if (day.meals != null) {
            for (ApiService.Meal meal : day.meals) {
                dailyCalories += meal.calories;

                // Преобразуем Meal в RecipeShort для адаптера
                ApiService.RecipeShort recipe = new ApiService.RecipeShort();
                recipe.id = meal.recipeId;
                recipe.title = meal.recipeTitle;
                recipe.calories = meal.calories;
                recipe.imageUrl = meal.imageUrl;
                // Используем тип приема пищи как тег для отображения
                recipe.difficulty = getMealTypeName(meal.mealType);

                mealsAsRecipes.add(recipe);
            }
        }

        holder.tvCalories.setText(String.format(Locale.getDefault(), "Общие калории: %.0f", dailyCalories));

        // Настраиваем вложенный адаптер рецептов
        RecipeAdapter mealAdapter = new RecipeAdapter(mealsAsRecipes);

        // ВАЖНО: Передаем слушатель нажатия во вложенный адаптер
        mealAdapter.setOnRecipeClickListener(recipeClickListener);

        holder.rvMeals.setLayoutManager(new LinearLayoutManager(holder.itemView.getContext(), LinearLayoutManager.HORIZONTAL, false));
        holder.rvMeals.setAdapter(mealAdapter);
    }

    private String getMealTypeName(String type) {
        if (type == null) return "";
        // Простой маппинг типов, если приходят числа или английские названия
        switch (type.toLowerCase()) {
            case "1": return "Завтрак";
            case "2": return "Обед";
            case "3": return "Ужин";
            case "4": return "Перекус";
            case "breakfast": return "Завтрак";
            case "lunch": return "Обед";
            case "dinner": return "Ужин";
            default: return type;
        }
    }

    @Override
    public int getItemCount() {
        return days.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvDate, tvCalories;
        RecyclerView rvMeals;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvDate = itemView.findViewById(R.id.tvDayDate);
            tvCalories = itemView.findViewById(R.id.tvDayCalories);
            rvMeals = itemView.findViewById(R.id.rvDayMeals);
        }
    }

    public void updateData(List<ApiService.MenuDay> newDays) {
        this.days.clear();
        if (newDays != null) {
            this.days.addAll(newDays);
        }
        notifyDataSetChanged();
    }
}