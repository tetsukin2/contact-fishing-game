using UnityEngine;

public class BrailleTestTrigger : MonoBehaviour
{
    [SerializeField] private string _thumbPatternSequence;
    [SerializeField] private string _indexPatternSequence;
    [SerializeField] private bool _active = false;

    private bool patternActive = false;

    void Update()
    {
        if (!InputDeviceManager.Instance.BLEDevice.IsConnected) return;

        if (_active && !patternActive)
        {
            patternActive = true;
            //BraillePatternPlayer.Instance.PlayPatternSequence(_thumbPatternSequence, true);
            BraillePatternPlayer.Instance.PlayPatternSequence(_thumbPatternSequence, BraillePatternPlayer.Finger.THUMB, true);
            BraillePatternPlayer.Instance.PlayPatternSequence(_indexPatternSequence, BraillePatternPlayer.Finger.INDEX, true);
        }
        else if (!_active && patternActive)
        {
            patternActive = false;
            BraillePatternPlayer.Instance.StopPatternSequence(BraillePatternPlayer.Finger.BOTH);
            //BraillePatternPlayer.Instance.StopPatternSequence();
        }

    }
}
