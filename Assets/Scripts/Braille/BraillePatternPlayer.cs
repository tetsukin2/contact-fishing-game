using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BraillePatternPlayer : MonoBehaviour
{
    public static BraillePatternPlayer Instance { get; private set; }
    public float PatternDelay = 0.2f; // Delay between patterns

    private class EncodedBraillePatternSequence
    {
        public string SequenceName; // Name of the sequence
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
    }

    public void PlayPatternSequence(string name)
    {
        PlayPatternSequence(name, false);
    }

    /// <summary>
    /// Play a braille pattern sequence
    /// </summary>
    /// <param name="name">Name of the sequence</param>
    /// <param name="loop">Whether to repeatedly play the sequence</param>
    public void PlayPatternSequence(string name, bool loop)
    {
        foreach (var sequence in _encodedBraillePatternSequences)
        {
            if (sequence.SequenceName == name)
            {
                // Stop any currently running pattern
                if (_patternCoroutine != null)
                {
                    StopCoroutine(_patternCoroutine);
                }
                // Start the new pattern
                _patternCoroutine = StartCoroutine(RunSequence(sequence, loop));
                return;
            }
        }
        Debug.LogError($"Pattern sequence {name} not found.");
    }

    public void StopPatternSequence()
    {
        if (_patternCoroutine != null)
        {
            StopCoroutine(_patternCoroutine);
            _patternCoroutine = null;
        }
        InputDeviceManager.SendBrailleASCII(0, 0);
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
        if (pattern.Row1.Length != 4 || pattern.Row2.Length != 4 || pattern.Row2.Length != 4 || pattern.Row2.Length != 4)
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

    private IEnumerator RunSequence(EncodedBraillePatternSequence sequence, bool loop)
    {
        WaitForSeconds interval = new(PatternDelay);
        int currentPatternIndex = 0;

        while (true)
        {
            InputDeviceManager.SendBrailleASCII(sequence.Values[currentPatternIndex].Value1, sequence.Values[currentPatternIndex].Value2);
            yield return interval;
            currentPatternIndex++;
            if (loop && currentPatternIndex >= sequence.Values.Count) break;
            currentPatternIndex %= sequence.Values.Count; // go back to start if exceed list size
        }
    }
}