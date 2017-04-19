using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };

public class Army : MonoBehaviour {
    private const double REACHED_WAYPOINT_DISTANCE = .05;
    [SerializeField]
    protected Team team;
    private Text powerText;
    private bool inCombat;
    private List<Army> enemiesInRange = new List<Army>();
    private List<Army> collidingEnemies = new List<Army>();
    private List<Vector2> currentTravelPath = new List<Vector2>();
    protected Transform rangeDisplay;

    #region Combat Stats
    private int power;
    [SerializeField]
    protected int attackDamage = 10;
    [SerializeField]
    private int armySize = 100;
    [SerializeField]
    private float speed = 0.03f;
    [SerializeField]
    protected float range = 0.5f;
    #endregion

    #region Getters/Setters/Predicates
    internal bool IsAlive()
    {
        return armySize > 0;
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

    public virtual void SetShowRangeDisplay(bool val)
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
            if(trans.name == "RangeDisplay")
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
        OnRangeChanged(range);
        UpdateText();
    }

    protected virtual void OnRangeChanged(float aRange)
    {
        var coll = transform.Find("RangeManager").GetComponent<CircleCollider2D>();
        coll.radius = aRange;
        rangeDisplay.localScale = new Vector3(aRange, aRange);
    }

    internal void RandomizeStats()
    {
        attackDamage = UnityEngine.Random.Range(1, 6);
        armySize = UnityEngine.Random.Range(5, 15);
        speed = UnityEngine.Random.Range(0.01f, 0.04f);
        range = UnityEngine.Random.Range(0.5f, 1f);
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
        power = armySize * attackDamage;
    }

    internal void TakeDamage(int damage)
    {
        armySize -= damage;
        UpdatePower();
    }

    internal void OnEnemyInRange(Army enemy)
    {
        enemiesInRange.Add(enemy);
        inCombat = true;
    }

    void FixedUpdate ()
    {
        if (currentTravelPath.Count > 0 && collidingEnemies.Count == 0)
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

    internal void MoveTo(Tile tile)
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
        if(enemiesInRange.Count > 0)
        {
            Assert.IsTrue(enemiesInRange[0] != null);
            Attack(enemiesInRange[0]);
        }
    }

    protected virtual void Attack(Army enemy)
    {
        enemy.TakeDamage(attackDamage);
        enemy.UpdateText();
        //Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
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
        GameManager.Get().OnArmyClicked(this);
    }

    public virtual void ChangeTravelPath(List<Vector2> swipePath)
    {
        currentTravelPath = swipePath;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var army = collision.collider.GetComponent<Army>();
        if(army != null && IsEnemy(army))
        {
            collidingEnemies.Add(army);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Wall(Clone)") // TODO improve
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
}
