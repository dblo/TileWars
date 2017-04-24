using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TraversableTile : Tile
{
    private List<Army> redOccupants = new List<Army>();
    private List<Army> blueOccupants = new List<Army>();
    private Team controllingTeam = Team.Neutral;
    public TiletModifiers modifiers;
    [SerializeField]
    protected List<Sprite> sprites;

    private void Awake()
    {
        modifiers = TileModifiersFactory.Create(tileType, rank);
    }

    public bool IsContested()
    {
        return redOccupants.Count > 0 && blueOccupants.Count > 0;
    }

    internal Team ControlledBy()
    {
        return controllingTeam;
    }

    public void EnterTile(Army army)
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
        army.TileModsChanged(modifiers);
    }

    public void LeaveTile(Army army)
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
        army.TileModsChanged(null);
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
    
    internal override void Upgrade()
    {
        rank++;
        GetComponent<SpriteRenderer>().sprite = sprites[rank];
        modifiers = TileModifiersFactory.Create(tileType, rank);

        foreach (var army in blueOccupants)
        {
            army.TileModsChanged(modifiers);
        }

        foreach (var army in redOccupants)
        {
            army.TileModsChanged(modifiers);
        }
    }
}
