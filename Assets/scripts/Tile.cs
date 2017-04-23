using System;
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
        GameManager.Get().OnSelection(this);
    }

    public string GetUpgradeDescriptor()
    {
        switch (tileType)
        {
            case TileType.Plain:
                return "P" + (rank + 1) + "-$" + UpgradeCost();
            case TileType.Hill:
                return "H" + (rank + 1) + "-$" + UpgradeCost();
            case TileType.Mine:
                return "M" + (rank + 1) + "-$" + UpgradeCost();
        }
        throw new ArgumentException();
    }

    internal virtual void Upgrade()
    {
        throw new NotImplementedException();
    }

    internal bool IsMaxRank()
    {
        return rank >= MAX_TILE_RANK;
    }
}
