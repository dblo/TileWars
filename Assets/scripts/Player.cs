using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ArmyType { Infantry, Artillery, Cavalery };

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

    private void Awake()
    {
    }

    virtual protected void Start()
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

    public void AttemptBuyInfantry() {
        AttemptBuyArmy(ArmyType.Infantry);
    }

    public void AttemptBuyArtillery()
    {
        AttemptBuyArmy(ArmyType.Artillery);
    }

    public void AttemptBuyCavalry()
    {
        AttemptBuyArmy(ArmyType.Cavalery);
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
            case ArmyType.Cavalery:
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

    internal string GetScore()
    {
        return score.ToString();
    }

    internal void OnEnemiesKilled(List<Army> armiesPendingRemoval)
    {
        foreach (var army in armies)
        {
            army.OnEnemiesKilled(armiesPendingRemoval);
        }
    }
}
