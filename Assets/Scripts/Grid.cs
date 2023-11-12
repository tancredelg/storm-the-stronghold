using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    public int Width { get; }
    public int Height { get; }
    public T[,] GridArr { get; }

    private Vector3 _originPos;
    private TextMesh[,] _textArr;

    public Grid(int w, int h, Vector3 originPos, Func<Grid<T>, int, int, T> createGridObject)
    {
        Width = w;
        Height = h;
        _originPos = originPos;
        GridArr = new T[Width, Height];
        _textArr = new TextMesh[Width, Height];

        for (int x = 0; x < GridArr.GetLength(0); x++)
        {
            for (int y = 0; y < GridArr.GetLength(1); y++)
            {
                GridArr[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x, y, -5f) + _originPos;
    }

    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - _originPos).x);
        y = Mathf.FloorToInt((worldPos - _originPos).y);

    }

    public void SetTextColor(int x, int y, Color color)
    {
        _textArr[x, y].color = color;
    }

    public void SetGridObject(int x, int y, T value)
    {
        if (x >= 0 && y >= 0 && x < Width && y < Height)
        {
            GridArr[x, y] = value;
            _textArr[x, y].text = GridArr[x, y].ToString();
        }
    }

    public void SetGridObject(Vector3 worldPos, T value)
    {
        GetXY(worldPos, out int x, out int y);
        SetGridObject(x, y, value);
    }

    public T GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < Width && y < Height)
        {
            return GridArr[x, y];
        }
        else
        {
            return default(T);
        }
    }

    public T GetGridObject(Vector3 worldPos)
    {
        GetXY(worldPos, out int x, out int y);
        return GetGridObject(x, y);
    }
}
