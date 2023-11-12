using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Room
{
    public RectInt Rect;
    public RoomType RoomType;
    public int EnemySpawnCount;
    public Vector2Int ChestPosition;
    public List<Room> ConnectedRooms { get; }
    public Vector2Int Center => Vector2Int.FloorToInt(Rect.center);
    public Vector3 CenterAsWorldPos => new Vector3(Center.x, Center.y, -5);
    
    public Room(RectInt rect)
    {
        Rect = rect;
        ConnectedRooms = new List<Room>();
    }

    public void SetRoomType(RoomType roomType)
    {
        RoomType = roomType;
        if (RoomType == RoomType.End)
        {
            ChestPosition = Center;
            EnemySpawnCount = 0;
        }
        else
        {
            switch (RoomType)
            {
                case RoomType.Start:
                    EnemySpawnCount = 0;
                    break;
                case RoomType.Melee:
                    EnemySpawnCount = 4;
                    break;
                case RoomType.Projectile:
                    EnemySpawnCount = 2;
                    break;
                default:
                    EnemySpawnCount = 0;
                    break;
            }
            ChestPosition = new Vector2Int(Center.x + Random.Range(-1, 2), Center.y + Random.Range(-1, 2));
        }
    }

    public List<Vector3> GetTilesInRoom()
    {
        var temp = new List<Vector3>();
        foreach (var p in Rect.allPositionsWithin)
        {
            if (p == ChestPosition) continue;
            temp.Add(new Vector3(p.x, p.y, -5));
        }
        return temp;
    }

    public float GetDistanceToRoom(Room other) => Vector2.Distance(Rect.center, other.Rect.center);

    public bool TryConnectRoom(Room room)
    {
        if (room == this || room.ConnectedRooms.Contains(this)) return false;
        ConnectedRooms.Add(room);
        return true;
    }

    public bool Contains(Vector3 pos) => Rect.Contains(Vector2Int.FloorToInt(pos));

    public bool Contains(int x, int y) => Rect.Contains(new Vector2Int(x, y));

    public override string ToString() => $"Room{Center}";
}
