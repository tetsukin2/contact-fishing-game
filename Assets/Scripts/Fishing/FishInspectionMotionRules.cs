using UnityEngine;

public static class FishInspectionMotionRules
{
    public static float GetPronationPrepareYThreshold()
    {
        return ResourceSystem.Instance.GameplayConfig.RollRightAngle;
    }

    public static float GetPronationHoldYThreshold()
    {
        return GetPronationPrepareYThreshold() * 0.40f;
    }

    public static float GetPronationLostResetYThreshold()
    {
        return GetPronationPrepareYThreshold() * 0.20f;
    }

    public static float GetMinimumExecuteDelaySeconds()
    {
        return 0.15f;
    }
public static bool HasReachedPronationPrepare(InputDeviceRotationHelper rotationHelper)
{
    /*
     * Use direct Y-axis check for inspection prepare.
     *
     * RollRightAngle is usually negative, e.g. -0.8.
     * So pronation prepare is reached when CurrentY <= -0.8.
     *
     * This avoids possible first-attempt issues from HasReachedRotationY(),
     * which may depend on rotation history.
     */
    float threshold = GetPronationPrepareYThreshold();

    if (threshold < 0f)
    {
        return rotationHelper.CurrentY <= threshold;
    }

    return rotationHelper.CurrentY >= threshold;
}

    public static bool IsMaintainingPronationPosture(InputDeviceRotationHelper rotationHelper)
    {
        return rotationHelper.CurrentY <= GetPronationHoldYThreshold();
    }

    public static bool HasLostPronationPosture(InputDeviceRotationHelper rotationHelper)
    {
        /*
         * Relaxed reset:
         * Do not reset just because Y changes during flexion/extension.
         *
         * Reset only when the controller is close to neutral again.
         */
        return Mathf.Abs(rotationHelper.CurrentX) < 0.30f
            && Mathf.Abs(rotationHelper.CurrentY) < 0.30f
            && rotationHelper.CurrentZ > 0.70f;
    }

    public static bool CanExecuteAfterPrepare(float prepareReachedTime)
    {
        return Time.time - prepareReachedTime >= GetMinimumExecuteDelaySeconds();
    }

    public static bool HasReachedInspectExtension(InputDeviceRotationHelper rotationHelper, float prepareReachedTime)
    {
        /*
         * Extension after pronation prepare.
         * Uses X + Z to avoid accepting radial deviation.
         */
        return CanExecuteAfterPrepare(prepareReachedTime)
            && rotationHelper.CurrentX <= -0.55f
            && rotationHelper.CurrentZ <= 0.35f;
    }

    public static bool HasReachedDropFlexion(InputDeviceRotationHelper rotationHelper, float prepareReachedTime)
    {
        /*
         * Flexion after pronation prepare.
         * Uses X + Z to avoid accepting ulnar deviation.
         */
        return CanExecuteAfterPrepare(prepareReachedTime)
            && rotationHelper.CurrentX >= 0.55f
            && rotationHelper.CurrentZ <= 0.60f;
    }
}