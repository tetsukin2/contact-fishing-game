using System.Reflection;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Formats data to Firestore's expected JSON structure.
/// </summary>
public static class FirestoreFormatUtility
{
    public static Dictionary<string, object> Unwrap(FirestoreDocumentWrapper wrapper, string fieldKey = null)
    {
        if (wrapper == null || wrapper.fields == null)
        {
            Debug.LogError("Failed to deserialize Firestore document or fields are missing.");
            return null;
        }

        if (!string.IsNullOrEmpty(fieldKey))
        {
            if (!wrapper.fields.TryGetValue(fieldKey, out var subField) || subField.mapValue == null)
            {
                Debug.LogWarning($"Field '{fieldKey}' not found in Firestore document.");
                return null;
            }

            return ParseMap(subField.mapValue);
        }

        return ParseMap(new Dictionary<string, FirestoreField> { { "root", new FirestoreField { mapValue = wrapper.fields } } })["root"] as Dictionary<string, object>;
    }

    public static string WrapAsFieldsOnly<T>(T dataClass)
{
    return SerializeObject(dataClass); // this returns just the fields dictionary
}


    private static Dictionary<string, object> ParseMap(Dictionary<string, FirestoreField> map)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in map)
        {
            var field = kvp.Value;

            if (field.stringValue != null) result[kvp.Key] = field.stringValue;
            else if (field.integerValue != null && long.TryParse(field.integerValue, out var l)) result[kvp.Key] = l;
            else if (field.doubleValue != null && double.TryParse(field.doubleValue, out var d)) result[kvp.Key] = d;
            else if (field.booleanValue != null && bool.TryParse(field.booleanValue, out var b)) result[kvp.Key] = b;
            else if (field.mapValue != null) result[kvp.Key] = ParseMap(field.mapValue);
            else result[kvp.Key] = null;
        }

        return result;
    }

    private static object GetPrimitiveValue(FirestoreField field)
    {
        if (!string.IsNullOrEmpty(field.stringValue)) return field.stringValue;
        if (!string.IsNullOrEmpty(field.integerValue)) return int.TryParse(field.integerValue, out int i) ? i : field.integerValue;
        if (!string.IsNullOrEmpty(field.doubleValue)) return double.TryParse(field.doubleValue, out double d) ? d : field.doubleValue;
        if (!string.IsNullOrEmpty(field.booleanValue)) return bool.TryParse(field.booleanValue, out bool b) ? b : field.booleanValue;

        if (field.mapValue != null)
        {
            var map = new Dictionary<string, object>();
            foreach (var kvp in field.mapValue)
            {
                map[kvp.Key] = GetPrimitiveValue(kvp.Value);
            }
            return map;
        }

        return null;
    }

    public static string WrapClass<T>(T dataClass)
    {
        StringBuilder sb = new();
        sb.Append("{\"fields\":");
        sb.Append(SerializeObject(dataClass));
        sb.Append("}");
        return sb.ToString();
    }

    // Serializes passed object to a Firestore-compatible JSON format. Fields become nested key-value pairs.
    // For example, {"foo":"bar"} becomes {"foo":{"stringValue":"bar"}}
    private static string SerializeObject(object obj)
    {
        StringBuilder sb = new();
        sb.Append("{");

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        bool first = true;

        // Serialize fields
        foreach (var field in fields)
        {
            object value = field.GetValue(obj);

            if (!first) sb.Append(",");
            sb.Append($"\"{ToCamelCase(field.Name)}\":");
            sb.Append(value == null ? "{\"nullValue\":null}" : SerializeValue(value));
            first = false;
        }

        // Serialize properties
        foreach (var prop in properties)
        {
            object value = prop.GetValue(obj);

            if (!first) sb.Append(",");
            sb.Append($"\"{ToCamelCase(prop.Name)}\":");
            sb.Append(value == null ? "{\"nullValue\":null}" : SerializeValue(value));
            first = false;
        }

        sb.Append("}");
        return sb.ToString();
    }


    // Serializes a value to a Firestore key-value pair format
    private static string SerializeValue(object value)
    {
        if (value == null)
            return "null";

        var type = value.GetType();

        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            return SerializeValue(underlyingValue);
        }

        switch (value)
        {
            case string s:
                return $"{{\"stringValue\":\"{EscapeString(s)}\"}}";
            case int i:
                return $"{{\"integerValue\":\"{i}\"}}";
            case long l:
                return $"{{\"integerValue\":\"{l}\"}}";
            case float f:
                return $"{{\"doubleValue\":{f.ToString("R")}}}";
            case double d:
                return $"{{\"doubleValue\":{d.ToString("R")}}}";
            case bool b:
                return $"{{\"booleanValue\":{(b ? "true" : "false")}}}";
            case DateTime dt:
                return $"{{\"timestampValue\":\"{dt.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.fffZ}\"}}";
            case Enum e:
                return $"{{\"stringValue\":\"{EscapeString(e.ToString())}\"}}";
            case IDictionary dict:
                return SerializeDictionary(dict);
            case IList list:
                return SerializeArray(list);
            default:
                if (value != null && value.GetType().IsClass)
                    return $"{{\"mapValue\":{{\"fields\":{SerializeObject(value)}}}}}";
                return "null";
        }
    }

    // Serializes an IList (array) to Firestore's expected format
    private static string SerializeArray(IList list)
    {
        StringBuilder sb = new();
        sb.Append("{\"arrayValue\":{\"values\":[");
        bool first = true;
        foreach (var item in list)
        {
            if (!first) sb.Append(",");
            sb.Append(SerializeValue(item));
            first = false;
        }
        sb.Append("]}}");
        return sb.ToString();
    }

    // Serializes an IDictionary (map) to Firestore's expected format
    private static string SerializeDictionary(IDictionary dict)
    {
        StringBuilder sb = new();
        sb.Append("{\"mapValue\":{\"fields\":{");
        bool first = true;
        foreach (DictionaryEntry entry in dict)
        {
            if (!(entry.Key is string key)) continue;
            if (!first) sb.Append(",");
            sb.Append($"\"{ToCamelCase(key)}\":");
            sb.Append(SerializeValue(entry.Value));
            first = false;
        }
        sb.Append("}}}");
        return sb.ToString();
    }

    // Just want this for convention sake, converts PascalCase to camelCase
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;
        if (name.Length == 1)
            return name.ToLower();
        return char.ToLower(name[0]) + name.Substring(1);
    }

    // Escapes special characters in strings before adding them to JSON
    private static string EscapeString(string input)
    {
        // should maybe include more escape characters? idk
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}