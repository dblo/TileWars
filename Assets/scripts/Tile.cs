using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    private List<Army> redOccupants = new List<Army>();
    private List<Army> blueOccupants = new List<Army>();
    private GameManager gameManager;
    private Team controllingTeam = Team.Neutral;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
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
        return redOccupants.Count > 0 && blueOccupants.Count > 0;
    }

    internal Team ControlledBy()
    {
        return controllingTeam;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var armyRange = collision.GetComponent<ArmyRangeManager>();
        if(armyRange)
        {
            var army = armyRange.GetArmy();
            if (army.GetTeam() == Team.Red)
                redOccupants.Add(army);
            else
                blueOccupants.Add(army);

            if (IsContested())
            {
                //gameManager.AddContestedTile(this);
            }
            else
            {
                ChangeControllingTeam(army.GetTeam());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var armyRange = collision.GetComponent<ArmyRangeManager>();
        if (armyRange)
        {
            var army = armyRange.GetArmy();
            if (army.GetTeam() == Team.Red)
            {
                redOccupants.Remove(army);
                if(blueOccupants.Count > 0)
                {
                    ChangeControllingTeam(Team.Blue);
                }
            }
            else
            {
                blueOccupants.Remove(army);
                if(redOccupants.Count > 0)
                {
                    ChangeControllingTeam(Team.Red);
                }
            }
        }
    }

    private void OnMouseDown()
    {
        gameManager.OnTileClicked(this);
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
