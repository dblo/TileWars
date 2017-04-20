using UnityEngine;

public class ArmyRangeManager : MonoBehaviour {
    private Army army;

	void Awake () {
        army = transform.GetComponentInParent<Army>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<Army>();
        if(enemy != null && army.IsEnemy(enemy)) {
            army.OnEnemyInRange(enemy);
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var enemy = collision.GetComponent<Army>();
        if (enemy != null && army.IsEnemy(enemy))
        {
            army.OnEnemyOutOfRange(enemy);
            return;
        }
    }
}
