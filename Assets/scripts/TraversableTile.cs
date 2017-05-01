using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TraversableTile : Tile
{
    private List<Army> redOccupants = new List<Army>();
    private List<Army> blueOccupants = new List<Army>();
    [SerializeField]
    private Team controllingTeam = Team.Neutral;
    public TiletModifiers modifiers;
    [SerializeField]
    protected List<Sprite> sprites;
    private bool isVisible;
    public GameObject fog;

    public static readonly Color GREEN = new Color(35 / 255f, 175 / 255f, 76 / 255f);
    public static readonly Color RED = new Color(255 / 255f, 0, 0);
    public static readonly Color BLUE = new Color(0, 0, 255 / 255f);

    private void Awake()
    {
        modifiers = TileModifiersFactory.Create(tileType, rank);
        ChangeControllingTeam(controllingTeam);
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
            if (controllingTeam != army.GetTeam())
            {
                Team formedController = controllingTeam;
                ChangeControllingTeam(army.GetTeam());
                GameManager.Get().OnTileControlChanged(this, formedController);
            }
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
            renderer.color = RED;
        else if (controllingTeam == Team.Blue)
            renderer.color = BLUE;
        else if (controllingTeam == Team.Neutral)
            renderer.color = GREEN;
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

    internal void SetVisible(bool aIsVisible)
    {
        isVisible = aIsVisible;
        var renderer = fog.GetComponent<SpriteRenderer>();
        var color = renderer.color;
        color.a = aIsVisible ? 0 : 0.5f;
        renderer.color = color;
    }

    internal bool GetVisible()
    {
        return isVisible;
    }
}
