using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnitsHandler : MonoBehaviour
{
    public List<GameObject> ActiveUnits { get; private set; }

    [SerializeField] private Image _progressBar;
    [SerializeField] private GameObject _meleeEnemy, _rangedEnemy;
    private PlayerController _playerController;
    private DungeonGenerator _dungeonGenerator;
    private List<Room> _enemyRoomsSpawned;
    private Room _roomAtPlayer;
    private bool _spawning;
    private readonly WaitForSeconds _timeBetweenSpawns = new WaitForSeconds(0.25f);

    private void Awake()
    {
        _dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        _playerController = FindObjectOfType<PlayerController>();
        _progressBar.fillAmount = 0;
    }

    private void Update()
    {
        if (_spawning || _dungeonGenerator == null || _playerController == null) return;
        _roomAtPlayer = _dungeonGenerator.Rooms.FirstOrDefault(r => !_enemyRoomsSpawned.Contains(r) && r.Contains(_playerController.transform.position));
        if (_roomAtPlayer == null) return;
        if (_enemyRoomsSpawned.Count < _dungeonGenerator.Rooms.Count)
        {
            StartCoroutine(SpawnEnemiesCR());
        }
    }

    public void Init()
    {
        _enemyRoomsSpawned = new List<Room>();
        ActiveUnits = new List<GameObject>();
    }

    public void DespawnActiveUnits()
    {
        if (ActiveUnits == null) return;
        while (ActiveUnits.Count > 0)
        {
            Destroy(ActiveUnits[0]);
            ActiveUnits.RemoveAt(0);
        }
    }

    private IEnumerator SpawnEnemiesCR()
    {
        _spawning = true;
        var spawnPositions = _roomAtPlayer.GetTilesInRoom();
        int spawnedEnemies = 0;
        while (spawnedEnemies < _roomAtPlayer.EnemySpawnCount)
        {
            var spawnPos = spawnPositions[Random.Range(0, spawnPositions.Count - 1)];
            if (Vector2.Distance(_playerController.transform.position, spawnPos) < _dungeonGenerator.RoomSize * 0.5f) continue;
            
            if (_roomAtPlayer.RoomType == RoomType.Melee)
            {
                ActiveUnits.Add(Instantiate(_meleeEnemy, spawnPos, Quaternion.identity, transform));
            }
            else
            {
                ActiveUnits.Add(Instantiate(_rangedEnemy, spawnPos, Quaternion.identity, transform));
            }
            spawnedEnemies++;
            yield return _timeBetweenSpawns;
        }
        
        _enemyRoomsSpawned.Add(_roomAtPlayer);
        _spawning = false;
        _progressBar.fillAmount = (_enemyRoomsSpawned.Count + 2f) / _dungeonGenerator.Rooms.Count;
    }
}