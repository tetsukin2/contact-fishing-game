using UnityEngine;

public class GameEndLevelState : LevelState
{
    public GameEndLevelState(LevelManager gameManager) : base(gameManager) { }

    private float _gameEndTimer = 0f;
    private bool _transitionHandled = false;

    public override void Enter()
    {
        _gameEndTimer = 0f;
        _transitionHandled = false;

        CameraController.Instance.SetCameraView(CameraController.CameraView.Gameplay);

        // In normal gameplay, allow player to skip.
        // In user test mode, do NOT allow skipping because the flow should advance automatically.
        if (!UserTestConfig.IsUserTestMode)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.AddListener(EndState);
        }
    }

    public override void Update()
    {
        if (_transitionHandled)
            return;

        _gameEndTimer += Time.deltaTime;

        // Use normal end duration as the wait before auto-transition
        if (_gameEndTimer >= gameManager.GameEndDuration)
        {
            EndState();
        }
    }

    private void EndState()
    {
        if (_transitionHandled)
            return;

        _transitionHandled = true;

        if (UserTestConfig.IsUserTestMode)
        {
            // Phase 1 → Phase 2
            if (UserTestConfig.CurrentPhase == 1)
            {
                Debug.Log("Phase 1 complete → Phase 2 (with haptics)");

                UserTestConfig.CurrentPhase = 2;
                UserTestConfig.HapticsEnabled = true;

                gameManager.StartGameFromUI();
                return;
            }

            // Phase 2 → Phase 3 (tactile test)
            if (UserTestConfig.CurrentPhase == 2)
            {
                Debug.Log("Phase 2 complete → Phase 3 (tactile test)");

                UserTestConfig.CurrentPhase = 3;
                UserTestConfig.HapticsEnabled = true;

                gameManager.TransitionToState(gameManager.TactileDiscriminationState);
                return;
            }
        }

        gameManager.TransitionToState(gameManager.EndScoreState);
    }

    public override void Exit()
    {
        if (InputDeviceManager.Instance != null &&
            InputDeviceManager.Instance.JoystickInput != null)
        {
            InputDeviceManager.Instance.JoystickInput.JoystickPressed.RemoveListener(EndState);
        }

        Debug.Log("Exiting Game End State");
    }
}