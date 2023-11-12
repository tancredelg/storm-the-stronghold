using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DungeonGenerator : MonoBehaviour, ISavableData
{
    public int Level { get; private set; }
    public int Size { get; private set; }
    public TileType[,] GridArr { get; private set; }
    public List<Room> Rooms { get; private set; }
    public Room StartRoom => Rooms[0];
    public Room EndRoom => Rooms.Last();
    public int RoomSize => _roomSize;

    [SerializeField][Range(2, 100)] private int _numOfRooms;
    [SerializeField][Range(7, 13)] private int _roomSize;
    [SerializeField] private bool _useFixedRoomSize;
    [SerializeField] private GameObject _wallSidePrefab, _wallTopPrefab, _floorPrefab, _portalPrefab, _chestPrefab;
    [SerializeField] private Sprite _floor1, _floor2, _floor3;
    private GameObject[,] _tilePrefabsArr;
    private int _attemptsLeft;

    private const int MIN_DIST_BETWEEN_ROOMS = 2;

    public void NewDungeon()
    {
        Level++;
        _numOfRooms = 3 * Level;
        Init();
        SpawnRooms(_numOfRooms);
        SetStartEndRooms();

        for (int i = 0; i < Rooms.Count; i++)
        {
            foreach (var pos in Rooms[i].Rect.allPositionsWithin)
            {
                GridArr[pos.x, pos.y] = TileType.Floor;
            }
        }

        var connectionTiles = ConnectRooms();
        foreach (var tilePos in connectionTiles)
        {
            GridArr[tilePos.x, tilePos.y] = TileType.Floor;
        }

        ConnectRooms().ForEach(pos => GridArr[pos.x, pos.y] = TileType.Floor);

        Rooms[0].SetRoomType(RoomType.Start);
        GridArr[Rooms[0].ChestPosition.x, Rooms[0].ChestPosition.y] = TileType.Chest;

        for (int i = 1; i < Rooms.Count; i++)
        {
            if (i == Rooms.Count - 1)
            {
                Rooms[i].SetRoomType(RoomType.End);
                GridArr[Rooms[i].ChestPosition.x, Rooms[i].ChestPosition.y] = TileType.Portal;
            }
            else
            {
                Rooms[i].SetRoomType(Random.Range(0, 2) == 0 ? RoomType.Melee : RoomType.Projectile);
                GridArr[Rooms[i].ChestPosition.x, Rooms[i].ChestPosition.y] = TileType.Chest;
            }
        }

        CleanUpTunnels();
        SetupTiles();
    }

    private void Init()
    {
        Rooms = new List<Room>();
        // Magic formula which scales the map surprisingly accurately (for roomSize between 8 and 16)
        Size = Mathf.RoundToInt(Mathf.Sqrt(_numOfRooms) * 1.5f * _roomSize) + 20;

        GridArr = new TileType[Size, Size];
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                GridArr[x, y] = TileType.Wall;

            }
        }
    }

    public void Load()
    {
        Level = GameData.Instance.Dungeon.Level;
        Size = GameData.Instance.Dungeon.Size;

        GridArr = new TileType[Size, Size];
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                GridArr[x, y] = GameData.Instance.Dungeon.GridArr1D[x * Size + y];
            }
        }

        Rooms = new List<Room>(GameData.Instance.Dungeon.Rooms);

        SetupTiles();
    }

    public void Save()
    {
        GameData.Instance.Dungeon.Level = Level;
        GameData.Instance.Dungeon.Size = Size;

        var gridArr1D = new TileType[Size * Size];
        int i = 0;
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                gridArr1D[i++] = GridArr[x, y];
            }
        }

        GameData.Instance.Dungeon.GridArr1D = gridArr1D;

        GameData.Instance.Dungeon.Rooms = Rooms;
    }

    public void LoadUnloadTiles(Vector3 playerPos)
    {
        float tileLoadDistance = (Camera.main.orthographicSize * Camera.main.aspect) + 2;
        for (int x = 2; x < Size - 2; x++)
        {
            for (int y = 2; y < Size - 2; y++)
            {
                if (_tilePrefabsArr[x, y] == null) continue;
                if (Vector2.Distance(playerPos, new Vector2(x, y)) < tileLoadDistance)
                {
                    if (!_tilePrefabsArr[x, y].activeInHierarchy)
                    {
                        _tilePrefabsArr[x, y].SetActive(true);
                    }
                }
                else
                {
                    if (_tilePrefabsArr[x, y].activeInHierarchy)
                    {
                        _tilePrefabsArr[x, y].SetActive(false);
                    }
                }
            }
        }
    }

    private void SpawnRooms(int numOfRooms)
    {
        // Buffer breaks loop when spawning is too hard - scales with numOfRooms
        _attemptsLeft = (int)(Mathf.Pow(numOfRooms, 1.5f) * 50);

        RectInt newRect;
        int x, y, roomWidth, roomHeight;

        // Spawn new rooms until none left or all attemps spent
        while (numOfRooms > 0 && _attemptsLeft > 0)
        {
            _attemptsLeft--;

            if (_useFixedRoomSize)
            {
                roomWidth = _roomSize;
                roomHeight = _roomSize;
            }
            else
            {
                int lowerBound = (_roomSize - (_roomSize / 3)) / 2;
                int upperBound = (_roomSize + (_roomSize / 3)) / 2;
                roomWidth = Random.Range(lowerBound, upperBound) * 2;
                roomHeight = Random.Range(lowerBound, upperBound) * 2;
            }

            x = Random.Range(4, Size - roomWidth - 4);
            y = Random.Range(4, Size - roomHeight - 4);
            newRect = new RectInt(x, y, roomWidth, roomHeight);

            if (Rooms.Count > 0 && !ValidateRoomPosition(newRect)) continue;

            Rooms.Add(new Room(newRect));
            numOfRooms--;
        }

        // Try again if not all rooms could be spawned, rare but possible
        if (Rooms.Count < _numOfRooms)
        {
            Rooms.Clear();
            SpawnRooms(_numOfRooms);
        }
    }

    private bool ValidateRoomPosition(RectInt newRoomRect)
    {
        foreach (var r in Rooms)
        {
            if (r.Rect.Overlaps(newRoomRect)
                || Mathf.Abs(newRoomRect.xMin - r.Rect.xMax) < MIN_DIST_BETWEEN_ROOMS
                || Mathf.Abs(newRoomRect.yMin - r.Rect.yMax) < MIN_DIST_BETWEEN_ROOMS
                || Mathf.Abs(newRoomRect.xMax - r.Rect.xMin) < MIN_DIST_BETWEEN_ROOMS
                || Mathf.Abs(newRoomRect.yMax - r.Rect.yMin) < MIN_DIST_BETWEEN_ROOMS) return false;
        }
        return Rooms.Any(r => Vector2.Distance(newRoomRect.center, r.Center) < 1.5f * _roomSize);
    }

    private void SetStartEndRooms()
    {
        var startRoom = Rooms[0];
        float distToStartRoom = 0;
        for (int i = 0; i < Rooms.Count; i++)
        {
            for (int j = 0; j < Rooms.Count; j++)
            {
                if (Rooms[i].Equals(Rooms[j])) continue;

                if (Rooms[i].GetDistanceToRoom(Rooms[j]) > distToStartRoom)
                {
                    startRoom = Rooms[i];
                    distToStartRoom = Rooms[i].GetDistanceToRoom(Rooms[j]);
                }
            }
        }
        Rooms.Sort((r1, r2) => startRoom.GetDistanceToRoom(r1).CompareTo(startRoom.GetDistanceToRoom(r2)));
    }

    private List<Vector2Int> ConnectRooms()
    {
        // List of room centers is more convenient to work with
        var roomCenters = Rooms.Select(r => r.Center).ToList();
        var corridorTiles = new HashSet<Vector2Int>();
        Vector2Int currentRoomCenter = roomCenters[0];

        foreach (var room in Rooms)
        {
            Rooms.Where(r => room.GetDistanceToRoom(r) < 2 * _roomSize)
                .ToList()
                .ForEach(r => room.TryConnectRoom(r));

            foreach (var connectedRoom in room.ConnectedRooms)
            {
                corridorTiles.UnionWith(CreateTunnelBetween(room.Center, connectedRoom.Center));
            }
        }
        return corridorTiles.ToList();
    }

    private List<Vector2Int> CreateTunnelBetween(Vector2Int currentPos, Vector2Int targetPos)
    {
        var newCorridor = new HashSet<Vector2Int>();

        while (currentPos.x != targetPos.x)
        {
            currentPos += (currentPos.x < targetPos.x) ? Vector2Int.right : Vector2Int.left;
            newCorridor.Add(currentPos);
        }
        while (currentPos.y != targetPos.y)
        {
            currentPos += (currentPos.y < targetPos.y) ? Vector2Int.up : Vector2Int.down;
            newCorridor.Add(currentPos);
        }
        return newCorridor.ToList();
    }

    private void CleanUpTunnels()
    {
        for (int x = 2; x < Size - 2; x++)
        {
            for (int y = 2; y < Size - 2; y++)
            {
                if (GridArr[x, y] != TileType.Wall) continue;

                // Remove any wall that has 3+ floor neighbours
                if ((GridArr[x + 1, y] == TileType.Floor && GridArr[x - 1, y] == TileType.Floor && (GridArr[x, y + 1] == TileType.Floor || GridArr[x, y - 1] == TileType.Floor))
                    || (GridArr[x, y + 1] == TileType.Floor && GridArr[x, y - 1] == TileType.Floor && (GridArr[x + 1, y] == TileType.Floor || GridArr[x - 1, y] == TileType.Floor)))
                {
                    GridArr[x, y] = TileType.Floor;
                }
            }
        }
    }

    private void SetupTiles()
    {
        foreach (Transform child in gameObject.transform)
        {
            foreach (Transform tile in child.transform)
            {
                Destroy(tile.gameObject);
            }
        }

        _tilePrefabsArr = new GameObject[Size, Size];
        for (int x = 1; x < Size - 1; x++)
        {
            for (int y = 1; y < Size - 1; y++)
            {
                if (GridArr[x, y] == TileType.Chest)
                {
                    _tilePrefabsArr[x, y] = Instantiate(_chestPrefab, new Vector3(x, y, 0), Quaternion.identity, transform.GetChild(2));
                    _tilePrefabsArr[x, y].GetComponent<Chest>().GenerateLoot(Rooms.FirstOrDefault(r => r.Contains(x, y)).RoomType);
                }
                else if (GridArr[x, y] == TileType.Portal)
                {
                    _tilePrefabsArr[x, y] = Instantiate(_portalPrefab, new Vector3(x, y, 0), Quaternion.identity, transform.GetChild(2));
                }
                else if (GridArr[x, y] == TileType.Floor)
                {
                    _tilePrefabsArr[x, y] = Instantiate(_floorPrefab, new Vector3(x, y, 0), Quaternion.identity, transform.GetChild(1));

                    // Randomize floor sprite
                    int randSpriteNum = Random.Range(0, 3);
                    _tilePrefabsArr[x, y].GetComponent<SpriteRenderer>().sprite = randSpriteNum == 0 ? _floor1 : randSpriteNum == 1 ? _floor2 : _floor3;
                }
                else if (GridArr[x - 1, y + 1] == TileType.Floor
                            || GridArr[x - 1, y] == TileType.Floor
                            || GridArr[x - 1, y - 1] == TileType.Floor
                            || GridArr[x, y + 1] == TileType.Floor
                            || GridArr[x, y - 1] == TileType.Floor
                            || GridArr[x + 1, y + 1] == TileType.Floor
                            || GridArr[x + 1, y] == TileType.Floor
                            || GridArr[x + 1, y - 1] == TileType.Floor)
                {
                    if (GridArr[x, y - 1] == TileType.Floor)
                    {
                        _tilePrefabsArr[x, y] = Instantiate(_wallSidePrefab, new Vector3(x, y, 0), Quaternion.identity, transform.GetChild(0));
                    }
                    else
                    {
                        _tilePrefabsArr[x, y] = Instantiate(_wallTopPrefab, new Vector3(x, y, 0), Quaternion.identity, transform.GetChild(0));
                    }
                }
            }
        }
    }
}