﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };
public enum ArmyType { Infantry = 0, Cavalry = 1, Artillery = 2};

public abstract class Army : MonoBehaviour, ISelectableObject
{
    private const double REACHED_WAYPOINT_DISTANCE = .05;
    protected readonly float HILL_RANGE_MULTIPLIER = 1.5f;

    [SerializeField]
    protected Team team;
    private Text powerText;
    private bool inCombat;
    private List<Army> enemiesInRange = new List<Army>();
    private List<Army> collidingEnemies = new List<Army>();
    private List<Vector2> currentTravelPath = new List<Vector2>();
    protected Transform rangeDisplay;
    protected bool inHill;
    TraversableTile nowInTile;
    protected int rank = 0; // Shown as rank+1 in-game

    private static List<int> upgradeCostLevels = new List<int> { 0, 100, 500, 1000 };
    private static List<int> purchaseCostLevels = new List<int> { 100, 200, 400, 800 };
    
    #region Combat Stats
    private int power;
    [SerializeField]
    protected int attackDamage = 10;
    [SerializeField]
    protected int hp = 100;
    [SerializeField]
    protected float speed = 0.03f;
    [SerializeField]
    protected float range = 0.5f;
    #endregion

    #region Getters/Setters/Predicates
    internal bool IsAlive()
    {
        return hp > 0;
    }

    internal float GetSpeed()
    {
        return speed;
    }

    internal bool IsEnemy(Army other)
    {
        return team != other.team;
    }

    internal bool IsStationary()
    {
        return currentTravelPath.Count == 0;
    }

    internal bool IsInCombat()
    {
        return inCombat;
    }

    internal string GetPower()
    {
        return power.ToString();
    }

    private void SetShowRangeDisplay(bool val)
    {
        rangeDisplay.gameObject.SetActive(val);
    }

    private Vector2 GetNextWaypoint()
    {
        if (currentTravelPath.Count == 0)
            throw new System.ArgumentException();
        return currentTravelPath[0];
    }

    internal Team GetTeam()
    {
        return team;
    }
    #endregion

    protected virtual void Awake()
    {
        foreach (Transform trans in transform)
        {
            if (trans.name == "RangeDisplay")
            {
                rangeDisplay = trans;
                break;
            }
        }
        powerText = GetComponentInChildren<Text>();
    }

    private void Start()
    {
        UpdatePower();
        OnRangeChanged();
        UpdateText();
    }

    internal static ArmyType ArmyToArmyType(Army army)
    {
        if (army is Infantry)
            return ArmyType.Infantry;
        else if (army is Artillery)
            return ArmyType.Artillery;
        throw new System.ArgumentException();
    }

    internal void OnEnteredTile(TraversableTile tile)
    {
        nowInTile = tile;
        nowInTile.AddOccupant(this);

        switch (tile.GetTileType())
        {
            case TileType.Plains:
                break;
            case TileType.Hill:
                SetInHill(true);
                break;
            case TileType.Mountain:
                break;
            default:
                break;
        }
    }

    internal void OnExitedTile()
    {
        if (nowInTile == null)
            return;

        nowInTile.RemoveOccupant(this);
        switch (nowInTile.GetTileType())
        {
            case TileType.Plains:
                break;
            case TileType.Hill:
                SetInHill(false);
                break;
            case TileType.Mountain:
                break;
            default:
                break;
        }
        nowInTile = null;
    }

    private void SetInHill(bool val)
    {
        inHill = val;
        UpdateRange();
    }

    internal void Upgrade()
    {
        rank++;
        UpdateAttackDamage();
        UpdateHP();
        UpdateSpeed();
        UpdateRange();
    }

    protected T GetItemAtRankOrLast<T>(ReadOnlyCollection<T> list)
    {
        if (rank < list.Count)
            return list[rank];
        return list.Last();
    }
    
    protected virtual void UpdateAttackDamage()
    {
        attackDamage = GetItemAtRankOrLast(GetAttackDamageLevels());
    }

    protected virtual void UpdateHP()
    {
        hp = GetItemAtRankOrLast(GetHPLevels());
    }

    protected virtual void UpdateSpeed()
    {
        speed = GetItemAtRankOrLast(GetSpeedLevels());
    }

    protected virtual void UpdateRange()
    {
        range = GetItemAtRankOrLast(GetRangeLevels());
        if (inHill)
            range *= HILL_RANGE_MULTIPLIER;
        OnRangeChanged();
    }

    protected abstract ReadOnlyCollection<int> GetAttackDamageLevels();
    protected abstract ReadOnlyCollection<int> GetHPLevels();
    protected abstract ReadOnlyCollection<float> GetSpeedLevels();
    protected abstract ReadOnlyCollection<float> GetRangeLevels();

    private void OnDestroy()
    {
        nowInTile.RemoveOccupant(this);
    }

    internal void OnEnemiesKilled(List<Army> armiesPendingRemoval)
    {
        enemiesInRange = enemiesInRange.Except(armiesPendingRemoval).ToList();
        collidingEnemies = collidingEnemies.Except(armiesPendingRemoval).ToList();
    }

    protected virtual void OnRangeChanged()
    {
        var coll = transform.Find("RangeManager").GetComponent<CircleCollider2D>();
        coll.radius = range;
        rangeDisplay.localScale = new Vector3(range, range);
    }

    internal void RandomizeStats()
    {
        attackDamage = UnityEngine.Random.Range(1, 6);
        hp = UnityEngine.Random.Range(5, 15);
        speed = UnityEngine.Random.Range(0.01f, 0.04f);
        range = UnityEngine.Random.Range(0.5f, 1f);
        UpdatePower();
    }

    internal void Cheat()
    {
        attackDamage = UnityEngine.Random.Range(3, 9);
        hp = UnityEngine.Random.Range(25, 35);
        speed = UnityEngine.Random.Range(0.03f, 0.03f);
        //range = UnityEngine.Random.Range(1.5f, 1.5f);
        UpdatePower();
    }

    internal void SetLevel(int level)
    {
        attackDamage = 2;
        hp = 10;
    }

    internal void ChangeTeam(Team newTeam)
    {
        team = newTeam;

        var renderer = GetComponent<SpriteRenderer>();
        if (team == Team.Red)
            renderer.color = Color.magenta;// new Color(230, 130, 130);
        else if (team == Team.Blue)
            renderer.color = Color.cyan;// new Color(0, 0, 255);
    }

    private void UpdatePower()
    {
        power = hp * attackDamage;
    }

    internal void TakeDamage(int damage)
    {
        hp -= damage;
        UpdatePower();
    }

    internal void OnEnemyInRange(Army enemy)
    {
        enemiesInRange.Add(enemy);
        inCombat = true;
    }

    void FixedUpdate()
    {
        if (currentTravelPath.Count > 0)// && collidingEnemies.Count == 0)
        {
            var newPos = Vector2.MoveTowards(transform.position, GetNextWaypoint(), GetSpeed());
            transform.position = newPos;
            if (Vector2.Distance(transform.position, GetNextWaypoint()) < REACHED_WAYPOINT_DISTANCE)
            {
                currentTravelPath.RemoveAt(0);
            }
        }
    }

    internal void OnEnemyOutOfRange(Army army)
    {
        enemiesInRange.Remove(army);
        if (enemiesInRange.Count == 0)
        {
            inCombat = false;
        }
    }

    internal void MoveTo(TraversableTile tile)
    {
        MoveTo(tile.transform.position);
    }

    internal void MoveTo(Vector3 worldCoord)
    {
        currentTravelPath.Clear();
        currentTravelPath.Add(worldCoord);
    }

    internal void DoCombat()
    {
        if (enemiesInRange.Count > 0)
        {
            Assert.IsTrue(enemiesInRange[0] != null);
            Attack(enemiesInRange[0]);
        }
    }

    protected virtual void Attack(Army enemy)
    {
        enemy.TakeDamage(attackDamage);
        enemy.UpdateText();
        //Debug.Log(team + " attacked: " + attackDamage + "dmg. " + enemy.team + " hp: " + enemy.armySize);
    }

    private void UpdateText()
    {
        powerText.text = GetPower();
    }

    internal void RemoveUnreachableWaypoint()
    {
        if (currentTravelPath.Count > 0)
            currentTravelPath.RemoveAt(0);
    }

    protected virtual void OnMouseDown()
    {
        GameManager.Get().OnSelection(this);
    }

    public virtual void ChangeTravelPath(List<Vector2> swipePath)
    {
        currentTravelPath = swipePath;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var army = collision.collider.GetComponent<Army>();
        if (army != null && IsEnemy(army))
        {
            collidingEnemies.Add(army);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Wall") // TODO improve
        {
            RemoveUnreachableWaypoint();
        }
        else if (collision.gameObject.GetComponent<Tile>() != null)
        {
            RemoveUnreachableWaypoint();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        var army = collision.collider.GetComponent<Army>();
        if (army != null && IsEnemy(army))
        {
            collidingEnemies.Remove(army);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tile = collision.GetComponent<TraversableTile>();
        if (tile != null)
        {
            OnExitedTile();
            OnEnteredTile(tile);
        }
    }

    public void Select()
    {
        SetShowRangeDisplay(true);
    }

    public void Deselect()
    {
        SetShowRangeDisplay(false);
    }

    public abstract string GetDescriptor();

    internal static int UpgradeCost(int rank)
    {
        if (rank >= upgradeCostLevels.Count - 1)
            return upgradeCostLevels.Last();
        return upgradeCostLevels[rank];
    }

    internal int PurchaseCost(int rank)
    {
        if (UpgradeMaxed())
            return purchaseCostLevels[upgradeCostLevels.Count - 1];
        return purchaseCostLevels[rank];
    }

    protected bool UpgradeMaxed()
    {
        return rank >= upgradeCostLevels.Count - 1;
    }

    internal abstract bool IsType(ArmyType type);
}
