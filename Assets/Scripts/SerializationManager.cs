using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SerializationManager
{
    public static readonly string SettingsPath = Application.persistentDataPath + "/settings/";
    public static readonly string SavesPath = Application.persistentDataPath + "/saves/";
    public static string SaveName = "AutoSave";

    public static void SaveGame()
    {
        //var formatter = new BinaryFormatter();
        if (!Directory.Exists(SavesPath))
        {
            Directory.CreateDirectory(SavesPath);
        }

        string filePath = SavesPath + SaveName + ".json";
        Debug.Log("Game saved at: " + SaveName);

        //FileStream file = File.Create(path);
        //formatter.Serialize(file, saveData);

        string json = JsonUtility.ToJson(GameData.Instance);
        File.WriteAllText(filePath, json);

        //file.Close();
    }

    public static List<string> GetSaveGameFiles()
    {
        var filePaths = new List<string>();
        if (!Directory.Exists(SavesPath))
        {
            return filePaths;
        }

        foreach (var filePath in Directory.GetFiles(SavesPath))
        {
            filePaths.Add(filePath);
        }
        return filePaths;
    }

    public static void NewSaveName()
    {
        if (!Directory.Exists(SavesPath))
        {
            Directory.CreateDirectory(SavesPath);
        }
        SaveName = "Save_" + (Directory.GetFiles(SavesPath).Length);
    }

    public static bool LoadSavedGame(string path)
    {
        if (!File.Exists(path)) return false;

        //var formatter = new BinaryFormatter();
        //FileStream file = File.Open(path, FileMode.Open);

        try
        {
            string json = File.ReadAllText(path);
            var gameData = JsonUtility.FromJson<GameData>(json);
            GameData.Instance = gameData;
            //object save = formatter.Deserialize(file);
            //file.Close();
            SaveName = path.Replace(SavesPath, "").Replace(".json", "");
            Debug.Log("Loaded saveData from: " + SaveName);
            return true;
        }
        catch (System.Exception)
        {
            Debug.LogErrorFormat("Failed to load file a {0}", path);
            //file.Close();
            return false;
        }
    }

    public static void SaveSettings()
    {
        if (!Directory.Exists(SettingsPath))
        {
            Directory.CreateDirectory(SettingsPath);
        }

        File.WriteAllText(SettingsPath + "Settings.json", JsonUtility.ToJson(SettingsData.Instance));
        Debug.Log("Settings saved at:\n" + SettingsPath + "Settings.json");
    }

    public static bool LoadSettings()
    {
        if (!Directory.Exists(SettingsPath) || Directory.GetFiles(SettingsPath).Length == 0) return false;

        try
        {
            string json = File.ReadAllText(SettingsPath + "Settings.json");
            var settingsData = JsonUtility.FromJson<SettingsData>(json);
            SettingsData.Instance = settingsData;
            Debug.Log("Settings loaded successfully");
            return true;
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to load settings");
            return false;
        }
    }
}
