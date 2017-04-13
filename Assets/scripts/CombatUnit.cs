using UnityEngine;

public class CombatUnit //: MonoBehaviour
{
    [SerializeField]
    private int power;
    [SerializeField]
    private int attackDamage = 10;
    [SerializeField]
    private int armySize = 100;
    [SerializeField]
    private float speed = 0.03f;
    [SerializeField]
    private float range = 1;

    internal CombatUnit()
    {
        UpdatePower();
    }

    internal void RandomizeStats()
    {
        attackDamage = UnityEngine.Random.Range(1, 6);
        armySize = UnityEngine.Random.Range(5, 15);
        speed = 0.03f;// UnityEngine.Random.Range(0.01f, 0.1f);
        range = UnityEngine.Random.Range(0.5f, 2);
        UpdatePower();
    }

    private void UpdatePower()
    {
        power = armySize * attackDamage;
    }

    internal void TakeDamage(CombatUnit combatUnit)
    {
        armySize -= combatUnit.attackDamage;
        UpdatePower();
    }

    internal string GetPower()
    {
        return power.ToString();
    }

    internal bool IsAlive()
    {
        return armySize > 0;
    }

    internal float GetSpeed()
    {
        return speed;
    }
}
