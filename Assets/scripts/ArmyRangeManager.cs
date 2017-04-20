using UnityEngine;

public class ArmyRangeManager : MonoBehaviour {
    private Army army;

	void Awake () {
        army = transform.GetComponent<Army>();
	}

    internal Army GetArmy()
    {
        return army;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tile = collision.GetComponent<Tile>();
        if (tile != null)
        {
            tile.AddOccupant(army);
            army.OnEnteredTile(tile);
            return;
        }

        var enemy = collision.GetComponent<ArmyRangeManager>();
        if(enemy != null && army.IsEnemy(enemy.army)) {
            army.OnEnemyInRange(enemy.army);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var tile = collision.GetComponent<Tile>();
        if (tile != null)
        {
            tile.RemoveOccupant(army);
            army.OnExitedTile(tile);
            return;
        }

        var enemy = collision.GetComponent<ArmyRangeManager>();
        if (enemy != null && army.IsEnemy(enemy.army))
        {
            army.OnEnemyOutOfRange(enemy.army);
            return;
        }
    }
}
