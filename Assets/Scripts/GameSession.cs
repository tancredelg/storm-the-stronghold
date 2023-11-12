using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public bool IsPaused { get; private set; }
    public bool SkillTreeIsOpen { get; private set; }

    [SerializeField] private GameObject _pauseScreen, _gameOverScreen;
    [SerializeField] private AudioSource _musicSource, _levelCompleteSound;
    [SerializeField] private TextMeshProUGUI _statsValuesText;
    private DungeonGenerator _dungeonGenerator;
    private PlayerController _playerController;
    private PlayerLevelManager _playerLevelManager;
    private EnemyUnitsHandler _enemyUnitsHandler;
    private SkillTree _skillTree;

    private void Awake()
    {
        _dungeonGenerator = GetComponentInChildren<DungeonGenerator>();
        _playerController = FindObjectOfType<PlayerController>();
        _playerLevelManager = FindObjectOfType<PlayerLevelManager>();
        _enemyUnitsHandler = FindObjectOfType<EnemyUnitsHandler>();
        _skillTree = FindObjectOfType<SkillTree>();

        _dungeonGenerator.NewDungeon();
        _enemyUnitsHandler.Init();
        _playerController.transform.position = _dungeonGenerator.StartRoom.CenterAsWorldPos;
        Resume();
    }

    public void LoadSavedGame()
    {
        if (GameData.Instance.Player.Health == 0)
        {
            ProcessGameOver();
            return;
        }
        _dungeonGenerator.NewDungeon();

        _dungeonGenerator.Load();
        _playerController.Load();
        _playerLevelManager.Load();

        _enemyUnitsHandler.DespawnActiveUnits();
        _enemyUnitsHandler.Init();
    }

    public void LoadNextLevel()
    {
        _levelCompleteSound.Play();
        ClearDroppedWeapons();
        _enemyUnitsHandler.DespawnActiveUnits();
        _dungeonGenerator.NewDungeon();
        _enemyUnitsHandler.Init();
        _playerController.transform.position = _dungeonGenerator.StartRoom.CenterAsWorldPos;

        foreach (var savableData in GetComponentsInChildren<ISavableData>())
        {
            savableData.Save();
        }

        SerializationManager.SaveGame();
    }

    public void Pause()
    {
        IsPaused = true;
        _musicSource.Pause();
        Time.timeScale = 0; 
        _pauseScreen.GetComponent<Canvas>().enabled = true;
    }

    public void Resume()
    {
        _pauseScreen.GetComponent<Canvas>().enabled = false;
        Time.timeScale = 1;
        _musicSource.UnPause();
        IsPaused = false;
    }

    public void ProcessGameOver()
    {
        foreach (var savableData in GetComponentsInChildren<ISavableData>())
        {
            savableData.Save();
        }

        SerializationManager.SaveGame();

        Time.timeScale = 0;
        IsPaused = true;
        _musicSource.Pause();
        _gameOverScreen.SetActive(true);
        _statsValuesText.text = $"{GameData.Instance.Dungeon.Level}\n" +
            $"{GameData.Instance.PlayerLevel.LevelSystemData[0]}\n" +
            $"{GameData.Instance.PlayerLevel.LevelSystemData[1]}\n" +
            $"{GameData.Instance.Player.Kills}";
        _gameOverScreen.GetComponent<AudioSource>().Play();
    }

    public void OpenSkillTree()
    {
        _pauseScreen.GetComponent<Canvas>().enabled = false;
        IsPaused = true;
        _musicSource.Pause();
        SkillTreeIsOpen = true;
        _skillTree.Open();
    }

    public void CloseSkillTree()
    {
        _skillTree.Close();
        _musicSource.UnPause();
        IsPaused = false;
        SkillTreeIsOpen = false;
    }

    private void ClearDroppedWeapons()
    {
        var droppedWeaponsObject = GameObject.Find("DroppedWeapons");
        if (droppedWeaponsObject == null)
        {
            print("No DroppedWeapons GameObject found");
            return;
        }
        foreach (Transform child in droppedWeaponsObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
}