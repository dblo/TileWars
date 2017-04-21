﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

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
    }

    void Start () {
        foreach (Transform trans in transform)
        {
            var tile = trans.GetComponent<Tile>();
            if (tile)
                tiles.Add(tile);
        }
        tiles = tiles.OrderBy(y => y.transform.position.y).ThenBy(x => x.transform.position.x).ToList();
        traverseableTiles = tiles.OfType<TraversableTile>().ToList();
    }

    internal int GetColsCount()
    {
        return boardCols;
    }

    internal int GetRowsCount()
    {
        return boardRows;
    }

    //private Tile GetTile(Vector2 screenCoordinate)
    //{
    //    var worldCoordinate = Camera.main.ScreenToWorldPoint(screenCoordinate);
    //    return GetTile(worldCoordinate.y, worldCoordinate.x);
    //}

    //private Tile GetTile(float row, float col)
    //{
    //    return GetTile((int)row, (int)col);
    //}

    public TraversableTile GetTile(int row, int col)
    {
        if (row < 0 || col < 0 || row >= boardRows || col >= boardCols)
            throw new ArgumentException();
        return tiles[row * boardCols + col] as TraversableTile;
    }

    //public ReadOnlyCollection<Tile> GetTiles()
    //{
    //    return tiles.AsReadOnly();
    //}

    internal ReadOnlyCollection<TraversableTile> GetTraversableTiles()
    {
        return traverseableTiles.AsReadOnly();
    }
}
