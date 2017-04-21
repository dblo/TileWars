using UnityEngine;

public enum TileType { Plains, Hill, Mountain, Water };

public class Tile : MonoBehaviour
{
    [SerializeField]
    private TileType tileType;

    internal TileType GetTileType()
    {
        return tileType;
    }

    private void OnMouseDown()
    {
        GameManager.Get().OnTileClicked(this);
    }
}
