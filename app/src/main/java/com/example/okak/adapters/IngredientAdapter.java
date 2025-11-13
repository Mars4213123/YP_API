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
import java.util.Locale;

public class IngredientAdapter extends RecyclerView.Adapter<IngredientAdapter.IngredientViewHolder> {

    private List<ApiService.Ingredient> ingredients;

    public IngredientAdapter(List<ApiService.Ingredient> ingredients) {
        this.ingredients = ingredients != null ? ingredients : new ArrayList<>();
    }

    @NonNull
    @Override
    public IngredientViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.item_ingredient, parent, false);
        return new IngredientViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull IngredientViewHolder holder, int position) {
        ApiService.Ingredient ingredient = ingredients.get(position);
        holder.bind(ingredient);
    }

    @Override
    public int getItemCount() {
        return ingredients.size();
    }

    static class IngredientViewHolder extends RecyclerView.ViewHolder {
        private TextView tvIngredientName;
        private TextView tvIngredientQuantity;

        public IngredientViewHolder(@NonNull View itemView) {
            super(itemView);
            tvIngredientName = itemView.findViewById(R.id.tvIngredientName);
            tvIngredientQuantity = itemView.findViewById(R.id.tvIngredientQuantity);
        }

        public void bind(ApiService.Ingredient ingredient) {
            tvIngredientName.setText(ingredient.name);
            tvIngredientQuantity.setText(String.format(Locale.getDefault(),
                    "%.1f %s", ingredient.quantity, ingredient.unit));
        }
    }
}