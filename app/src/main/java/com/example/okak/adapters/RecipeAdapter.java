package com.example.okak.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.network.ApiService;
import java.util.ArrayList;
import java.util.List;

public class RecipeAdapter extends RecyclerView.Adapter<RecipeAdapter.RecipeViewHolder> {

    private List<ApiService.RecipeShort> recipes;
    private OnRecipeClickListener listener;

    public interface OnRecipeClickListener {
        void onRecipeClick(ApiService.RecipeShort recipe);
    }

    public RecipeAdapter(List<ApiService.RecipeShort> recipes) {
        this.recipes = recipes != null ? recipes : new ArrayList<>();
    }

    public void setOnRecipeClickListener(OnRecipeClickListener listener) {
        this.listener = listener;
    }

    public void updateData(List<ApiService.RecipeShort> newRecipes) {
        this.recipes.clear();
        if (newRecipes != null) {
            this.recipes.addAll(newRecipes);
        }
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public RecipeViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_recipe, parent, false);
        return new RecipeViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull RecipeViewHolder holder, int position) {
        ApiService.RecipeShort recipe = recipes.get(position);
        holder.bind(recipe);
    }

    @Override
    public int getItemCount() {
        return recipes.size();
    }

    class RecipeViewHolder extends RecyclerView.ViewHolder {
        private TextView tvTitle;
        private TextView tvCalories;
        private TextView tvTime;
        private TextView tvDifficulty;

        public RecipeViewHolder(@NonNull View itemView) {
            super(itemView);
            tvTitle = itemView.findViewById(R.id.tvRecipeTitle);
            tvCalories = itemView.findViewById(R.id.tvRecipeCalories);
            tvTime = itemView.findViewById(R.id.tvRecipeTime);
            tvDifficulty = itemView.findViewById(R.id.tvRecipeDifficulty);

            itemView.setOnClickListener(v -> {
                if (listener != null) {
                    int position = getAdapterPosition();
                    if (position != RecyclerView.NO_POSITION) {
                        listener.onRecipeClick(recipes.get(position));
                    }
                }
            });
        }

        public void bind(ApiService.RecipeShort recipe) {
            tvTitle.setText(recipe.title);
            tvCalories.setText(String.format("%.0f ккал", recipe.calories));
            tvTime.setText(String.format("%d мин", recipe.prepTime + recipe.cookTime));
            tvDifficulty.setText(recipe.difficulty);
        }
    }
}