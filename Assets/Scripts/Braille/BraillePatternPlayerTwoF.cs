using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BraillePatternPlayerTwoF : MonoBehaviour
{
    public enum Finger
    {
        THUMB = 0,
        INDEX = 1,
        BOTH
    }

    public static BraillePatternPlayerTwoF Instance { get; private set; }

    public float PatternDelay = 0.2f; // Delay between patterns

    private class EncodedBraillePatternSequence
    {
        public string SequenceName; // PromptName of the sequence
        public List<EncodedBraillePattern> Values; // List of encoded patterns
    }

    /// <summary>
    /// Encoded Braille pattern to be sent to the Braille pins
    /// </summary>
    private class EncodedBraillePattern
    {
        public int Value1;
        public int Value2;
    }

    [SerializeField] private List<BraillePinPatternSequence> _braillePatternSequences;

    private List<EncodedBraillePatternSequence> _encodedBraillePatternSequences = new();

    private EncodedBraillePatternSequence _currentIndexSequence;
    private int _currentIndexPatternIndex = 0;
    private bool _isIndexPatternLoop = false;

    private EncodedBraillePatternSequence _currentThumbSequence;
    private int _currentThumbPatternIndex = 0;
    private bool _isThumbPatternLoop = false;

    private Coroutine _patternCoroutine;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of BraillePatternPlayer exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Encode a list of sequences
        foreach (var sequence in _braillePatternSequences)
        {
            _encodedBraillePatternSequences.Add(EncodePatternSequence(sequence));
        }

        //_patternCoroutine = StartCoroutine(RunSequence());
    }

    public void PlayPatternSequence(string name, bool loop)
    {
        PlayPatternSequence(name, Finger.BOTH, loop);
    }

    public void PlayPatternSequence(string name, Finger side)
    {
        PlayPatternSequence(name, side, false);
    }

    public void PlayPatternSequence(string name, Finger side, bool loop)
    {
        foreach (var sequence in _encodedBraillePatternSequences)
        {
            if (sequence.SequenceName == name)
            {
                // Setup thumb sequence
                if (side == Finger.THUMB || side == Finger.BOTH)
                {
                    _currentThumbSequence = sequence;
                    _isThumbPatternLoop = loop;
                    _currentThumbPatternIndex = 0;
                }
                // Setup index sequence
                if (side == Finger.INDEX || side == Finger.BOTH)
                {
                    _currentIndexSequence = sequence;
                    _isIndexPatternLoop = loop;
                    _currentIndexPatternIndex = 0;
                }
                return;
            }
        }
        Debug.LogError($"Pattern sequence {name} not found.");
    }

    public void StopPatternSequence()
    {
        StopPatternSequence(Finger.BOTH);
    }

    public void StopPatternSequence(Finger finger)
    {
        if (finger == Finger.THUMB || finger == Finger.BOTH)
        {
            ResetThumbSequence();
        }
        if (finger == Finger.INDEX || finger == Finger.BOTH)
        {
            ResetIndexSequence();
        }
    }

    /// <summary>
    /// Convert a BraillePinPatternSequence into a format that can be sent to the Braille pins
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    private EncodedBraillePatternSequence EncodePatternSequence(BraillePinPatternSequence sequence)
    {
        List<EncodedBraillePattern> encodedPatterns = new();

        foreach (var pattern in sequence.Sequence)
        {
            encodedPatterns.Add(EncodePattern(pattern));
        }

        return new EncodedBraillePatternSequence
        {
            SequenceName = sequence.name,
            Values = encodedPatterns
        };
    }

    /// <summary>
    /// Convert a BraillePattern into a format that can be sent to the Braille pins
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private EncodedBraillePattern EncodePattern(BraillePinPatternSequence.BraillePattern pattern)
    {
        // Create arrays to hold the values
        int[] v1 = new int[8];
        int[] v2 = new int[8];

        // Ensure rows can only be of length 4
        if (pattern.Row1.Length != 4 || pattern.Row2.Length != 4 || pattern.Row3.Length != 4 || pattern.Row4.Length != 4)
        {
            Debug.LogError("Invalid Pattern length");
            return null;
        }

        // Encode either 0 or 1
        for (int i = 0; i < 4; i++)
        {
            v1[i] = (pattern.Row1[i] == '0') ? 0 : 1;
            v1[i + 4] = (pattern.Row2[i] == '0') ? 0 : 1;
            v2[i] = (pattern.Row3[i] == '0') ? 0 : 1;
            v2[i + 4] = (pattern.Row4[i] == '0') ? 0 : 1;
        }

        return new EncodedBraillePattern
        {
            Value1 = (64 * v1[0] + 4 * v1[1] + 2 * v1[2] + v1[3] + 128 * v1[4] + 32 * v1[5] + 16 * v1[6] + 8 * v1[7]),
            Value2 = (64 * v2[0] + 4 * v2[1] + 2 * v2[2] + v2[3] + 128 * v2[4] + 32 * v2[5] + 16 * v2[6] + 8 * v2[7])
        };
    }

    private IEnumerator RunSequence()
    {
        WaitForSeconds interval = new(PatternDelay);

        while (true)
        {
            int t1 = 0;
            int t2 = 0;
            int i1 = 0;
            int i2 = 0;

            if (_currentThumbSequence != null) // Send pattern to thumb
            {
                t1 = _currentThumbSequence.Values[_currentThumbPatternIndex].Value1;
                t2 = _currentThumbSequence.Values[_currentThumbPatternIndex].Value2;
                _currentThumbPatternIndex++;
                if (!_isThumbPatternLoop && _currentThumbPatternIndex >= _currentThumbSequence.Values.Count)
                    ResetThumbSequence();
                else
                    _currentThumbPatternIndex %= _currentThumbSequence.Values.Count; // Wrap around if looping              
            }

            if (_currentIndexSequence != null) // Send pattern to indes
            {
                i1 = _currentIndexSequence.Values[_currentIndexPatternIndex].Value1;
                i2 = _currentIndexSequence.Values[_currentIndexPatternIndex].Value2;
                _currentIndexPatternIndex++;
                if (!_isIndexPatternLoop && _currentIndexPatternIndex >= _currentIndexSequence.Values.Count)
                    ResetIndexSequence();
                else
                    _currentIndexPatternIndex %= _currentIndexSequence.Values.Count; // Wrap around if looping              
            }
            InputDeviceManager.SendBrailleASCII(t1, t2, i1, i2);
            yield return interval; // buffer interval
        }
    }

    private void ResetThumbSequence()
    {
        _currentThumbSequence = null;
        _currentThumbPatternIndex = 0;
    }

    private void ResetIndexSequence()
    {
        _currentIndexSequence = null;
        _currentIndexPatternIndex = 0;
    }
}
