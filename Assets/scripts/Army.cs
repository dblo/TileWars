using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour {
    public int attack;
    public float speed;
    public int hp;
    public float range;
    private Transform nextWaypoint;
	
	void FixedUpdate () {
		if(nextWaypoint != null)
        {
            var newPos = Vector2.MoveTowards(transform.position, nextWaypoint.position, speed);
            transform.position = newPos;

            if (Vector2.Distance(transform.position, nextWaypoint.position) < .005)
                nextWaypoint = null;
        }
	}

    internal void MoveTo(Tile tile)
    {
        nextWaypoint = tile.transform;
    }
}
