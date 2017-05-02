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
    protected GameObject cavalryPrefab;
    [SerializeField]
    protected GameObject artilleryPrefab;
    protected List<Army> armies;
    [SerializeField]
    protected int cash;
    protected int score;
    private const int MAX_CASH = 9999;
    [SerializeField]
    protected Team team;
    // Mapped to by ArmyType for 0=Infantry, 1=Cavalry, 2=Artillery
    protected List<int> armyRanks = new List<int> { 0, 0, 0 };
    public bool cheater; //debug
    Text buyInfantryText;
    Text buyCavalryText;
    Text buyArtilleryText;
    private int controllingTilesCount;

    public int ControllingTilesCount
    {
        get { return controllingTilesCount; }
    }

    internal void IncrementControlledTiles()
    {
        controllingTilesCount++;
    }

    internal void DecementControlledTiles()
    {
        controllingTilesCount--;
    }

    protected virtual void Awake()
    {
        armies = new List<Army>(maxArmyCount);
        buyInfantryText = GameObject.Find("BuyInfantryText").GetComponent<Text>();
        buyCavalryText = GameObject.Find("BuyCavalryText").GetComponent<Text>();
        buyArtilleryText = GameObject.Find("BuyArtilleryText").GetComponent<Text>();
    }

    protected virtual void Start()
    {
        UpdateBuyText(ArmyType.Infantry, armyRanks[(int)ArmyType.Infantry]);
        UpdateBuyText(ArmyType.Cavalry, armyRanks[(int)ArmyType.Cavalry]);
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

    protected GameObject GetArmyPrefab(ArmyType type)
    {
        switch (type)
        {
            case ArmyType.Infantry:
                return infantryPrefab;
            case ArmyType.Cavalry:
                return cavalryPrefab;
            case ArmyType.Artillery:
                return artilleryPrefab;
            default:
                throw new ArgumentException();
        }
    }

    protected ArmyType GetRandomArmyType()
    {
        var r = UnityEngine.Random.Range(0, 3);
        if (r == 0)
            return ArmyType.Infantry;
        else if (r == 1)
            return ArmyType.Cavalry;
        else
            return ArmyType.Artillery;
    }

    internal virtual bool TryBuyArmy(ArmyType armyType, Vector2 spawnPoint)
    {
        GameObject prefab;
        int armyRank;
        switch (armyType)
        {
            case ArmyType.Infantry:
                prefab = infantryPrefab;
                armyRank = GetInfantryRank();
                break;
            case ArmyType.Cavalry:
                prefab = cavalryPrefab;
                armyRank = GetCavalryRank();
                break;
            case ArmyType.Artillery:
                prefab = artilleryPrefab;
                armyRank = GetArtilleryRank();
                break;
            default:
                throw new ArgumentException();
        }

        if (cash >= Army.PurchaseCost(armyRank))
        {
            var newArmy = Instantiate(prefab, transform).GetComponent<Army>();
            newArmy.transform.position = spawnPoint;
            newArmy.SetRankAndHP(armyRank);
            newArmy.ChangeTeam(team);

            if (cheater)
                newArmy.Cheat();

            armies.Add(newArmy);
            cash -= Army.PurchaseCost(armyRank);
            GameManager.Get().OnSelectionChange(newArmy);
            return true;
        }
        return false;
    }

    protected Vector2 GetSpawnPoint()
    {
        return transform.position;
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

    internal bool TryUpgrade(Tile tile)
    {
        var upgradeCost = tile.UpgradeCost();
        if (upgradeCost <= cash && !tile.UpgradeMaxed())
        {
            tile.Upgrade();
            cash -= upgradeCost;
            return true;
        }
        return false;
    }

    internal virtual bool TryUpgrade(ArmyType armyType)
    {
        var armyRank = armyRanks[(int)armyType];
        var upgradeCost = Army.UpgradeCost(armyRank);
        if (upgradeCost <= cash)
        {
            UpgradeArmiesOfType(armyType);
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
                buyInfantryText.text = "Buy Inf\n" + "$" + Army.PurchaseCost(armyRank).ToString();
                break;
            case ArmyType.Cavalry:
                buyCavalryText.text = "Buy Cav\n" + "$" + Army.PurchaseCost(armyRank).ToString();
                break;
            case ArmyType.Artillery:
                buyArtilleryText.text = "Buy Art\n" + "$" + Army.PurchaseCost(armyRank).ToString();
                break;
            default:
                throw new ArgumentException();
        }
    }

    protected void UpgradeArmiesOfType(ArmyType armyType)
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

    internal bool CanAffordArmy(ArmyType type)
    {
        switch (type)
        {
            case ArmyType.Infantry:
                return cash >= Army.PurchaseCost(GetInfantryRank());
            case ArmyType.Cavalry:
                return cash >= Army.PurchaseCost(GetCavalryRank());
            case ArmyType.Artillery:
                return cash >= Army.PurchaseCost(GetArtilleryRank());
        }
        throw new ArgumentException();
    }

    internal bool CanAffordArmyUpgrade(ArmyType type)
    {
        switch (type)
        {
            case ArmyType.Infantry:
                return cash >= Army.UpgradeCost(GetInfantryRank());
            case ArmyType.Cavalry:
                return cash >= Army.UpgradeCost(GetCavalryRank());
            case ArmyType.Artillery:
                return cash >= Army.UpgradeCost(GetArtilleryRank());
        }
        throw new ArgumentException();
    }
}
