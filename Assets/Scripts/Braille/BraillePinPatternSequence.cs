using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Braille Pattern Sequence")]
public class BraillePinPatternSequence : ScriptableObject
{
    public string SequenceName;
    /// <summary>
    /// 0 or 1 in a row of 4
    /// 
    /// 1001
    /// 0000
    /// 1001
    /// 0110
    /// </summary>
    [System.Serializable]
    public struct BraillePattern
    {
        public string Row1;
        public string Row2;
        public string Row3;
        public string Row4;
    }

    public List<BraillePattern> Sequence;
}
