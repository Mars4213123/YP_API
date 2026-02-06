package com.example.androidupproject.models;

import com.google.gson.annotations.SerializedName;

public class MenuItemDto {
    @SerializedName("RecipeId")
    public int recipeId;

    @SerializedName("RecipeTitle")
    public String recipeTitle;

    @SerializedName("Date")
    public String date; // "yyyy-MM-dd"

    @SerializedName("MealType")
    public String mealType; // "Завтрак", "Обед", "Ужин"
}