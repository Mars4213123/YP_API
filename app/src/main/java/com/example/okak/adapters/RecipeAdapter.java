package com.example.okak.adapters;

import android.os.Bundle;
import android.util.Log;
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

        // Загрузка изображения
        Glide.with(holder.itemView.getContext())
                .load(recipe.imageUrl)
                .placeholder(R.drawable.ic_launcher_foreground)
                .error(R.drawable.ic_launcher_background)
                .into(holder.ivImage);


        holder.ivFavorite.setImageResource(recipe.isFavorite ? R.drawable.ic_favorite : R.drawable.ic_favorite);

        holder.itemView.setOnClickListener(v -> {
            Bundle bundle = new Bundle();
            bundle.putInt("recipeId", recipe.id);
            try {
                Navigation.findNavController(v).navigate(R.id.navigation_recipe_detail, bundle);
            } catch (Exception e) {
                Log.e("RecipeAdapter", "Navigation to recipe detail failed", e);
            }
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