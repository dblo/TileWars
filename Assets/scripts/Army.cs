using System;
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
    private Vector2 invalid = new Vector2(-1, -1);
    private Vector2 waypointOffset;
    private bool inCombat;

    private void Start()
    {
        waypointOffset = Vector2.zero;
        textPlate = GetComponentInChildren<Text>();
        nextWaypoint = invalid;

        if (randomizeStats)
        {
            attack = UnityEngine.Random.Range(1, 6);
            hp = UnityEngine.Random.Range(1, 15);
        }

        // Post-init
        UpdateText();
    }

    internal bool IsStationary()
    {
        return nextWaypoint == invalid;
    }

    internal bool IsInCombat()
    {
        return inCombat;
    }

    void FixedUpdate () {
		if(nextWaypoint != invalid)
        {
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint + waypointOffset, speed);
            transform.position = newPos;

            if (Vector2.Distance(transform.position, nextWaypoint) < .005)
            {
                nextWaypoint = invalid;
            }
        }
	}

    internal void MoveTo(Tile tile)
    {
        nextWaypoint = tile.transform.position;
    }

    internal Team GetTeam()
    {
        return team;
    }

    internal void Attack(Army enemy)
    {
        enemy.TakeDamage(attack);
        //Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
    }

    private void TakeDamage(int damage)
    {
        hp -= damage;
        UpdateText();
    }

    internal void EnteredContestedTile(Vector2 offset)
    {
        waypointOffset = offset;
    }

    private void UpdateText()
    {
        textPlate.text = hp.ToString();
    }

    internal bool Alive()
    {
        return hp > 0;
    }

    internal void RemoveOffset()
    {
        // Move to center of curr tile to negate offset
        nextWaypoint = (Vector2) transform.position - waypointOffset;
        waypointOffset = Vector2.zero;
    }

    internal void StopAndFight()
    {
        nextWaypoint = invalid;
        inCombat = true;
    }
}
