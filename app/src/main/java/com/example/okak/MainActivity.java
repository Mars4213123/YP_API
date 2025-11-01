package com.example.okak;

import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.navigation.NavController;
// ИСПРАВЛЕНИЕ: Импортируем NavHostFragment
import androidx.navigation.fragment.NavHostFragment;
import androidx.navigation.ui.AppBarConfiguration;
import androidx.navigation.ui.NavigationUI;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class MainActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        BottomNavigationView navView = findViewById(R.id.nav_view);

        // --- ИСПРАВЛЕНИЕ НАЧАЛО ---
        // Это гарантированный способ получить NavController в onCreate
        // Он находит сам NavHostFragment в менеджере фрагментов
        NavHostFragment navHostFragment = (NavHostFragment) getSupportFragmentManager()
                .findFragmentById(R.id.nav_host_fragment_activity_main);

        // ... и затем получает контроллер у этого экземпляра
        NavController navController = navHostFragment.getNavController();
        // --- ИСПРАВЛЕНИЕ КОНЕЦ ---

        AppBarConfiguration appBarConfiguration = new AppBarConfiguration.Builder(
                R.id.navigation_home, R.id.navigation_search, R.id.navigation_shopping, R.id.navigation_favorites)
                .build();

        NavigationUI.setupActionBarWithNavController(this, navController, appBarConfiguration);
        NavigationUI.setupWithNavController(navView, navController);
    }
}