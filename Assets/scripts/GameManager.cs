using UnityEngine;

public class GameManager : MonoBehaviour {
    GameBoard gameBoard;
    Army selectedObject;

	// Use this for initialization
	void Start () {
        gameBoard = FindObjectOfType<GameBoard>();
        transform.localScale = new Vector3(gameBoard.boardCols, gameBoard.boardRows);
        transform.position = new Vector3(gameBoard.boardCols / 2f, gameBoard.boardRows / 2f, -1);
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
