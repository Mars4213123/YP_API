package com.example.androidupproject.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class RecipeDto {
    @SerializedName("Id")
    public int id;

    @SerializedName("Title")
    public String title;

    @SerializedName("Description")
    public String description;

    @SerializedName("Instructions")
    public String instructions; // Приходит одной строкой

    @SerializedName("ImageUrl")
    public String imageUrl;

    @SerializedName("Calories")
    public double calories;

    @SerializedName("Ingredients")
    public List<IngredientDto> ingredients;
}