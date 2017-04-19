using UnityEngine;

public class ArmyRangeManager : MonoBehaviour {
    private Army parent;

	void Awake () {
        parent = transform.parent.GetComponent<Army>();
	}

    internal Army GetArmy()
    {
        return parent;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<ArmyRangeManager>();
        if(enemy != null && parent.IsEnemy(enemy.parent)) {
            parent.OnEnemyInRange(enemy.parent);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<ArmyRangeManager>();
        if (enemy != null && parent.IsEnemy(enemy.parent))
        {
            parent.OnEnemyOutOfRange(enemy.parent);
        }
    }
}
