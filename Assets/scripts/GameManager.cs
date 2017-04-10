using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
    GameBoard gameBoard;
    Army selectedObject;
    List<Tile> contestedTiles = new List<Tile>();
    private int logicCounter;
    private const int LOGIC_TICKS = 100;

    // Use this for initialization
    void Start () {
        gameBoard = FindObjectOfType<GameBoard>();
        transform.localScale = new Vector3(gameBoard.boardCols, gameBoard.boardRows);
        transform.position = new Vector3(gameBoard.boardCols / 2f, gameBoard.boardRows / 2f, -1);
	}

    public void AddContestedTile(Tile tile)
    {
        contestedTiles.Add(tile);
    }

    private void FixedUpdate()
    {
        if(logicCounter <= 0)
        {
            logicCounter = LOGIC_TICKS;
            RunCombatLogic();
        }
        else
        {
            logicCounter--;
        }
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
       var tile =  gameBoard.GetTile(Input.mousePosition);

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
        if(selectedObject != null)
        {
            var army = selectedObject.GetComponent<Army>();
            if(army != null){
                army.MoveTo(tile);
            }
        }
    }
}
