package com.example.androidupproject.models;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class MenuDto {
    @SerializedName("Id")
    public int id;

    @SerializedName("Name")
    public String name;

    @SerializedName("Items")
    public List<MenuItemDto> items;
}