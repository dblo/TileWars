using System;
using UnityEngine;

public class GameBoard : MonoBehaviour {

    private Tile[] tiles;
    public int boardRows = 0;
    public int boardCols = 0;
    public GameObject plainsPrefab;

    void Start () {
        tiles = new Tile[boardRows * boardCols];
        for(int i=0; i < boardRows; ++i)
        {
            for(int j = 0; j < boardCols; ++j)
            {
                var newTile = Instantiate(plainsPrefab, transform);
                newTile.transform.position += new Vector3(j, i, 0);
                tiles[i * boardCols + j] = newTile.GetComponent<Tile>();
            }
        }
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

    private Tile GetTile(int row, int col)
    {
        if (row < 0 || col < 0 || row >= boardRows || col >= boardCols)
            throw new ArgumentException();
        return tiles[row * boardCols + col];
    }
}
