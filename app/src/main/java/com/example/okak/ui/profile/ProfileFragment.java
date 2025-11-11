package com.example.okak.ui.profile;

import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import com.example.okak.R;
import com.example.okak.LoginActivity;
import com.example.okak.network.AuthTokenManager;
import com.example.okak.viewmodel.UserViewModel;

public class ProfileFragment extends Fragment {
    private UserViewModel userViewModel;
    private TextView tvUsername, tvEmail, tvFullName, tvAllergies;
    private Button btnLogout;
    private ProgressBar progressBar;

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View root = inflater.inflate(R.layout.fragment_profile, container, false);
        tvUsername = root.findViewById(R.id.tvUsername);
        tvEmail = root.findViewById(R.id.tvEmail);
        tvFullName = root.findViewById(R.id.tvFullName);
        tvAllergies = root.findViewById(R.id.tvAllergies);
        btnLogout = root.findViewById(R.id.btnLogout);
        progressBar = root.findViewById(R.id.progressBarProfile);
        userViewModel = new ViewModelProvider(this).get(UserViewModel.class);
        setupObservers();
        btnLogout.setOnClickListener(v -> logout());
        userViewModel.loadProfile();
        return root;
    }

    private void setupObservers() {
        userViewModel.getProfile().observe(getViewLifecycleOwner(), profile -> {
            if (profile != null) {
                tvUsername.setText("Username: " + profile.username);
                tvEmail.setText("Email: " + profile.email);
                tvFullName.setText("Полное имя: " + profile.fullName);
            }
        });
        userViewModel.getAllergies().observe(getViewLifecycleOwner(), allergies -> {
            if (allergies != null && !allergies.isEmpty()) {
                String allergiesStr = String.join(", ", allergies);
                tvAllergies.setText("Аллергии: " + allergiesStr);
            } else {
                tvAllergies.setText("Аллергии: Нет");
            }
        });
        userViewModel.getLoading().observe(getViewLifecycleOwner(), loading -> {
            progressBar.setVisibility(loading ? View.VISIBLE : View.GONE);
        });
        userViewModel.getError().observe(getViewLifecycleOwner(), error -> {
            if (error != null) {
                Toast.makeText(getContext(), error, Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void logout() {
        AuthTokenManager.clearToken(requireContext().getApplicationContext());
        Toast.makeText(getContext(), "Выход выполнен", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(getActivity(), LoginActivity.class);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(intent);
        getActivity().finish();
    }
}