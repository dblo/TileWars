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
    [SerializeField]
    private GameObject wallPrefab;

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
        var leftWall = Instantiate(wallPrefab, transform);
        leftWall.transform.localScale = new Vector3(1, boardRows);
        leftWall.transform.position = transform.position + new Vector3(-1f, 2.5f);

        var rightWall = Instantiate(wallPrefab, transform);
        rightWall.transform.localScale = new Vector3(1, boardRows);
        rightWall.transform.position += new Vector3(boardCols, 2.5f);

        var upperWall = Instantiate(wallPrefab, transform);
        upperWall.transform.position += new Vector3(-0.5f + boardCols / 2f, boardRows);
        upperWall.transform.localScale = new Vector3(boardCols + 2, 1);

        var lowerWall = Instantiate(wallPrefab, transform);
        lowerWall.transform.position += new Vector3(-0.5f + boardCols / 2f, -1);
        lowerWall.transform.localScale = new Vector3(boardCols + 2, 1);
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
