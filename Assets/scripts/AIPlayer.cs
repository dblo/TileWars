using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    private int logicCounter;
    private GameBoard gameBoard;

    protected void Start()
    {
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
            SpawnArmy(GetSpawnPoint(), GetRandomArmyPrefab());
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
            SpawnArmy(GetSpawnPoint(), GetRandomArmyPrefab());
        }
    }

    protected override void SpawnArmy(Vector2 spawnPoint, GameObject prefab)
    {
        base.SpawnArmy(spawnPoint, prefab);
        armies[armies.Count - 1].RandomizeStats();
    }

    private void MoveArmies()
    {
        foreach (var army in armies)
        {
            if (!army.IsInCombat() && army.IsStationary())
            {
                TraversableTile nextDesination = GetRandomTraversableTile();
                army.MoveTo(nextDesination);
            }
        }
    }

    private TraversableTile GetRandomTraversableTile()
    {
        TraversableTile nextDesination = null;
        while (nextDesination == null)
        {
            var col = Random.Range(0, gameBoard.GetColsCount());
            var row = Random.Range(0, gameBoard.GetRowsCount());
            nextDesination = gameBoard.GetTile(row, col);
        }
        return nextDesination;
    }
}
