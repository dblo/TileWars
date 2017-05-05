using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };
public enum ArmyType { Infantry = 0, Cavalry = 1, Artillery = 2 };

public abstract class Army : MonoBehaviour, ISelectableObject, ITileObserver
{
    private static float REACHED_WAYPOINT_DISTANCE = .05f;
    protected static float HILL_RANGE_MULTIPLIER = 1.5f;
    protected static float FAVORED_COMBAT_BONUS = 1.5f;

    [SerializeField]
    protected Team team;
    private Text powerText;
    private bool inCombat;
    private List<Army> enemiesInRange = new List<Army>();
    private List<Army> collidingEnemies = new List<Army>();
    private List<Vector2> currentTravelPath = new List<Vector2>();
    protected Transform rangeDisplay;
    TraversableTile nowInTile;
    protected int rank = 0; // Shown as rank+1 in-game

    private static List<int> upgradeCostLevels = new List<int> { 100, 500, 1000 };
    private static List<int> purchaseCostLevels = new List<int> { 50, 100, 200, 400 };

    #region Combat Stats
    private int power;
    [SerializeField]
    protected int attackDamage;
    [SerializeField]
    protected int defense;
    [SerializeField]
    protected int hp;
    [SerializeField]
    protected float speed;
    [SerializeField]
    protected float range;
    protected ITileCombatModifiers tileCombatMods;
    private const int MIN_DAMAGE_TAKEN = 1;
    private int maxHP;
    private int regen = 1;
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

    internal bool InVisibleTile()
    {
        if (nowInTile)
        {
            // May get here if inquiring right after this was spawned, before ontriggerenter has registered intile
            return nowInTile.GetVisible();
        }
        return false;
    }

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
        UpdateMaxHP();
        hp = maxHP;
    }

    private void Start()
    {
        UpdatePower();
        OnRangeChanged();
    }

    void FixedUpdate()
    {
        if (currentTravelPath.Count > 0)
        {
            var speed = GetSpeed();
            if (collidingEnemies.Count > 0)
                speed *= 0.5f;

            // TODO This only needed for immediatly after instantiation, before ontriggerenter has executed. Get rid off
            if (nowInTile != null)
            {
                var currentTileControlledBy = nowInTile.ControlledBy();
                if (currentTileControlledBy != team && currentTileControlledBy != Team.Neutral)
                    speed *= 0.75f;
            }

            var newPos = Vector2.MoveTowards(transform.position, GetNextWaypoint(), speed);
            transform.position = newPos;
            if (Vector2.Distance(transform.position, GetNextWaypoint()) < REACHED_WAYPOINT_DISTANCE)
            {
                currentTravelPath.RemoveAt(0);
            }
        }
    }

    internal static ArmyType ArmyToArmyType(Army army)
    {
        if (army is Infantry)
            return ArmyType.Infantry;
        else if (army is Cavalry)
            return ArmyType.Cavalry;
        else if (army is Artillery)
            return ArmyType.Artillery;
        throw new System.ArgumentException();
    }

    internal void OnEnteredTile(TraversableTile tile)
    {
        OnLeftTile();
        nowInTile = tile;
        nowInTile.EnterTile(this);
    }

    internal void OnLeftTile()
    {
        if (nowInTile == null)
            return;

        nowInTile.LeaveTile(this);
    }

    internal void Regen()
    {
        if (hp >= maxHP) return;
        hp = Mathf.Clamp(hp + regen, hp, maxHP);
        UpdatePower();

    }

    internal void Upgrade()
    {
        rank++;
        UpdateStats();
    }

    internal ReadOnlyCollection<Army> GetEnemiesInRange()
    {
        return enemiesInRange.AsReadOnly();
    }

    internal void SetRankAndHP(int aRank)
    {
        rank = aRank;
        maxHP = GetItemAtRankOrLast(GetHPLevels());
        hp = maxHP;
    }

    private void UpdateStats()
    {
        UpdateAttackDamage();
        UpdateDefense();
        UpdateSpeed();
        UpdateRange();
        UpdatePower();
        UpdateMaxHP();
    }

    internal void SetVisible(bool IsVisible)
    {
        GetComponent<SpriteRenderer>().enabled = IsVisible;
        GetComponentInChildren<Canvas>().enabled = IsVisible;
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

    protected virtual void UpdateDefense()
    {
        defense = Mathf.CeilToInt(GetItemAtRankOrLast(GetDefenseLevels()));
    }

    protected virtual void UpdateMaxHP()
    {
        maxHP = GetItemAtRankOrLast(GetHPLevels());
    }

    protected virtual void UpdateSpeed()
    {
        speed = GetItemAtRankOrLast(GetSpeedLevels());
    }

    protected virtual void UpdateRange()
    {
        range = GetItemAtRankOrLast(GetRangeLevels());
        range *= tileCombatMods.RangeMultiplier;
        OnRangeChanged();
    }

    protected abstract ReadOnlyCollection<int> GetAttackDamageLevels();
    protected abstract ReadOnlyCollection<int> GetDefenseLevels();
    protected abstract ReadOnlyCollection<int> GetHPLevels();
    protected abstract ReadOnlyCollection<float> GetSpeedLevels();
    protected abstract ReadOnlyCollection<float> GetRangeLevels();

    private void OnDestroy()
    {
        if (nowInTile)
            nowInTile.LeaveTile(this);
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
        rangeDisplay.localScale = new Vector3(2 * range, 2 * range);
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
        power = (attackDamage + defense) * hp;
        UpdatePowerText();
    }

    internal void OnEnemyInRange(Army enemy)
    {
        enemiesInRange.Add(enemy);
        inCombat = true;
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

    internal virtual void AttackIfAble()
    {
        if (enemiesInRange.Count == 0)
            return;

        var enemy = enemiesInRange[0];
        var damage = attackDamage * GetAttackMultiplier(enemy) * enemy.tileCombatMods.DefenceMultiplier;
        enemy.TakeDamage((int)Mathf.Ceil(damage));
    }

    protected abstract float GetAttackMultiplier(Army enemy);

    internal void TakeDamage(int damage)
    {
        int damageToTake = MIN_DAMAGE_TAKEN;
        if (damage > defense)
            damageToTake = damage - defense;
        hp -= damageToTake;
    }

    private void UpdatePowerText()
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
        GameManager.Get().OnSelectionChange(this);
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
        else if (collision.gameObject.tag == "Obstacle")
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

    public void Select()
    {
        SetShowRangeDisplay(true);
    }

    public void Deselect()
    {
        SetShowRangeDisplay(false);
    }

    public abstract string GetUpgradeDescriptor();

    internal int UpgradeCost()
    {
        return UpgradeCost(rank);
    }

    internal static int UpgradeCost(int rank)
    {
        if (rank < upgradeCostLevels.Count)
            return upgradeCostLevels[rank];
        return upgradeCostLevels.Last();
    }

    internal static int PurchaseCost(int rank)
    {
        if (rank < purchaseCostLevels.Count)
            return purchaseCostLevels[rank];
        return purchaseCostLevels.Last();
    }

    internal bool UpgradeMaxed()
    {
        return rank >= upgradeCostLevels.Count;
    }

    internal abstract bool IsType(ArmyType type);

    public void TileModsChanged(ITileCombatModifiers mods)
    {
        tileCombatMods = mods;
        if (tileCombatMods != null)
        {
            // When leaving a tile, leave the old stats to be overwritter when entered the new tile
            UpdateStats();
        }
    }
}
