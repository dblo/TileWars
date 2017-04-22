using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    protected int maxArmyCount;
    [SerializeField]
    protected GameObject infantryPrefab;
    [SerializeField]
    protected GameObject artilleryPrefab;
    protected List<Army> armies;
    [SerializeField]
    private int cash;
    private int score;
    private const int MAX_CASH = 9999;
    [SerializeField]
    private Team team;

    // Mapped to by ArmyType for 0=Infantry, 1=Cavalry, 2=Artillery
    List<int> armyRanks = new List<int> { 0, 0, 0 };
    public bool cheater; //debug

    private void Awake()
    {
        armies = new List<Army>(maxArmyCount);
        SpawnArmies();
    }

    public ReadOnlyCollection<Army> GetArmies()
    {
        return armies.AsReadOnly();
    }

    internal int GetCash()
    {
        return cash;
    }

    public void TryBuyInfantry()
    {
        AttemptBuyArmy(ArmyType.Infantry);
    }

    public void TryBuyArtillery()
    {
        AttemptBuyArmy(ArmyType.Artillery);
    }

    public void TryBuyCavalry()
    {
        AttemptBuyArmy(ArmyType.Cavalry);
    }

    private void AttemptBuyArmy(ArmyType type)
    {
        switch (type)
        {
            case ArmyType.Infantry:
                if (cash >= 50)
                {
                    SpawnArmy(GetSpawnPoint(), infantryPrefab);
                    cash -= 50;
                }
                break;
            case ArmyType.Artillery:
                if (cash >= 100)
                {
                    SpawnArmy(GetSpawnPoint(), artilleryPrefab);
                    cash -= 100;
                }
                break;
            case ArmyType.Cavalry:
                break;
            default:
                throw new System.ArgumentException();
        }
    }

    protected Vector2 GetSpawnPoint()
    {
        return transform.position;
    }

    protected virtual void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            SpawnArmy(GetSpawnPoint(), artilleryPrefab);
        }
    }

    protected virtual void SpawnArmy(Vector2 spawnPoint, GameObject prefab)
    {
        var newArmy = Instantiate(prefab, transform).GetComponent<Army>();
        newArmy.transform.position = spawnPoint;
        newArmy.ChangeTeam(team);
        armies.Add(newArmy);

        if(cheater)
            newArmy.Cheat();
    }

    internal void KillArmies(List<Army> toRemove)
    {
        armies = armies.Except(toRemove).ToList();
        foreach (var army in toRemove)
        {
            Destroy(army.gameObject);
        }
    }

    internal void AddScore(int val)
    {
        score += val;
    }

    internal void AddCash(int val)
    {
        cash += val;
        if (cash >= MAX_CASH)
            cash = MAX_CASH;
    }

    internal int GetScore()
    {
        return score;
    }

    internal void OnEnemiesKilled(List<Army> armiesPendingRemoval)
    {
        foreach (var army in armies)
        {
            army.OnEnemiesKilled(armiesPendingRemoval);
        }
    }

    internal bool TryUpgrade(ArmyType armyType)
    {
        var armyRank = armyRanks[(int)armyType];
        var upgradeCost = Army.UpgradeCost(armyRank);
        if (upgradeCost <= cash)
        {
            Upgrade(armyType);
            cash -= upgradeCost;
            armyRanks[(int)armyType]++;
            return true;
        }
        return false;
    }

    private void Upgrade(ArmyType armyType)
    {
        foreach (var army in armies)
        {
            if (army.IsType(armyType))
                army.Upgrade();
        }
    }
}
