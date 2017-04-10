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
    private Transform nextWaypoint;
    private Text textPlate;
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
        //Debug.Log(team + " attacked: " + attack + "dmg. " + enemy.team + " hp: " + enemy.hp);
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
        textPlate.text = hp.ToString();
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
