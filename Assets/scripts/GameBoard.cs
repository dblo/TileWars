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
}
