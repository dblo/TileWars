using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    Text buyInfantryText;
    Text buyCavalryText;
    Text buyArtilleryText;

    // Mapped to by ArmyType for 0=Infantry, 1=Cavalry, 2=Artillery
    List<int> armyRanks = new List<int> { 0, 0, 0 };
    public bool cheater; //debug

    private void Awake()
    {
        armies = new List<Army>(maxArmyCount);
        buyInfantryText = GameObject.Find("BuyInfantryText").GetComponent<Text>();
        buyCavalryText = GameObject.Find("BuyCavalryText").GetComponent<Text>();
        buyArtilleryText = GameObject.Find("BuyArtilleryText").GetComponent<Text>();
        SpawnArmies();
    }

    protected virtual void Start()
    {
        UpdateBuyText(ArmyType.Infantry, armyRanks[(int)ArmyType.Infantry]);
        //UpdateBuyText(ArmyType.Cavalry, armyRanks[(int)ArmyType.Cavalry]);
        UpdateBuyText(ArmyType.Artillery, armyRanks[(int)ArmyType.Artillery]);
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
                if (cash >= Army.PurchaseCost(GetInfantryRank()))
                {
                    var newArmy = SpawnArmy(GetSpawnPoint(), infantryPrefab);
                    GameManager.Get().OnSelection(newArmy);
                    cash -= Army.PurchaseCost(GetInfantryRank());
                }
                break;
            //case ArmyType.Cavalry:
            //    if (cash >= Army.PurchaseCost(GetCavalryRank()))
            //    {
            //        var newArmy = SpawnArmy(GetSpawnPoint(), );
            //        GameManager.Get().OnSelection(newArmy);
            //        cash -= Army.PurchaseCost(GetCavalryRank()));
            //    }
            //    break;
            case ArmyType.Artillery:
                if (cash >= Army.PurchaseCost(GetArtilleryRank()))
                {
                    var newArmy = SpawnArmy(GetSpawnPoint(), artilleryPrefab);
                    GameManager.Get().OnSelection(newArmy);
                    cash -= Army.PurchaseCost(GetArtilleryRank());
                }
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

    protected virtual Army SpawnArmy(Vector2 spawnPoint, GameObject prefab)
    {
        var newArmy = Instantiate(prefab, transform).GetComponent<Army>();
        newArmy.transform.position = spawnPoint;
        newArmy.ChangeTeam(team);
        armies.Add(newArmy);

        if (cheater)
            newArmy.Cheat();
        return newArmy;
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
            UpdateBuyText(armyType, armyRanks[(int)armyType]);
            return true;
        }
        return false;
    }

    private void UpdateBuyText(ArmyType armyType, int armyRank)
    {
        switch (armyType)
        {
            case ArmyType.Infantry:
                buyInfantryText.text = "Buy\nI" + (armyRank + 1) + "-$" + Army.PurchaseCost(armyRank).ToString();
                return;
            case ArmyType.Cavalry:
                break;
            case ArmyType.Artillery:
                buyArtilleryText.text = "Buy\nA" + (armyRank + 1) + "-$" + Army.PurchaseCost(armyRank).ToString();
                return;
        }
        throw new ArgumentException();
    }

    private void Upgrade(ArmyType armyType)
    {
        foreach (var army in armies)
        {
            if (army.IsType(armyType))
                army.Upgrade();
        }
    }

    private int GetInfantryRank()
    {
        return armyRanks[(int)ArmyType.Infantry];
    }

    private int GetCavalryRank()
    {
        return armyRanks[(int)ArmyType.Cavalry];
    }

    private int GetArtilleryRank()
    {
        return armyRanks[(int)ArmyType.Artillery];
    }
}
