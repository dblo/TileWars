using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class GameBoard : MonoBehaviour {
    [SerializeField]
    private int boardRows;
    [SerializeField]
    private int boardCols;
    private List<Tile> tiles;
    private List<TraversableTile> traverseableTiles;

    private void Awake()
    {
        tiles = new List<Tile>(boardRows * boardCols);
        foreach (Transform trans in transform)
        {
            var tile = trans.GetComponent<Tile>();
            if (tile)
            {
                // All tiles to be anchored at integer points
                Assert.IsTrue(tile.transform.position.x < Mathf.Ceil(tile.transform.position.x));
                Assert.IsTrue(tile.transform.position.y < Mathf.Ceil(tile.transform.position.y));
                tiles.Add(tile);
            }
        }
        tiles = tiles.OrderBy(y => y.transform.position.y).ThenBy(x => x.transform.position.x).ToList();
        traverseableTiles = tiles.OfType<TraversableTile>().ToList();
        Assert.IsTrue(tiles.Count == boardRows * boardCols);
    }

    internal int GetColsCount()
    {
        return boardCols;
    }

    internal int GetRowsCount()
    {
        return boardRows;
    }

    // Row, col in WorldSpace
    public TraversableTile GetTile(int row, int col)
    {
        if (row < 0 || col < 0 || row >= boardRows || col >= boardCols)
            throw new ArgumentException();
        return tiles[row * boardCols + col] as TraversableTile;
    }

    internal ReadOnlyCollection<TraversableTile> GetTraversableTiles()
    {
        return traverseableTiles.AsReadOnly();
    }

    internal bool IsFrontlinetile(TraversableTile tile, Team InquiringTeam)
    {
        var tilePos = tile.transform.position;
        int leftCol = (int)Mathf.Clamp(tilePos.x - 1, 0, tilePos.x);
        int rightCol = (int)Mathf.Clamp(tilePos.x + 1, tilePos.x, boardCols - 1);
        int lowerY = (int)Mathf.Clamp(tilePos.y - 1, 0, tilePos.y);
        int upperY = (int)Mathf.Clamp(tilePos.y + 1, tilePos.y, boardRows - 1);

        for (int i = leftCol; i <= rightCol; i++)
        {
            for (int j = lowerY; j <= upperY; j++)
            {
                var trans1 = tile.transform;

                if (GetTile(j, i).ControlledBy() == InquiringTeam)
                {
                    var trans2 = GetTile(j, i).transform;
                    return true;
                }
            }
        }
        return false;
    }
}
