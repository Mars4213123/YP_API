package com.example.androidupproject.models;

import com.google.gson.annotations.SerializedName;

public class IngredientDto {
    // Сервер может прислать Id или id
    @SerializedName(value = "Id", alternate = {"id"})
    public int id;

    // ГЛАВНОЕ ИСПРАВЛЕНИЕ:
    // Мы говорим: "Если придет 'Name', 'name', 'ProductName' или 'productName' — запиши это сюда".
    @SerializedName(value = "Name", alternate = {"name", "ProductName", "productName"})
    public String name;

    @SerializedName(value = "Unit", alternate = {"unit"})
    public String unit;

    @SerializedName(value = "Category", alternate = {"category"})
    public String category;

    // Это поле нужно для удаления, если сервер присылает IngredientId
    @SerializedName(value = "IngredientId", alternate = {"ingredientId"})
    public int ingredientId;

    public IngredientDto() {}

    public IngredientDto(String name, String unit) {
        this.name = name;
        this.unit = unit;
        this.category = "Разное";
        this.id = 0;
    }

    // Вспомогательный метод для получения правильного ID при удалении
    public int getDeleteId() {
        // Если есть ingredientId (из таблицы холодильника), используем его
        if (ingredientId != 0) return ingredientId;
        // Иначе обычный id
        return id;
    }
}