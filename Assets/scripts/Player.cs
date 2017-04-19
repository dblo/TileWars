using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public enum ArmyType { Infantry, Artillery };

public class Player : MonoBehaviour
{
    [SerializeField]
    protected int maxArmyCount;
    [SerializeField]
    protected GameObject infantryPrefab;
    [SerializeField]
    protected GameObject artilleryPrefab;
    [SerializeField]
    private GameObject hqPrefab;
    protected HQ hq;
    protected List<Army> armies;
    [SerializeField]
    private int cash;
    private int score;
    private const int MAX_CASH = 9999;
    [SerializeField]
    private Team team;

    private void Awake()
    {
        hq = Instantiate(hqPrefab, transform).GetComponent<HQ>();
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

    public void AttemptBuyArmy(ArmyType type)
    {
        switch (type)
        {
            case ArmyType.Infantry:
                if (cash >= 50)
                {
                    SpawnArmy(hq.GetSpawnPoint(), infantryPrefab);
                    cash -= 50;
                }
                break;
            case ArmyType.Artillery:
                if (cash >= 100)
                {
                    SpawnArmy(hq.GetSpawnPoint(), artilleryPrefab);
                    cash -= 100;
                }
                break;
            default:
                throw new System.ArgumentException();
        }
    }

    protected virtual void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            SpawnArmy(hq.GetSpawnPoint(), artilleryPrefab);
        }
    }

    protected virtual void SpawnArmy(Vector2 spawnPoint, GameObject prefab)
    {
        var newArmy = Instantiate(prefab, transform).GetComponent<Army>();
        newArmy.transform.position = spawnPoint;
        newArmy.ChangeTeam(team);
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
        if (cash >= MAX_CASH)
            cash = MAX_CASH;
    }

    internal void UpdateUI()
    {
        hq.UpdateUI();
    }
}
