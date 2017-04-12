using System;
using UnityEngine;

public class Tile : MonoBehaviour {
    private Army occupantRed;
    private Army occupantBlue;
    private GameManager gameManager;
    private Team controllingTeam = Team.Neutral;
    //private int level = 1;

    internal Team ControlledBy()
    {
        return controllingTeam;
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    internal bool IsOccupied()
    {
        return occupantRed != null || occupantBlue != null;
    }

    internal int GetScoreValue()
    {
        return 1;
    }

    internal int GetCashValue()
    {
        return 1;
    }

    public bool IsContested()
    {
        return occupantBlue != null && occupantRed != null;
    }

    internal Army GetOccupant()
    {
        if (occupantRed != null)
            return occupantRed;
        else if (occupantBlue != null)
            return occupantBlue;
        return null;
    }

    public Army GetDefender()
    {
        return occupantRed;
    }

    public Army GetAttacker()
    {
        return occupantBlue;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if(army)
        {
            if (army.GetTeam() == Team.Red)
                occupantRed = army;
            else
                occupantBlue = army;

            if (IsContested())
            {
                gameManager.AddContestedTile(this);
                army.EnteredContestedTile(new Vector3(-0.5f, -0.5f));

                occupantBlue.StopAndFight();
                occupantRed.StopAndFight();
            }
            else
            {
                ChangeControllingTeam(army.GetTeam());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if (army)
        {
            if (army.GetTeam() == Team.Red)
            {
                occupantRed = null;
                if(occupantBlue)
                {
                    occupantBlue.RemoveOffset();
                    ChangeControllingTeam(Team.Blue);
                }
            }
            else
            {
                occupantBlue = null;
                if(occupantRed)
                {
                    occupantRed.RemoveOffset();
                    ChangeControllingTeam(Team.Red);
                }
            }
        }
    }

    private void ChangeControllingTeam(Team team)
    {
        controllingTeam = team;
        var renderer = GetComponent<SpriteRenderer>();

        if (controllingTeam == Team.Red)
            renderer.color = new Color(200, 0, 0);
        else if (controllingTeam == Team.Blue)
            renderer.color = new Color(0, 0, 200);
        else if (controllingTeam == Team.Neutral)
            renderer.color = new Color(255, 255, 255);
        else
            throw new ArgumentException();
    }
}
