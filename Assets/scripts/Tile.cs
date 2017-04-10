using UnityEngine;

public class Tile : MonoBehaviour {
    private Army occupantRed;
    private Army occupantBlue;
    private bool contested = false;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    internal bool IsOccupied()
    {
        return occupantRed != null || occupantBlue != null;
    }

    public bool IsContested()
    {
        return contested;
    }

    public void AddOccupant(Army army)
    {
        occupantRed = army;
    }

    internal Army GetOccupant()
    {
        if (occupantRed != null)
            return occupantRed;
        else if (occupantBlue != null)
            return occupantBlue;
        return null;
    }

    public Army GetDefender()
    {
        return occupantRed;
    }

    public Army GetAttacker()
    {
        return occupantBlue;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if(army)
        {
            if (army.GetTeam() == Army.Team.Red)
                occupantRed = army;
            else
                occupantBlue = army;

            if (occupantBlue != null && occupantRed != null)
            {
                contested = true;
                gameManager.AddContestedTile(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if (army)
        {
            if (army.GetTeam() == Army.Team.Red)
                occupantRed = null;
            else
                occupantBlue = null;

            if (occupantBlue == null || occupantRed == null)
                contested = false;
        }
    }
}
