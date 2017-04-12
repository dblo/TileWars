using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour {
    public int maxArmyCount;
    public Vector2 spawnpoint;
    private List<Army> armies;
	private int logicCounter;
    private GameBoard gameBoard;
    public GameObject armyPrefab;

    private void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        armies = new List<Army>(maxArmyCount);
        SpawnArmies();
    }

    void Update () {
        logicCounter--;
        if (logicCounter <= 0)
        {
            logicCounter = 50;
            RespawnnDeadArmies();
            MoveArmies(false);
        }
    }

    private void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            armies.Add(Instantiate(armyPrefab, transform).GetComponent<Army>());
        }
    }

    private void RespawnnDeadArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            if (armies[i] == null)
                armies[i] = Instantiate(armyPrefab, transform).GetComponent<Army>();
        }
    }

    private void MoveArmies(bool forceMove)
    {
        foreach (var army in armies)
        {
            if (army != null && !army.IsInCombat() && (army.IsStationary() || forceMove))
            {
                var col = Random.Range(0, gameBoard.GetColsCount()-1);
                var row = Random.Range(0, gameBoard.GetRowsCount()-1);
                Tile tile = gameBoard.GetTile(row, col);
                army.MoveTo(tile);
            }
        }
    }
}
