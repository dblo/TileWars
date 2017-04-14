using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };

public class Army : MonoBehaviour {
    [SerializeField]
    private Team team;
    private Vector2 nextWaypoint;
    private Text powerText;
    private Vector2 invalidPos = new Vector2(-1, -1);
    private bool inCombat;
    private GameManager gameManager;
    private List<Army> enemiesInRange = new List<Army>();

    #region Combat Stats
    private int power;
    [SerializeField]
    private int attackDamage = 10;
    [SerializeField]
    private int armySize = 100;
    [SerializeField]
    private float speed = 0.03f;
    [SerializeField]
    private float range = 0.5f;
    #endregion

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        powerText = GetComponentInChildren<Text>();
        nextWaypoint = invalidPos;
        // Post-init
        UpdatePower();
        OnRangeChanged();
        UpdateText();
    }

    //private void Update()
    //{
    //    OnRangeChanged();
    //}

    private void OnRangeChanged()
    {
        var rangeManager = GetComponentInChildren<ArmyRangeManager>();
        var coll = rangeManager.GetComponent<CircleCollider2D>();
        coll.radius = range;
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
            renderer.color = new Color(255, 0, 0);
        else if (team == Team.Blue)
            renderer.color = new Color(0, 0, 255);
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
        return nextWaypoint == invalidPos;
    }

    internal bool IsInCombat()
    {
        return inCombat;
    }

    void FixedUpdate () {
		if(nextWaypoint != invalidPos)
        {
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint, GetSpeed());
            transform.position = newPos;
            if (Vector2.Distance(transform.position, nextWaypoint) < .005)
            {
                nextWaypoint = invalidPos;
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
        nextWaypoint = tile.transform.position;
    }

    internal void MoveTo(Vector3 worldCoord)
    {
        nextWaypoint = worldCoord;
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
        nextWaypoint = invalidPos;
    }

    private void OnMouseDown()
    {
        gameManager.OnArmyClicked(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if(army != null && IsEnemy(army))
        {
            Stop();
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
            OnEnemyOutOfRange(army);
        }
    }
}
