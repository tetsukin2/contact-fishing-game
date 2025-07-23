
public class LoginScreen : GUIContainer
{
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
