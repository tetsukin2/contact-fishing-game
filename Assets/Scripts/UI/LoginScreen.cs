using TMPro;
using UnityEngine;
using Firebase.Auth;
using Firebase.Extensions;
using System;

/// <summary>
/// Handles login and registration for Firebase Auth + REST Firestore.
/// </summary>
public class LoginScreen : GUIContainer
{
    [Header("Login Input Fields")]
    [SerializeField] private TMP_InputField LoginEmailField;
    [SerializeField] private TMP_InputField LoginPasswordField;

    [Header("Register Input Fields")]
    [SerializeField] private TMP_InputField RegisterUsernameField;
    [SerializeField] private TMP_InputField RegisterEmailField;
    [SerializeField] private TMP_InputField RegisterPasswordField;

    [Header("Panels")]
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject RegisterPanel;

    public void LoginWithEmailPassword()
    {
        string email = LoginEmailField.text.Trim();
        string password = LoginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email or password is empty. Aborting login.");
            return;
        }

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Login failed: " + task.Exception?.Flatten().Message);
                    return;
                }

                Debug.Log("✅ User logged in: " + task.Result.User.UserId);
                OnLogin();
            });
    }

    public void RegisterNewAccount()
    {
        string username = RegisterUsernameField.text.Trim();
        string email = RegisterEmailField.text.Trim();
        string password = RegisterPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Missing fields for registration.");
            return;
        }

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("Registration failed: " + task.Exception?.Flatten().Message);
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                Debug.Log("✅ New user registered: " + newUser.UserId);

                var userData = new
                {
                    name = username,
                    email = email,
                    role = "patient",
                    createdAt = DateTime.UtcNow.ToString("o"),
                    therapistId = (string)null
                };

                FirebaseUploadHandler.Instance.PostData("users", userData, newUser.UserId);
                Debug.Log("🟢 User document queued for upload via REST.");

                OnLogin();
            });
    }

    public void ShowRegisterUI()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
    }

    public void ShowLoginUI()
    {
        RegisterPanel.SetActive(false);
        LoginPanel.SetActive(true);
    }

    public void LoginAsGuest()
    {
        FirebaseConnectionHandler.Instance.SignInAnonymously();
        FirebaseConnectionHandler.Instance.SignInSuccess.AddListener(OnLogin);
    }

    public void OnLogin()
    {
        FirebaseConnectionHandler.Instance.SignInSuccess.RemoveListener(OnLogin);
        MainMenuUIController.Instance.OnLoginComplete();
    }
}

