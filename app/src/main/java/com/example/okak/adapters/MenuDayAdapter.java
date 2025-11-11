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
        holder.tvCalories.setText("Общие калории: " + day.totalCalories);

        List<ApiService.RecipeShort> mealsAsRecipes = new java.util.ArrayList<>();
        if (day.meals != null) {
            for (ApiService.Meal meal : day.meals) {
                if (meal.recipe != null) {
                    mealsAsRecipes.add(meal.recipe);
                }
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
}