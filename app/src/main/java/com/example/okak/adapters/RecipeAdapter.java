package com.example.okak.adapters;

import android.os.Bundle; // <-- Добавлен импорт
import android.util.Log; // <-- Добавлен импорт
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.navigation.Navigation;
import androidx.recyclerview.widget.RecyclerView;

import com.bumptech.glide.Glide;
import com.example.okak.R;
import com.example.okak.network.ApiService;
import java.util.List;

public class RecipeAdapter extends RecyclerView.Adapter<RecipeAdapter.ViewHolder> {
    private List<ApiService.RecipeShort> recipes;
    public RecipeAdapter(List<ApiService.RecipeShort> recipes) {
        this.recipes = recipes;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_recipe, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        ApiService.RecipeShort recipe = recipes.get(position);
        holder.tvTitle.setText(recipe.title);
        holder.tvCalories.setText("Калории: " + recipe.calories);
        Glide.with(holder.itemView).load(recipe.imageUrl).into(holder.ivImage);
        holder.ivFavorite.setImageResource(recipe.isFavorite ? R.drawable.ic_favorite : R.drawable.ic_favorite);
        holder.itemView.setOnClickListener(v -> {
            // --- ИСПРАВЛЕНИЕ ---
            // Логика Safe Args (RecipeDetailFragmentDirections) не будет работать,
            // когда этот адаптер вызывается из HomeFragment (через MenuDayAdapter),
            // так как в navigation graph нет action от home к detail.
            //
            // Самый надежный способ, который будет работать из ЛЮБОГО фрагмента
            // - это передать ID через Bundle, как и предполагает комментарий "bundle with id".

            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);

            try {
                // Навигация к R.id.navigation_recipe_detail с передачей Bundle
                Navigation.findNavController(v).navigate(R.id.navigation_recipe_detail, bundle);
            } catch (Exception e) {
                // Добавляем лог на случай, если навигация не удалась
                Log.e("RecipeAdapter", "Navigation to recipe detail failed", e);
            }
            // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
        });
    }

    @Override
    public int getItemCount() {
        return recipes.size();
    }


    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvTitle, tvCalories;
        ImageView ivImage, ivFavorite;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvTitle = itemView.findViewById(R.id.tvRecipeTitle);
            tvCalories = itemView.findViewById(R.id.tvRecipeCalories);
            ivImage = itemView.findViewById(R.id.ivRecipeImage);

            ivFavorite = itemView.findViewById(R.id.ivFavorite);
        }
    }
}