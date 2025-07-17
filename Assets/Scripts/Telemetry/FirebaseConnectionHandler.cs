using Firebase.Auth;
using Firebase;
using UnityEngine;
using Firebase.Extensions;

public class FirebaseConnectionHandler : SingletonPersistent<FirebaseConnectionHandler>
{
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    public string CurrentAuthToken { get; private set; }

    // A flag to ensure we don't try to send data before auth is ready
    public bool AuthInitialized { get; private set; } = false;

    protected override void OnAwake()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(this, null); // Check initial state
                Debug.Log("Firebase dependencies resolved. Firebase Auth initialized.");
                AuthInitialized = true; // Set flag
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public void OnLogin()
    {
        SignInAnonymously(); // Automatically sign in anonymously on login
    }

    // This method is called whenever the authentication state changes
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            currentUser = auth.CurrentUser;
            if (currentUser != null)
            {
                Debug.Log($"Signed in as {currentUser.DisplayName ?? currentUser.UserId}");
                // Get the ID Token immediately after sign-in
                GetAndStoreIdToken();
            }
            else
            {
                Debug.Log("Signed out.");
                CurrentAuthToken = null;
            }
        }
    }

    // Function to get the ID Token
    async void GetAndStoreIdToken()
    {
        if (currentUser == null)
        {
            CurrentAuthToken = null;
            return;
        }

        try
        {
            // forceRefresh: false means it will return a cached token if valid,
            // or refresh it if it's expired or about to expire.
            // set to true to force a refresh (e.g., if you suspect a stale token).

            // Replace the problematic line with the following code:
            CurrentAuthToken = await currentUser.TokenAsync(false);
            Debug.Log($"Fetched ID Token: {CurrentAuthToken.Substring(0, 20)}..."); // Log first 20 chars for brevity
            // Now you can safely call your data POST method!
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting ID Token: {ex.Message}");
            CurrentAuthToken = null;
        }
    }

    // Example of initiating anonymous sign-in
    public void SignInAnonymously()
    {
        if (auth != null)
        {
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    return;
                }
                Debug.Log("User signed in anonymously.");
            });
        }
        else
        {
            Debug.LogError("Firebase Auth not initialized yet.");
        }
    }

    void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth.SignOut(); // Sign out when the handler is destroyed
            auth = null;
        }
    }
}
