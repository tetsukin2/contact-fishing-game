using System;
using System.Collections.Generic;

[System.Serializable]
public class FirestoreDocumentWrapper
{
    public Dictionary<string, FirestoreField> fields;
}

[System.Serializable]
public class FirestoreField
{
    public string stringValue;
    public string integerValue;
    public string doubleValue;
    public string booleanValue;
    public Dictionary<string, FirestoreField> mapValue;
    public ArrayValue arrayValue; 

    public string GetValueAsString()
    {
        if (!string.IsNullOrEmpty(stringValue)) return stringValue;
        if (!string.IsNullOrEmpty(integerValue)) return integerValue;
        if (!string.IsNullOrEmpty(doubleValue)) return doubleValue;
        if (!string.IsNullOrEmpty(booleanValue)) return booleanValue;
        return null;
    }
}

[System.Serializable]
public class ArrayValue
{
    public List<FirestoreField> values;
}
