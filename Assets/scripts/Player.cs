using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum ArmyType { Infantry, Artillery };

public class Player : MonoBehaviour
{
    [SerializeField]
    protected int maxArmyCount;
    [SerializeField]
    protected GameObject armyPrefab;
    [SerializeField]
    private GameObject hqPrefab;
    protected HQ hq;

    private void Awake()
    {
        hq = Instantiate(hqPrefab, transform).GetComponent<HQ>();
    }

    internal int GetCash()
    {
        return cash;
    }

    protected List<Army> armies;
    private int cash;
    private int score;

    virtual protected void Start()
    {
        armies = new List<Army>(maxArmyCount);
        SpawnArmies();
    }

    public ReadOnlyCollection<Army> GetArmies()
    {
        return armies.AsReadOnly();
    }

    public void AttemptBuyArmy(Direction dir)
    {
        if (cash >= 100)
        {
            SpawnArmy(hq.GetSpawnPoint());
            cash -= 100;
        }
    }

    private void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            SpawnArmy(hq.GetSpawnPoint());
        }
    }

    protected void SpawnArmy(Vector2 spawnPoint)
    {
        var newArmy = Instantiate(armyPrefab, transform).GetComponent<Army>();
        newArmy.transform.position = spawnPoint;
        armies.Add(newArmy);
    }

    internal void KillArmy(List<Army> toRemove)
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
    }

    internal void UpdateUI()
    {
        hq.UpdateUI();
    }
}
