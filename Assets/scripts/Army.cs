﻿using System;
using UnityEngine;

public class Army : MonoBehaviour {
    public enum Team { Red, Blue, Neutral };
    public int attack;
    public float speed;
    public int hp;
    public float range;
    private Transform nextWaypoint;
    public Team team;

    void FixedUpdate () {
		if(nextWaypoint != null)
        {
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint.position, speed);
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
        enemy.hp -= attack;
    }

    internal bool Alive()
    {
        return hp > 0;
    }
}
