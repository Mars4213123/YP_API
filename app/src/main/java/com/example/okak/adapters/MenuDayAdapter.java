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

public class MenuDayAdapter extends RecyclerView.Adapter<MenuDayAdapter.ViewHolder> {
    private List<ApiService.MenuDay> days;

    public MenuDayAdapter(List<ApiService.MenuDay> days) {
        this.days = days;
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
        holder.tvDate.setText(day.date);

        // Считаем калории дня на клиенте, так как C# DTO не предоставляет
        // 'totalCalories' на уровне дня.
        double dailyCalories = 0;
        if (day.meals != null) {
            for (ApiService.Meal meal : day.meals) {
                dailyCalories += meal.calories;
            }
        }
        holder.tvCalories.setText("Общие калории: " + String.format("%.0f", dailyCalories));


        // ИСПРАВЛЕНО: DTO Meal больше не содержит вложенный RecipeShort,
        // а содержит поля рецепта напрямую.
        List<ApiService.RecipeShort> mealsAsRecipes = new ArrayList<>();
        if (day.meals != null) {
            for (ApiService.Meal meal : day.meals) {
                // Преобразуем Meal в RecipeShort для адаптера
                ApiService.RecipeShort recipe = new ApiService.RecipeShort();
                recipe.id = meal.recipeId;
                recipe.title = meal.recipeTitle;
                recipe.calories = meal.calories;
                recipe.imageUrl = meal.imageUrl;
                mealsAsRecipes.add(recipe);
            }
        }

        RecipeAdapter mealAdapter = new RecipeAdapter(mealsAsRecipes);
        holder.rvMeals.setLayoutManager(new LinearLayoutManager(holder.itemView.getContext(), LinearLayoutManager.HORIZONTAL, false));
        holder.rvMeals.setAdapter(mealAdapter);
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
        this.days.addAll(newDays);
        notifyDataSetChanged();
    }

}