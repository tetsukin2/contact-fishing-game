public static class UserTestConfig
{
    public static bool IsUserTestMode = false;
    public static bool HapticsEnabled = true;
    public static int CurrentPhase = 0;

    // -1 means "do not override normal value"
    public static int OverrideFishTotalToCatch = -1;

    public static void ResetToNormalMode()
    {
        IsUserTestMode = false;
        HapticsEnabled = true;
        CurrentPhase = 0;
        OverrideFishTotalToCatch = -1;
    }
}