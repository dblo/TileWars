﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileType { Plain, Hill, Mine, Mountain, Water };

public class Tile : MonoBehaviour, ISelectableObject
{
    public const int MAX_TILE_RANK = 3;
    [SerializeField]
    protected TileType tileType;
    protected int rank = 0;
    
    private static List<int> upgradeCostLevels = new List<int> { 100, 500, 1000 };

    public void Select()
    {
        //Activate selected overlay
    }

    public void Deselect()
    {
        //deactivate selected overlay
    }

    internal int UpgradeCost()
    {
        if (rank < upgradeCostLevels.Count)
            return upgradeCostLevels[rank];
        return upgradeCostLevels.Last();
    }

    internal TileType GetTileType()
    {
        return tileType;
    }

    private void OnMouseDown()
    {
        GameManager.Get().OnSelectionChange(this);
    }

    public string GetUpgradeDescriptor()
    {
        if (UpgradeMaxed())
            return null;

        switch (tileType)
        {
            case TileType.Plain:
                return "Pla $" + UpgradeCost();
            case TileType.Hill:
                return "Hil $" + UpgradeCost();
            case TileType.Mine:
                return "Min $" + UpgradeCost();
        }
        throw new ArgumentException();
    }

    internal virtual void Upgrade()
    {
        throw new NotImplementedException();
    }

    internal bool UpgradeMaxed()
    {
        return rank >= MAX_TILE_RANK;
    }
}
