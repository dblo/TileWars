using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { Plains, Hill, Mountain };

public class Tile : MonoBehaviour {
    private List<Army> redOccupants = new List<Army>();
    private List<Army> blueOccupants = new List<Army>();
    private Team controllingTeam = Team.Neutral;
    [SerializeField]
    private TileType tileType;

    internal int GetScoreValue()
    {
        return 1;
    }

    internal int GetCashValue()
    {
        return 1;
    }

    internal TileType GetTileType()
    {
        return tileType;
    }

    public bool IsContested()
    {
        return redOccupants.Count > 0 && blueOccupants.Count > 0;
    }

    internal Team ControlledBy()
    {
        return controllingTeam;
    }

    public void AddOccupant(Army army)
    {
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

    public void RemoveOccupant(Army army)
    {
        if (army.GetTeam() == Team.Red)
        {
            redOccupants.Remove(army);
            if (!IsContested() && blueOccupants.Count > 0)
            {
                ChangeControllingTeam(Team.Blue);
            }
        }
        else
        {
            blueOccupants.Remove(army);
            if (!IsContested() && redOccupants.Count > 0)
            {
                ChangeControllingTeam(Team.Red);
            }
        }
    }

    private void OnMouseDown()
    {
        GameManager.Get().OnTileClicked(this);
    }

    private void ChangeControllingTeam(Team team)
    {
        controllingTeam = team;
        var renderer = GetComponent<SpriteRenderer>();

        if (controllingTeam == Team.Red)
            renderer.color = new Color(100, 0, 0);
        else if (controllingTeam == Team.Blue)
            renderer.color = new Color(0, 0, 100);
        else if (controllingTeam == Team.Neutral)
            renderer.color = new Color(255, 255, 255);
        else
            throw new ArgumentException();
    }
}
