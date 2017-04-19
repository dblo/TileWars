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
        var enemy = collision.GetComponent<ArmyRangeManager>();
        if(enemy != null && army.IsEnemy(enemy.army)) {
            army.OnEnemyInRange(enemy.army);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<ArmyRangeManager>();
        if (enemy != null && army.IsEnemy(enemy.army))
        {
            army.OnEnemyOutOfRange(enemy.army);
        }
    }
}
