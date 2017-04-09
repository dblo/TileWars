using UnityEngine;

public class Tile : MonoBehaviour {
    public Army occupant;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal bool IsOccupied()
    {
        return occupant != null;
    }

    public void AddOccupant(Army army)
    {
        occupant = army;
    }

    internal Army GetOccupant()
    {
        return occupant;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if(army)
        {
            occupant = army;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if (army)
        {
            occupant = null;
        }
    }
}
