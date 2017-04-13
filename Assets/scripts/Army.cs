using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Team { Red, Blue, Neutral };

public class Army : MonoBehaviour {
    [SerializeField]
    private int attack;
    [SerializeField]
    private float speed;
    [SerializeField]
    private int hp;
    [SerializeField]
    private float range;
    [SerializeField]
    private Team team;
    [SerializeField]
    bool randomizeStats;
    private Vector2 nextWaypoint;
    private Text textPlate;
    private Vector2 invalidPos = new Vector2(-1, -1);
    private bool inCombat;
    private GameManager gameManager;
    private List<Army> enemiesInRange = new List<Army>();

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (randomizeStats)
        {
            attack = UnityEngine.Random.Range(1, 6);
            hp = UnityEngine.Random.Range(1, 15);
        }
    }

    private void Start()
    {
        textPlate = GetComponentInChildren<Text>();
        nextWaypoint = invalidPos;
        // Post-init
        UpdateText();
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
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint, speed);
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
        enemy.TakeDamage(attack);
        //Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
    }

    private void TakeDamage(int damage)
    {
        hp -= damage;
        UpdateText();
    }

    private void UpdateText()
    {
        textPlate.text = hp.ToString();
    }

    internal bool IsAlive()
    {
        return hp > 0;
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
        if (collision.gameObject.name == "Wall(Clone)")
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
