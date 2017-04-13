using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player {
	private int logicCounter;
    private GameBoard gameBoard;

    override protected void Start()
    {
        base.Start();
        gameBoard = FindObjectOfType<GameBoard>();
    }

    void Update () {
        logicCounter--;
        if (logicCounter <= 0)
        {
            logicCounter = 50;
            RespawnnDeadArmies();
            MoveArmies();
        }
    }

    private void RespawnnDeadArmies()
    {
        for (int i = 0; i < maxArmyCount - armies.Count; i++)
        {
            SpawnArmy(hq.GetSpawnPoint());
        }
    }

    private void MoveArmies()
    {
        foreach (var army in armies)
        {
            if (army != null && !army.IsInCombat() && army.IsStationary())
            {
                var col = Random.Range(0, gameBoard.GetColsCount()-1);
                var row = Random.Range(0, gameBoard.GetRowsCount()-1);
                Tile tile = gameBoard.GetTile(row, col);
                army.MoveTo(tile);
            }
        }
    }
}
