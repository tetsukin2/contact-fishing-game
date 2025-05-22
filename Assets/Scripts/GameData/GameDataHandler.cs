using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameDataHandler : SingletonPersistent<GameDataHandler>
{
    /// <summary>
    /// Current game data worked on by the game.
    /// </summary>
    public static GameData CurrentGameData { get; private set; }

    // The sole reason this is a monobehaviour
    protected override void OnAwake()
    {
        // Load game data on startup
        // TODO: Custom filenames(?) idk
        CurrentGameData = LoadGameData();
    }

    /// <summary>
    /// Saves current data to data/game_data.dat
    /// </summary>
    // Using data folder in case we want to save other data?
    public static void SaveGameData()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/");
        BinaryFormatter bf = new();
        FileStream file = File.Create(Application.persistentDataPath + "/data.dat");
        bf.Serialize(file, CurrentGameData);
        file.Close();
        Debug.Log("Game data saved!");
    }

    private static GameData LoadGameData()
    {
        if (!File.Exists(Application.persistentDataPath + "/data.dat"))
        {
            Debug.Log("No save data found.");
            return new GameData();
        }

        BinaryFormatter bf = new();
        FileStream file = File.Open(Application.persistentDataPath + $"/data.dat", FileMode.Open);
        GameData data = (GameData)bf.Deserialize(file);
        Debug.Log("Game data loaded!");
        file.Close();
        return data;
    }

    public static void DeleteGameData()
    {
        string filePath = Application.persistentDataPath + "/data.dat";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Game data file deleted.");
        }
        else
        {
            Debug.Log("No game data file found to delete.");
        }

        CurrentGameData = LoadGameData(); // Reload the game data after deletion  
    }

    // should this be here or in a different proper class that isn't data cuz bruh
    public static string ConvertToTimeFormat(float timer)
    {
        return Mathf.FloorToInt(timer / 60).ToString() + ":" + Mathf.FloorToInt(timer % 60).ToString("D2");
    }
}
