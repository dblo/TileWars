using UnityEngine;

public class Tile : MonoBehaviour {
    private Army occupantRed;
    private Army occupantBlue;
    private GameManager gameManager;
    private Army.Team controllingTeam = Army.Team.Neutral;

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
        return occupantBlue != null && occupantRed != null;
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
                gameManager.AddContestedTile(this);
                army.EnteredContestedTile(new Vector3(-0.5f, -0.5f));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var army = collision.GetComponent<Army>();
        if (army)
        {
            if (army.GetTeam() == Army.Team.Red)
            {
                occupantRed = null;
                if(occupantBlue)
                {
                    occupantBlue.RemoveOffset();
                }
            }
            else
            {
                occupantBlue = null;
                if(occupantRed)
                {
                    occupantRed.RemoveOffset();
                }
            }
        }
    }
}
