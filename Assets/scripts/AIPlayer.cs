using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : Player
{
    private int logicCounter;
    private GameBoard gameBoard;

    protected override void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
    }

    void Update()
    {
        logicCounter--;
        if (logicCounter <= 0)
        {
            logicCounter = 50;
            RespawnDeadArmies();
            MoveArmies();
        }
    }

    private void RespawnDeadArmies()
    {
        for (int i = 0; i < maxArmyCount - armies.Count; i++)
        {
            AddCash(Army.PurchaseCost(0));
            TryBuyArmy(GetRandomArmyPrefab(), 0);
            armies.Last().RandomizeStats();
        }
    }

    protected override void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            AddCash(Army.PurchaseCost(0));
            TryBuyArmy(GetRandomArmyPrefab(), 0);
            armies.Last().RandomizeStats();
        }
    }

    protected override bool TryBuyArmy(GameObject prefab, int armyRank)
    {
        var newArmy = Instantiate(prefab, transform).GetComponent<Army>();
        newArmy.transform.position = GetSpawnPoint();
        newArmy.SetRank(armyRank);
        newArmy.ChangeTeam(team);
        armies.Add(newArmy);
        return true;
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
