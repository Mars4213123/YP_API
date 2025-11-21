package com.example.okak;

import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.lifecycle.ViewModelProvider;
import androidx.navigation.NavController;
import androidx.navigation.fragment.NavHostFragment;
import androidx.navigation.ui.NavigationUI;
import com.example.okak.network.AuthTokenManager;
import com.example.okak.viewmodel.MenuViewModel;
import com.example.okak.viewmodel.RecipeViewModel;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class MainActivity extends AppCompatActivity {
    private MenuViewModel menuViewModel;
    private RecipeViewModel recipeViewModel;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        int userId = AuthTokenManager.getUserId(this);

        menuViewModel = new ViewModelProvider(this).get(MenuViewModel.class);
        recipeViewModel = new ViewModelProvider(this).get(RecipeViewModel.class);

        if (userId != -1) {
            menuViewModel.setUserId(userId);
            recipeViewModel.setUserId(userId);
            menuViewModel.loadCurrentMenu();
        }

        BottomNavigationView navView = findViewById(R.id.nav_view);
        NavHostFragment navHostFragment = (NavHostFragment) getSupportFragmentManager()
                .findFragmentById(R.id.nav_host_fragment_activity_main);

        if (navHostFragment != null) {
            NavController navController = navHostFragment.getNavController();
            NavigationUI.setupWithNavController(navView, navController);
        }
    }
}