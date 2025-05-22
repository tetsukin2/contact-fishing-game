using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BraillePatternPlayer : SingletonPersistent<BraillePatternPlayer>
{
    public enum Finger
    {
        THUMB = 0,
        INDEX = 1,
        BOTH
    }

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

    public UnityEvent<Finger> PatternEnded = new();

    private List<EncodedBraillePatternSequence> _encodedThumbBraillePatternSequences = new();
    private List<EncodedBraillePatternSequence> _encodedIndexBraillePatternSequences = new();

    private EncodedBraillePatternSequence _currentIndexSequence;
    private int _currentIndexPatternIndex = 0;
    private bool _isIndexPatternLoop = false;

    private EncodedBraillePatternSequence _currentThumbSequence;
    private int _currentThumbPatternIndex = 0;
    private bool _isThumbPatternLoop = false;

    private Coroutine _patternCoroutine;

    protected override void OnAwake()
    {
        // Register all braille pattern sequences from resources
        List<BraillePinPatternSequence> rawBraillePatternSequences =
            Resources.LoadAll<BraillePinPatternSequence>("BraillePatternSequences").ToList();

        // Encode to readable format
        foreach (var sequence in rawBraillePatternSequences)
        {
            _encodedThumbBraillePatternSequences.Add(EncodePatternSequence(sequence, Finger.THUMB));
            _encodedIndexBraillePatternSequences.Add(EncodePatternSequence(sequence, Finger.INDEX));
        }
    }

    public void PlayPatternSequence(string name, bool loop)
    {
        PlayPatternSequence(name, Finger.BOTH, loop);
    }

    public void PlayPatternSequence(string name, Finger side)
    {
        PlayPatternSequence(name, side, false);
    }

    /// <summary>
    /// Start playing a sequence
    /// </summary>
    /// <param name="name"></param>
    /// <param name="side"></param>
    /// <param name="loop"></param>
    public void PlayPatternSequence(string name, Finger side, bool loop)
    {
        bool found = false;
        if (side == Finger.THUMB || side == Finger.BOTH)
        {
            // Search via loop
            foreach (var sequence in _encodedThumbBraillePatternSequences)
            {
                if (sequence.SequenceName == name)
                {
                    _currentThumbSequence = sequence;
                    _isThumbPatternLoop = loop;
                    _currentThumbPatternIndex = 0;
                    found = true;
                    break;
                }
            }
        }
        
        // Not a second check cuz I think both should be identical
        if (side == Finger.INDEX || side == Finger.BOTH)
        {
            // Search via loop
            foreach (var sequence in _encodedIndexBraillePatternSequences)
            {
                if (sequence.SequenceName == name)
                {
                    _currentIndexSequence = sequence;
                    _isIndexPatternLoop = loop;
                    _currentIndexPatternIndex = 0;
                    found = true;
                    break;
                }
            }
        }

        // Nothing found
        if (!found)
        {
            Debug.LogError($"Pattern sequence {name} not found.");
            return;
        }

        if ((_currentIndexSequence != null || _currentThumbSequence != null) && _patternCoroutine == null)
        {
            _patternCoroutine = StartCoroutine(RunSequence());
        }
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
        SequenceStopCheck();
    }

    /// <summary>
    /// Stop playing sequences if both are null
    /// </summary>
    private void SequenceStopCheck()
    {
        if (_currentIndexSequence == null && _currentThumbSequence == null && _patternCoroutine != null)
        {
            StopCoroutine(_patternCoroutine);
            _patternCoroutine = null;
        }
    }

    /// <summary>
    /// Convert a BraillePinPatternSequence into a format that can be sent to the Braille pins
    /// </summary>
    /// <param name="sequence"></param>
    /// <returns></returns>
    private EncodedBraillePatternSequence EncodePatternSequence(BraillePinPatternSequence sequence, Finger finger)
    {
        if (finger == Finger.BOTH)
        {
            Debug.LogError("EncodePattern must be called with either THUMB or INDEX");
        }

        List<EncodedBraillePattern> encodedPatterns = new();

        foreach (var pattern in sequence.Sequence)
        {
            if (finger == Finger.THUMB)
                encodedPatterns.Add(EncodePattern(pattern, Finger.THUMB));
            else
                encodedPatterns.Add(EncodePattern(pattern, Finger.INDEX));
        }

        return new EncodedBraillePatternSequence
        {
            SequenceName = sequence.name,
            Values = encodedPatterns
        };
    }

    /// <summary>
    /// Convert a BraillePattern into a format that can be sent to the thumb Braille pins
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private EncodedBraillePattern EncodePattern(BraillePinPatternSequence.BraillePattern pattern, Finger finger)
    {
        if (finger == Finger.BOTH)
        {
            Debug.LogError("EncodePattern must be called with either THUMB or INDEX");
        }

        // Create arrays to hold the values
        int[,] v = new int[4,4];

        // Ensure rows can only be of length 4
        if (pattern.Row1.Length != 4 || pattern.Row2.Length != 4 || pattern.Row3.Length != 4 || pattern.Row4.Length != 4)
        {
            Debug.LogError("Invalid Pattern length");
            return null;
        }

        // Encode either 0 or 1
        for (int i = 0; i < 4; i++)
        {
            v[0,i] = (pattern.Row1[i] == '0') ? 0 : 1;
            v[1,i] = (pattern.Row2[i] == '0') ? 0 : 1;
            v[2,i] = (pattern.Row3[i] == '0') ? 0 : 1;
            v[3,i] = (pattern.Row4[i] == '0') ? 0 : 1;
        }

        // Mappings
        if (finger == Finger.THUMB)
            return new EncodedBraillePattern
            {
                Value1 = 1 * v[0,0] + 8 * v[0,1] 
                    + 2 * v[1,0] + 16 * v[1,1] 
                    + 4 * v[2,0] + 32 * v[2,1] 
                    + 64 * v[3,0] + 128 * v[3,1],
                Value2 = 1 * v[0,2] + 8 * v[0,3] 
                    + 2 * v[1,2] + 16 * v[1,3] 
                    + 4 * v[2,2] + 32 * v[2,3] 
                    + 64 * v[3,2] + 128 * v[3,3]
            };
        else 
            return new EncodedBraillePattern
            {
                Value1 = 64 * v[0, 0] + 4 * v[0, 1] + 2 * v[0, 2] + v[0,3] 
                    + 128 * v[1, 0] + 32 * v[1, 1] + 16 * v[1, 2] + 8 * v[1, 3],
                Value2 = 64 * v[2, 0] + 4 * v[2, 1] + 2 * v[2, 2] + v[2, 3] 
                    + 128 * v[3, 0] + 32 * v[3, 1] + 16 * v[3, 2] + 8 * v[3, 3]
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
            InputDeviceManager.Instance.SendBrailleASCII(t1, t2, i1, i2);
            yield return interval; // buffer interval
            SequenceStopCheck();
        }
    }

    private void ResetThumbSequence()
    {
        _currentThumbSequence = null;
        _currentThumbPatternIndex = 0;
        PatternEnded.Invoke(Finger.THUMB);
    }

    private void ResetIndexSequence()
    {
        _currentIndexSequence = null;
        _currentIndexPatternIndex = 0;
        PatternEnded.Invoke(Finger.INDEX);
    }
}
