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
    private Text scoreText;
    private Text cashText;
    private int redCash;
    private int blueCash;
    private int redScore;
    private int blueScore;
    private const int LOGIC_TICKS = 50;

    Player bluePlayer;
    Player redPlayer;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        // Copy scale and pos from gameBoard so that the GameManager collider covers the entire board
        transform.localScale = new Vector3(gameBoard.GetColsCount(), gameBoard.GetRowsCount());
        transform.position = new Vector3(gameBoard.GetColsCount() / 2f, gameBoard.GetRowsCount() / 2f, -1);
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        cashText = GameObject.Find("CashText").GetComponent<Text>();
        bluePlayer = GameObject.Find("Player").GetComponent<Player>();
        redPlayer = GameObject.Find("AIPlayer").GetComponent<Player>();
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
            RunCombatLogic();
            UpdateCashScore();
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
                    redCash += tile.GetCashValue();
                    redScore += tile.GetScoreValue();
                    break;
                case Team.Blue:
                    blueCash += tile.GetCashValue();
                    blueScore += tile.GetScoreValue();
                    break;
            }
        }
        UpdateCashText();
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "$R:B " + redScore + ":" + blueScore;
    }

    private void UpdateCashText()
    {
        cashText.text = "!R:B " + redCash + ":" + blueCash;
    }

    private void RunCombatLogic()
    {
        //if (contestedTiles.Count == 0)
        //    return;
                
        foreach (var army in redPlayer.GetArmies())
        {
            if(army.IsInCombat())
            {
                army.DoCombat();
            }
        }
        foreach (var army in bluePlayer.GetArmies())
        {
            if (army.IsInCombat())
            {
                army.DoCombat();
            }
        }

        List<Army> armiesPendingRemoval = new List<Army>();
        foreach (var army in redPlayer.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
            }
        }

        redPlayer.KillArmy(armiesPendingRemoval);
        armiesPendingRemoval.Clear();

        foreach (var army in bluePlayer.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
            }
        }
        bluePlayer.KillArmy(armiesPendingRemoval);
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
