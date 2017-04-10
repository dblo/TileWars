using System;
using UnityEngine;
using UnityEngine.UI;

public class Army : MonoBehaviour {
    public enum Team { Red, Blue, Neutral };
    public int attack;
    public float speed;
    public int hp;
    public float range;
    private Transform nextWaypoint;
    public Team team;
    Text textPlate;
    private string level = "1";
    private Vector3 origin = new Vector3();
    private Vector3 waypointOffset;

    private void Start()
    {
        attack = UnityEngine.Random.Range(1, 6);
        hp = UnityEngine.Random.Range(10, 20);
        waypointOffset = origin;
        textPlate = GetComponentInChildren<Text>();

        // Post-init
        UpdateText();
    }

    void FixedUpdate () {
		if(nextWaypoint != null)
        {
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint.position + waypointOffset, speed);
            transform.position = newPos;

            if (Vector2.Distance(transform.position, nextWaypoint.position) < .005)
            {
                nextWaypoint = null;
            }
        }
	}

    internal void MoveTo(Tile tile)
    {
        nextWaypoint = tile.transform;
    }

    internal Team GetTeam()
    {
        return team;
    }

    internal void Attack(Army enemy)
    {
        enemy.TakeDamage(attack);
        Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
    }

    private void TakeDamage(int damage)
    {
        hp -= damage;
        UpdateText();
    }

    internal void EnteredContestedTile(Vector3 offset)
    {
        waypointOffset = offset;
    }

    private void UpdateText()
    {
        textPlate.text = level.ToString() + " - " + hp.ToString();
    }

    internal bool Alive()
    {
        return hp > 0;
    }

    internal void RemoveOffset()
    {
        // Move to center of curr tile to negate offset
        if(waypointOffset != origin)
        {
            nextWaypoint.position = transform.position - waypointOffset;
            waypointOffset = origin;
        }
    }
}
