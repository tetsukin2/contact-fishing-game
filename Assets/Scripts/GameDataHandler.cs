using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataHandler
{
    public static void SaveGameData(GameData data, string path, string fileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + path))
            Directory.CreateDirectory(Application.persistentDataPath + path);
        BinaryFormatter bf = new();
        FileStream file = File.Create(Application.persistentDataPath + $"{path}/{fileName}.dat");
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved!");
    }

    public static GameData GetGameData(string path, string fileName)
    {
        if (!File.Exists(Application.persistentDataPath + $"{path}/{fileName}.dat"))
        {
            Debug.Log("No save data found.");
            return new GameData();
        }

        BinaryFormatter bf = new();
        FileStream file = File.Open(Application.persistentDataPath + $"{path}/{fileName}", FileMode.Open);
        GameData data = (GameData)bf.Deserialize(file);
        Debug.Log("Game data loaded!");
        file.Close();
        return data;
    }

    public static void DeleteAllLevelData()
    {
        string path = Application.persistentDataPath;
        DirectoryInfo directory = new DirectoryInfo(path);
        directory.Delete(true);
        Directory.CreateDirectory(path);
    }

    public static string ConvertToTimeFormat(float timer)
    {
        return Mathf.FloorToInt(timer / 60).ToString() + ":" + Mathf.FloorToInt(timer % 60).ToString("D2");
    }
}

[Serializable]
public class GameData
{
    public float highScore = float.MaxValue;
}
