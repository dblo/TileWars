using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private Army selectedArmy;
    private List<Tile> contestedTiles = new List<Tile>();
    private int logicCounter;
    private const int LOGIC_TICKS = 50;
    Player p1;
    Player p2;

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
    }

    //public void AddContestedTile(Tile tile)
    //{
    //    contestedTiles.Add(tile);
    //}

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
            selectedArmy.MoveTo(worldCoord);
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
}
