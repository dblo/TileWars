using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GameBoard : MonoBehaviour {
    private List<Tile> tiles;
    [SerializeField]
    private int boardRows;
    [SerializeField]
    private int boardCols;
    [SerializeField]
    private GameObject plainsPrefab;

    void Start () {
        tiles = new List<Tile>(boardRows * boardCols);
        for(int i = 0 ; i < boardRows; ++i)
        {
            for(int j = 0; j < boardCols; ++j)
            {
                var newTile = Instantiate(plainsPrefab, transform);
                newTile.transform.position += new Vector3(j, i, 0);
                tiles.Add(newTile.GetComponent<Tile>());
            }
        }
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
