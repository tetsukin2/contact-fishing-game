using System.Reflection;
using System;
using System.Collections;
using System.Text;

/// <summary>
/// Formats data to Firestore's expected JSON structure.
/// </summary>
public static class FirestoreFormatUtility
{
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
        bool first = true;

        foreach (var field in fields)
        {
            object value = field.GetValue(obj);
            if (value == null) continue;

            if (!first) sb.Append(",");
            sb.Append($"\"{ToCamelCase(field.Name)}\":");
            sb.Append(SerializeValue(value));
            first = false;
        }

        sb.Append("}");
        return sb.ToString();
    }

    // Serializes a value to a Firestore key-value pair format
    private static string SerializeValue(object value)
    {
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