package com.example.okak.adapters;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.example.okak.R;
import com.example.okak.network.ApiService;
import java.util.List;

public class ShoppingItemAdapter extends RecyclerView.Adapter<ShoppingItemAdapter.ViewHolder> {
    public interface OnItemToggleListener {
        void onItemToggle(int itemId, boolean isChecked);
    }

    private List<ApiService.ShoppingListItem> items;
    private OnItemToggleListener listener;

    public ShoppingItemAdapter(List<ApiService.ShoppingListItem> items, OnItemToggleListener listener) {
        this.items = items;
        this.listener = listener;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_shopping_item, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        ApiService.ShoppingListItem item = items.get(position);

        // C# DTO использует 'ingredientName'
        holder.tvName.setText(item.name);
        holder.tvQuantity.setText(item.quantity + " " + item.unit);

        holder.cbBought.setOnCheckedChangeListener(null);
        // C# DTO использует 'isPurchased'
        holder.cbBought.setChecked(item.isBought);
        holder.cbBought.setOnCheckedChangeListener((buttonView, isChecked) -> {
            if (listener != null) {
                listener.onItemToggle(item.id, isChecked);
            }
        });
    }

    @Override
    public int getItemCount() {
        return items.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        TextView tvName, tvQuantity;
        CheckBox cbBought;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvName = itemView.findViewById(R.id.tvItemName);
            tvQuantity = itemView.findViewById(R.id.tvItemQuantity);
            cbBought = itemView.findViewById(R.id.ivToggleBought);
        }
    }
    public void updateData(List<ApiService.ShoppingListItem> newItems) {
        this.items.clear();
        this.items.addAll(newItems);
        notifyDataSetChanged();
    }

}