using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private Army selectedObject;
    private List<Tile> contestedTiles = new List<Tile>();
    private int logicCounter;
    private Text scoreText;
    private Text cashText;
    private int redCash;
    private int blueCash;
    private int redScore;
    private int blueScore;
    private const int LOGIC_TICKS = 50;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        // Copy scale and pos from gameBoard so that the GameManager collider covers the entire board
        transform.localScale = new Vector3(gameBoard.GetColsCount(), gameBoard.GetRowsCount());
        transform.position = new Vector3(gameBoard.GetColsCount() / 2f, gameBoard.GetRowsCount() / 2f, -1);
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        cashText = GameObject.Find("CashText").GetComponent<Text>();
    }

    public void AddContestedTile(Tile tile)
    {
        contestedTiles.Add(tile);
    }

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
        if (contestedTiles.Count == 0)
            return;

        List<Tile> tilesPendingRemoval = new List<Tile>();
        foreach (Tile tile in contestedTiles)
        {
            var attacker = tile.GetAttacker();
            var defender = tile.GetDefender();

            attacker.Attack(defender);
            if (defender.Alive())
            {
                defender.Attack(attacker);
                if (!attacker.Alive())
                {
                    tilesPendingRemoval.Add(tile);
                    KillArmy(attacker);
                }
            }
            else
            {
                tilesPendingRemoval.Add(tile);
                KillArmy(defender);
            }
        }
        contestedTiles = contestedTiles.Except(tilesPendingRemoval).ToList();
    }

    private void KillArmy(Army army)
    {
        Destroy(army.gameObject);
    }

    private void OnMouseDown()
    {
        var tile = gameBoard.GetTile(Input.mousePosition);
        if (ObjectSelected())
        {
            MoveSelected(tile);
            ClearSelection();
        }
        else if (tile.IsOccupied())
        {
            selectedObject = tile.GetOccupant();
        }
    }

    private void ClearSelection()
    {
        selectedObject = null;
    }

    private bool ObjectSelected()
    {
        return selectedObject != null;
    }

    private void MoveSelected(Tile tile)
    {
        if (selectedObject != null)
        {
            var army = selectedObject.GetComponent<Army>();
            if (army != null)
            {
                army.MoveTo(tile);
            }
        }
    }
}
