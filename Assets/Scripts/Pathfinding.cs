using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private List<PathNode> _openList;
    private HashSet<PathNode> _closedList;
    private bool _useDiagonals;
    private int _startX, _startY, _endX, _endY;

    private readonly Grid<PathNode> _grid;
    private const int STRAIGHT_COST = 10;
    private const int DIAGONAL_COST = 14;

    public Pathfinding(int w, int h, bool useDiagonals)
    {
        _grid = new Grid<PathNode>(w, h, Vector3.zero, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
        _useDiagonals = useDiagonals;
    }

    public Grid<PathNode> GetGrid() => _grid;

    public PathNode GetNode(int x, int y) => _grid.GetGridObject(x, y);

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        _startX = startX;
        _startY = startY;
        _endX = endX;
        _endY = endY;

        PathNode startNode = _grid.GetGridObject(startX, startY);
        PathNode endNode = _grid.GetGridObject(endX, endY);

        _openList = new List<PathNode> { startNode };
        _closedList = new HashSet<PathNode>();

        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                PathNode pathNode = _grid.GetGridObject(x, y);
                pathNode.GCost = 999999999;
                pathNode.LastNode = null;
            }
        }
        startNode.GCost = 0;
        startNode.HCost = GetHeuristicDistance(startNode, endNode);

        while (_openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(_openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            _openList.Remove(currentNode);
            _closedList.Add(currentNode);

            foreach (var neighbourNode in GetNeighbourList(currentNode))
            {
                if (_closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.IsWalkable)
                {
                    _closedList.Add(neighbourNode);
                    continue;
                }

                int newGCost = currentNode.GCost + GetHeuristicDistance(currentNode, neighbourNode);
                if (newGCost < neighbourNode.GCost && !_openList.Contains(neighbourNode))
                {
                    neighbourNode.LastNode = currentNode;
                    neighbourNode.GCost = newGCost;
                    neighbourNode.HCost = GetHeuristicDistance(neighbourNode, endNode);
                    _openList.Add(neighbourNode);
                }
            }
        }
        // No more nodes on openList, no path found
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        var neighbourList = new List<PathNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                if (!_useDiagonals)
                    if ((x == -1 && y == -1) || (x == -1 && y == 1) || (x == 1 && y == -1) || (x == 1 && y == 1)) continue;

                int newX = currentNode.X + x;
                int newY = currentNode.Y + y;

                if (newX >= 0 && newX < _grid.Width && newY >= 0 && newY < _grid.Height)
                {
                    neighbourList.Add(GetNode(newX, newY));
                }
            }
        }
        return neighbourList;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        var path = new List<PathNode>() { endNode };
        var currentNode = endNode;

        while (currentNode.LastNode != null)
        {
            path.Add(currentNode.LastNode);
            currentNode = currentNode.LastNode;
        }

        path.Reverse();
        return path;
    }

    private int GetHeuristicDistance(PathNode a, PathNode b)
    {
        int dx = Mathf.Abs(a.X - b.X);
        int dy = Mathf.Abs(a.Y - b.Y);
        if (_useDiagonals)
        {
            return (STRAIGHT_COST * Mathf.Abs(dx - dy)) + (DIAGONAL_COST * Mathf.Min(dx, dy));
        }
        int dx2 = Mathf.Abs(_startX - _endX);
        int dy2 = Mathf.Abs(_startY - _endY);
        int cross = Mathf.Abs(dx * dx2 - dy * dy2);
        return (STRAIGHT_COST * Mathf.Abs(dx - dy)) + (DIAGONAL_COST * Mathf.Min(dx, dy)) - Mathf.RoundToInt(cross);
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
