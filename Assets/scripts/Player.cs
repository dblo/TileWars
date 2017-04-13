using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    protected Vector2 armySpawnPoint;
    [SerializeField]
    protected int maxArmyCount;
    [SerializeField]
    protected GameObject armyPrefab;
    protected List<Army> armies;

    virtual protected void Start()
    {
        armies = new List<Army>(maxArmyCount);
        SpawnArmies();
    }

    public ReadOnlyCollection<Army> GetArmies()
    {
        return armies.AsReadOnly();
    }

    private void SpawnArmies()
    {
        for (int i = 0; i < maxArmyCount; i++)
        {
            SpawnArmy();
        }
    }

    protected void SpawnArmy()
    {
        var newArmy = Instantiate(armyPrefab, transform).GetComponent<Army>();
        newArmy.transform.position = armySpawnPoint;
        armies.Add(newArmy);
    }

    internal void KillArmy(List<Army> toRemove)
    {
        armies = armies.Except(toRemove).ToList();
        foreach (var army in toRemove)
        {
            Destroy(army.gameObject);
        }
    }
}
