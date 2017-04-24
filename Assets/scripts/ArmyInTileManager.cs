using UnityEngine;

public class ArmyInTileManager : MonoBehaviour {
    Army parent;
    private void Awake()
    {
        parent = GetComponentInParent<Army>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tile = collision.GetComponent<TraversableTile>();
        if (tile != null)
        {
            parent.OnEnteredTile(tile);
        }
    }
}
