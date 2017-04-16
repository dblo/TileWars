using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };

public class Army : MonoBehaviour {
    [SerializeField]
    protected Team team;
    private Text powerText;
    private bool inCombat;
    private List<Army> enemiesInRange = new List<Army>();
    private List<Vector2> currentTravelPath = new List<Vector2>();
    private List<Army> collidingEnemies = new List<Army>();
    private Transform rangeDisplay;

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
    }

    private void Start()
    {
        powerText = GetComponentInChildren<Text>();
        // Post-init
        UpdatePower();
        OnRangeChanged();
        UpdateText();
    }

    //private void Update()
    //{
    //    OnRangeChanged();
    //}

    protected virtual void OnRangeChanged()
    {
        var rangeManager = GetComponentInChildren<ArmyRangeManager>();
        var coll = rangeManager.GetComponent<CircleCollider2D>();
        coll.radius = range;

        rangeDisplay.localScale = new Vector3(range, range);
    }

    internal void RandomizeStats()
    {
        attackDamage = UnityEngine.Random.Range(1, 6);
        armySize = UnityEngine.Random.Range(5, 15);
        speed = UnityEngine.Random.Range(0.01f, 0.04f);
        range = UnityEngine.Random.Range(0.5f, 1f);
        UpdatePower();
    }

    internal void SetTeam(Team newTeam)
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

    internal void TakeDamage(Army army)
    {
        armySize -= army.attackDamage;
        UpdatePower();
    }

    internal string GetPower()
    {
        return power.ToString();
    }

    internal bool IsAlive()
    {
        return armySize > 0;
    }

    internal float GetSpeed()
    {
        return speed;
    }

    internal void OnEnemyInRange(Army enemy)
    {
        enemiesInRange.Add(enemy);
        inCombat = true;
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

    private Vector2 NextWaypoint()
    {
        if (currentTravelPath.Count == 0)
            throw new System.ArgumentException();
        return currentTravelPath[0];
    }

    void FixedUpdate () {
		if(currentTravelPath.Count > 0 && collidingEnemies.Count == 0)
        {
            var newPos = Vector2.MoveTowards(transform.position, NextWaypoint(), GetSpeed());
            transform.position = newPos;
            if (Vector2.Distance(transform.position, NextWaypoint()) < .05)
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

    internal Team GetTeam()
    {
        return team;
    }

    internal void DoCombat()
    {
        if(enemiesInRange.Count > 0)
        {
            Attack(enemiesInRange[0]);
        }
    }

    private void Attack(Army enemy)
    {
        enemy.TakeDamage(this);
        enemy.UpdateText();
        //Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
    }
    
    private void UpdateText()
    {
        powerText.text = GetPower();
    }

    internal void Stop()
    {
        currentTravelPath.Clear();
        //if (currentTravelPath.Count > 0)
            //currentTravelPath.RemoveAt(0);
    }

    protected virtual void OnMouseDown()
    {
        GameManager.Get().OnArmyClicked(this);

    }

    public virtual void GiveNewPath(List<Vector2> swipePath)
    {
        currentTravelPath = swipePath;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if(army != null && IsEnemy(army))
        {
            collidingEnemies.Add(army);
            return;
        }
        if (collision.gameObject.name == "Wall(Clone)") // TODO improve
        {
            Stop();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if (army != null && IsEnemy(army))
        {
            collidingEnemies.Remove(army);
            OnEnemyOutOfRange(army);// TODO ???
        }
    }

    public virtual void SetShowRangeDisplay(bool val)
    {
        rangeDisplay.gameObject.SetActive(val);
    }
}
