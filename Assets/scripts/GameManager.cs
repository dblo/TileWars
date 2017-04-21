using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private Army selectedArmy;
    private int logicCounter;
    private const int LOGIC_TICKS = 50;
    private Player p1;
    private Player p2;
    private List<Vector2> swipePath = new List<Vector2>();
    private float nextMousePoll;
    private const float MOUSE_POLL_RATE = 0.1f;
    private const float MIN_SWIPE_TIME = 0.2f;
    private float swipeStartTime = -1;

    private static GameManager instance = null;

    private void Awake()
    {
        if (instance != null)
            throw new System.Exception("GameManager can nly exist on a single GameObject");

        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Is this reliable if using callied from other script's Awake()?
    public static GameManager Get()
    {
        if (instance == null)
            throw new System.NullReferenceException();
        return instance;
    }

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        p1 = GameObject.Find("Player").GetComponent<Player>();

        var p2GO = GameObject.Find("AIPlayer");
        if(p2GO != null)
            p2 = p2GO.GetComponent<Player>();

        nextMousePoll = Time.time;
    }

    private void Update()
    {
        if(nextMousePoll <= Time.time)
        {
            nextMousePoll = Time.time + MOUSE_POLL_RATE;
            HandleMouseGesture();
        }
    }

    private void FixedUpdate()
    {
        if (logicCounter <= 0)
        {
            logicCounter = LOGIC_TICKS;

            if(p2 && p1)
            {
                RunCombatLogic();
                UpdateCashScore();
            }
        }
        else
        {
            logicCounter--;
        }
    }

    private void UpdateCashScore()
    {
        var tiles = gameBoard.GetTraversableTiles();
        foreach (TraversableTile tile in tiles)
        {
            switch (tile.ControlledBy())
            {
                case Team.Red:
                    p2.AddCash(tile.GetCashValue());
                    p2.AddScore(tile.GetScoreValue());
                    break;
                case Team.Blue:
                    p1.AddCash(tile.GetCashValue());
                    p1.AddScore(tile.GetScoreValue());
                    break;
            }
        }
        p1.UpdateUI();
        p2.UpdateUI();
    }

    private void RunCombatLogic()
    {
        foreach (var army in p2.GetArmies())
        {
            if(army.IsInCombat())
            {
                army.DoCombat();
            }
        }
        foreach (var army in p1.GetArmies())
        {
            if (army.IsInCombat())
            {
                army.DoCombat();
            }
        }

        List<Army> armiesPendingRemoval = new List<Army>();
        foreach (var army in p2.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
            }
        }
        p1.OnEnemiesKilled(armiesPendingRemoval);
        p2.KillArmies(armiesPendingRemoval);
        armiesPendingRemoval.Clear();

        foreach (var army in p1.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
            }
        }
        p2.OnEnemiesKilled(armiesPendingRemoval);
        p1.KillArmies(armiesPendingRemoval);
        armiesPendingRemoval.Clear();
    }

    internal void OnArmyClicked(Army army)
    {
        if (selectedArmy == army)
            return;

        if (selectedArmy)
        {
            ClearArmySelection();
        }
        selectedArmy = army;
        selectedArmy.SetShowRangeDisplay(true);
    }

    internal void OnTileClicked(Tile tile)
    {
        if (ArmySelected())
        {
            ClearArmySelection();
        }
    }

    internal void OnHQClicked()
    {
        if (selectedArmy)
        {
            ClearArmySelection();
        }
    }

    private bool ArmySelected()
    {
        return selectedArmy != null;
    }

    private void ClearArmySelection()
    {
        selectedArmy.SetShowRangeDisplay(false);
        selectedArmy = null;
    }

    void HandleMouseGesture()
    {
        if (selectedArmy == null)
            return;

        if (Input.GetMouseButton(0))
        {
            if (swipeStartTime < 0)
            {
                swipeStartTime = Time.time;
            }
            swipePath.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        else if(swipeStartTime > 0)
        {
            if(swipeStartTime + MIN_SWIPE_TIME <= Time.time)
            {
                selectedArmy.ChangeTravelPath(swipePath);
            }
            swipePath = new List<Vector2>();
            swipeStartTime = -1;
            return;
        }
    }
}
