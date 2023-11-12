using System.Collections.Generic;

[System.Serializable]
public class DungeonData
{
    public int Level;
    public int Size;
    public TileType[] GridArr1D;
    public List<Room> Rooms;

    public override string ToString()
    {
        return $"[DungeonData]\nLevel: {Level}\nSize: {Size}";
    }
}
