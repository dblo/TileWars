using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private Army selectedArmy;
    //private List<Tile> contestedTiles = new List<Tile>();
    private int logicCounter;
    private const int LOGIC_TICKS = 50;
    private Player p1;
    private Player p2;
    private List<Vector2> swipePath = new List<Vector2>();
    private float nextMousePoll;
    private const float MOUSE_POLL_RATE = 0.1f;
    private const float MIN_SWIPE_TIME = 0.2f;
    private float swipeStartTime = -1;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        // Copy scale and pos from gameBoard so that the GameManager collider covers the entire board
        transform.localScale = new Vector3(gameBoard.GetColsCount(), gameBoard.GetRowsCount());
        transform.position = new Vector3(gameBoard.GetColsCount() / 2f, gameBoard.GetRowsCount() / 2f, -1);
        p1 = GameObject.Find("Player").GetComponent<Player>();

        var p2GO = GameObject.Find("AIPlayer");
        if(p2GO != null)
            p2 = p2GO.GetComponent<Player>();

        nextMousePoll = Time.time;
    }

    //public void AddContestedTile(Tile tile)
    //{
    //    contestedTiles.Add(tile);
    //}

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
        var tiles = gameBoard.GetTiles();
        foreach (Tile tile in tiles)
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
        //if (contestedTiles.Count == 0)
        //    return;
                
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

        p2.KillArmy(armiesPendingRemoval);
        armiesPendingRemoval.Clear();

        foreach (var army in p1.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
            }
        }
        p1.KillArmy(armiesPendingRemoval);
    }

    internal void OnArmyClicked(Army army)
    {
        selectedArmy = army;
    }

    internal void OnTileClicked(Tile tile)
    {
        if (selectedArmy != null)
        {
            var worldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //selectedArmy.MoveTo(worldCoord);
            selectedArmy = null;
        }
        else
            ClearSelection();
    }

    private void ClearSelection()
    {
        selectedArmy = null;
    }

    //private bool ObjectSelected()
    //{
    //    return selectedArmy != null;
    //}

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
                selectedArmy.GiveNewPath(swipePath);
            }
            swipePath = new List<Vector2>();
            swipeStartTime = -1;
            return;
        }
    }
}
