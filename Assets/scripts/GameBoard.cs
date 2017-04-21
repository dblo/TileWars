using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    private List<Tile> tiles;
    [SerializeField]
    private int boardRows;
    [SerializeField]
    private int boardCols;
    [SerializeField]
    private GameObject plainsPrefab;
    [SerializeField]
    private GameObject hillPrefab;
    [SerializeField]
    private GameObject wallPrefab;
    [SerializeField]

    private void Awake()
    {
        tiles = new List<Tile>(boardRows * boardCols);
    }

    void Start () {
        foreach (Transform trans in transform)
        {
            var tile = trans.GetComponent<Tile>();
            if (tile)
            {
                int col = (int)trans.position.x;
                int row = (int)trans.position.y;
                tiles.Add(tile);
                //tiles.Insert(row * GetColsCount() + col, tile);
            }
        }

        tiles = tiles.OrderBy(y => y.transform.position.y).ThenBy(x => x.transform.position.x).ToList();
    }

    internal int GetColsCount()
    {
        return boardCols;
    }

    internal int GetRowsCount()
    {
        return boardRows;
    }

    public Tile GetTile(Vector2 screenCoordinate)
    {
        var worldCoordinate = Camera.main.ScreenToWorldPoint(screenCoordinate);
        return GetTile(worldCoordinate.y, worldCoordinate.x);
    }

    private Tile GetTile(float row, float col)
    {
        return GetTile((int)row, (int)col);
    }

    public Tile GetTile(int row, int col)
    {
        if (row < 0 || col < 0 || row >= boardRows || col >= boardCols)
            throw new ArgumentException();
        return tiles[row * boardCols + col];
    }

    public ReadOnlyCollection<Tile> GetTiles()
    {
        return tiles.AsReadOnly();
    }
}
