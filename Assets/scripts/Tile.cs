using System;
using UnityEngine;

public enum TileType { Plains, Hill, Mountain, Water };

public class Tile : MonoBehaviour, ISelectableObject
{
    [SerializeField]
    private TileType tileType;

    public void Select()
    {
        //Activate selected overlay
    }

    public void Deselect()
    {
        //deactivate selected overlay
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
            case TileType.Plains:
                break;
            case TileType.Hill:
                break;
            case TileType.Mountain:
                break;
            case TileType.Water:
                break;
            default:
                break;
        }
        return "";
    }
}
