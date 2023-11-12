using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PFTest : MonoBehaviour
{
    private Pathfinding _pathfinding;

    private void Start()
    {
        _pathfinding = new Pathfinding(10, 10, true);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PathfindToPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        if (Input.GetMouseButtonDown(1))
        {
            PlaceWall(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    private void PlaceWall(Vector3 pos)
    {
        _pathfinding.GetGrid().GetXY(pos, out int x, out int y);
        _pathfinding.GetNode(x, y).IsWalkable = !_pathfinding.GetNode(x, y).IsWalkable;
        if (!_pathfinding.GetNode(x, y).IsWalkable)
        {
            _pathfinding.GetGrid().SetTextColor(x, y, Color.black);
        }
        else
        {
            _pathfinding.GetGrid().SetTextColor(x, y, Color.white);
        }
    }

    private void PathfindToPos(Vector3 targetPos)
    {
        _pathfinding.GetGrid().GetXY(targetPos, out int x, out int y);
        List<PathNode> path = _pathfinding.FindPath(0, 0, x, y);
        if (path != null)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(
                    new Vector3(path[i].X, path[i].Y) * 10f + Vector3.one * 5,
                    new Vector3(path[i + 1].X, path[i + 1].Y) * 10f + Vector3.one * 5,
                    Color.green, 3f);
            }
        }
    }
}
