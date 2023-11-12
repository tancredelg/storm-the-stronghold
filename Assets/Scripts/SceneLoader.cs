using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SerializationManager.LoadSettings();
            QualitySettings.SetQualityLevel(SettingsData.Instance.QualityLevel);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene("GameplayScene");
        SerializationManager.NewSaveName();
        AudioManager.Instance.GetComponent<AudioSource>().Pause();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
        if (sceneName == "GameplayScene")
        {
            AudioManager.Instance.GetComponent<AudioSource>().Pause();
        }
        else if (!AudioManager.Instance.GetComponent<AudioSource>().isPlaying)
        {
            AudioManager.Instance.GetComponent<AudioSource>().Play();
        }
    }

    public void LoadSaveGame(string filePath)
    {
        StartCoroutine(LoadSaveGameCR(filePath));
    }

    public IEnumerator LoadSaveGameCR(string filePath)
    {
        LoadScene("GameplayScene");
        yield return null;
        var gameSession = FindObjectOfType<GameSession>();
        if (gameSession == null) yield break;
        SerializationManager.LoadSavedGame(filePath);
        gameSession.LoadSavedGame();
    }
}
