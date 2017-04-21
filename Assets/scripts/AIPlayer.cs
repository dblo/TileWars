using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    private int logicCounter;
    private GameBoard gameBoard;

    override protected void Start()
    {
        base.Start();
        gameBoard = FindObjectOfType<GameBoard>();
    }

    void Update()
    {
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
            SpawnArmy(hq.GetSpawnPoint(), GetRandomArmyPrefab());
        }
    }

    protected GameObject GetRandomArmyPrefab()
    {
        if (Random.Range(0, 2) == 0)
            return infantryPrefab;
        else
            return artilleryPrefab;
    }

    protected override void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            SpawnArmy(hq.GetSpawnPoint(), GetRandomArmyPrefab());
        }
    }

    protected override void SpawnArmy(Vector2 spawnPoint, GameObject prefab)
    {
        base.SpawnArmy(spawnPoint, prefab);
        //armies[armies.Count - 1].RandomizeStats();
        armies[armies.Count - 1].SetLevel(0);
    }

    private void MoveArmies()
    {
        foreach (var army in armies)
        {
            if (!army.IsInCombat() && army.IsStationary())
            {
                TraversableTile nextDesination = null;
                while (nextDesination == null)
                {
                    var col = Random.Range(0, gameBoard.GetColsCount() - 1);
                    var row = Random.Range(0, gameBoard.GetRowsCount() - 1);
                    nextDesination = gameBoard.GetTile(row, col);
                }
                army.MoveTo(nextDesination);
            }
        }
    }
}
