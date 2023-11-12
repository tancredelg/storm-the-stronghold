using UnityEngine;

[System.Serializable]
public class GameData
{
    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameData();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    public DungeonData Dungeon = new DungeonData();
    public PlayerData Player = new PlayerData();
    public PlayerLevelData PlayerLevel = new PlayerLevelData();

    public override string ToString()
    {
        return $"{Dungeon}\n{Player}\n{PlayerLevel}";
    }
}
