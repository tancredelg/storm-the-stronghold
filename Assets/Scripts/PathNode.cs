using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int X { get; }
    public int Y { get; }
    public Vector3 WorldPos => new Vector3(X, Y, -5);

    public int GCost, HCost;
    public int FCost => GCost + HCost;
    public PathNode LastNode;
    public bool IsWalkable;

    private readonly Grid<PathNode> _grid;

    public PathNode(Grid<PathNode> grid, int x, int y)
    {
        _grid = grid;
        X = x;
        Y = y;
        IsWalkable = false;
    }

    public override string ToString()
    {
        return $"{(IsWalkable ? "Walkable" : "Unwalkable")} Pathnode ({X},{Y})";
    }
}
