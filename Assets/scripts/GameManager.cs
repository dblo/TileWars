using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private ISelectableObject selectedObject;
    private float logicCounter = 0;
    private const int COMBAT_LOGIC_INTERVAL = 1;
    private Player p1;
    private Player p2;
    private List<Vector2> swipePath = new List<Vector2>();
    private float nextMousePoll;
    private const float MOUSE_POLL_RATE = 0.1f;
    private const float MIN_SWIPE_TIME = 0.2f;
    private float swipeStartTime = -1;

    private Text p1CashText;
    private Text p1ScoreText;
    private Text p2ScoreText;
    private Text upgradeText;

    private Button buyInfantryButton;
    private Button buyCavalryButton;
    private Button buyArtilleryButton;
    private Button upgradeButton;

    private static GameManager instance = null;
    private int SCORE_TO_WIN = 10000;
    private bool changedSelectionThisMouseEvent;
    private SpriteRenderer tileSelectionRenderer;

    private void Awake()
    {
        if (instance != null)
            throw new System.Exception("GameManager can nly exist on a single GameObject");

        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        p1ScoreText = GameObject.Find("PlayerScoreText").GetComponent<Text>();
        p2ScoreText = GameObject.Find("OpponentScoreText").GetComponent<Text>();
        p1CashText = GameObject.Find("PlayerCashText").GetComponent<Text>();
        upgradeText = GameObject.Find("UpgradeText").GetComponent<Text>();
        buyInfantryButton = GameObject.Find("BuyInfantryButton").GetComponent<Button>();
        buyCavalryButton = GameObject.Find("BuyCavalryButton").GetComponent<Button>();
        buyArtilleryButton = GameObject.Find("BuyArtilleryButton").GetComponent<Button>();
        tileSelectionRenderer = GameObject.Find("TileSelection").GetComponent<SpriteRenderer>();
        upgradeButton = GameObject.Find("UpgradeButton").GetComponent<Button>();
    }

    // Is this reliable if called from other script's Awake()?
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
        if (p2GO != null)
            p2 = p2GO.GetComponent<Player>();

        nextMousePoll = Time.time;
    }

    private void Update()
    {
        if (ArmySelected())
            ProcessGesturForSelectedArmy();

        logicCounter -= Time.deltaTime;
        if (logicCounter <= 0)
        {
            logicCounter = COMBAT_LOGIC_INTERVAL;
            if (p1 && p2) // TODO disable for release?
            {
                RunCombatLogic();
                UpdateCashScore();
                //CheckIfGameOver();
                UpdateStandingsTexts();
            }
        }
        // TODO Do this only after spending or gaining cash
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        if (p1.CanAffordArmy(ArmyType.Infantry))
            buyInfantryButton.interactable = true;
        else
            buyInfantryButton.interactable = false;

        if (p1.CanAffordArmy(ArmyType.Cavalry))
            buyCavalryButton.interactable = true;
        else
            buyCavalryButton.interactable = false;

        if (p1.CanAffordArmy(ArmyType.Artillery))
            buyArtilleryButton.interactable = true;
        else
            buyArtilleryButton.interactable = false;

        if (ArmySelected())
        {
            if (p1.CanAffordArmyUpgrade(Army.ArmyToArmyType(GetSelectedArmy())))
                upgradeButton.interactable = true;
            else
                upgradeButton.interactable = false;
        }
        else if (TileSelected())
        {
            if(p1.GetCash() >= GetSelectedTile().UpgradeCost())
                upgradeButton.interactable = true;
            else
                upgradeButton.interactable = false;
        }
        else
        {
            upgradeButton.interactable = false;
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
                    p2.AddCash(tile.modifiers.CashValue);
                    p2.AddScore(tile.modifiers.ScoreValues);
                    break;
                case Team.Blue:
                    p1.AddCash(tile.modifiers.CashValue);
                    p1.AddScore(tile.modifiers.ScoreValues);
                    break;
            }
        }
    }

    private void CheckIfGameOver()
    {
        if (p1.GetScore() >= SCORE_TO_WIN || p2.GetScore() >= SCORE_TO_WIN)
            RestartLevel();
    }

    private void UpdateStandingsTexts()
    {
        p1CashText.text = "$ " + p1.GetCash();
        p1ScoreText.text = "P1 " + p1.GetScore();
        p2ScoreText.text = "P2 " + p2.GetScore();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
    }

    private void RunCombatLogic()
    {
        foreach (var army in p2.GetArmies())
        {
            if (army.IsInCombat())
                army.AttackIfAble();
        }
        foreach (var army in p1.GetArmies())
        {
            if (army.IsInCombat())
                army.AttackIfAble();
        }
        PostCombatCleanup(p1, p2);
        PostCombatCleanup(p2, p1);
    }

    private void PostCombatCleanup(Player playerA, Player playerB)
    {
        List<Army> armiesPendingRemoval = new List<Army>();
        foreach (var army in playerA.GetArmies())
        {
            if (!army.IsAlive())
            {
                armiesPendingRemoval.Add(army);
                if (IsSelected(army))
                {
                    ClearSelection();
                    UpdateUpgradeText();
                }
            }
        }
        playerB.OnEnemiesKilled(armiesPendingRemoval);
        playerA.KillArmies(armiesPendingRemoval);
    }

    private bool IsSelected(Army army)
    {
        return GetSelectedArmy() == army;
    }

    internal void OnSelection(ISelectableObject obj)
    {
        if (selectedObject == obj)
        {
            return;
        }

        ClearSelection();
        if (!SelectionAllowed(obj))
        {
            UpdateUpgradeText();
            return;
        }
        selectedObject = obj;
        selectedObject.Select();

        if (TileSelected())
        {
            ShowAndMoveSelectionOverlay();
        }
        UpdateUpgradeText();
        changedSelectionThisMouseEvent = true;
    }

    // Translate the overlay in the 2D plane and enable rendering
    private void ShowAndMoveSelectionOverlay()
    {
        var newPos = GetSelectedTile().transform.position;
        newPos.z = tileSelectionRenderer.transform.position.z;
        tileSelectionRenderer.transform.position = newPos;
        tileSelectionRenderer.enabled = true;
    }

    private void HideSelectionOverlay()
    {
        tileSelectionRenderer.enabled = false;
    }

    private bool SelectionAllowed(ISelectableObject obj)
    {
        if (obj is Army)
        {
            return (obj as Army).GetTeam() == Team.Blue;
        }
        else if (obj is Tile)
        {
            return obj is TraversableTile && (obj as TraversableTile).ControlledBy() == Team.Blue;
        }
        return true;
    }

    private void UpdateUpgradeText()
    {
        if (selectedObject != null)
        {
            var upgradeDetails = selectedObject.GetUpgradeDescriptor();
            if (upgradeDetails != null)
            {
                upgradeText.text = "Upgrade\n" + upgradeDetails;
                GameObject.Find("UpgradeButton").GetComponent<Button>().interactable = true;
            }
            else
            {
                upgradeText.text = "Upgrade\nMaxed";
                GameObject.Find("UpgradeButton").GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            upgradeText.text = "Upgrade";
            GameObject.Find("UpgradeButton").GetComponent<Button>().interactable = false;
        }
    }

    private void ClearSelection()
    {
        HideSelectionOverlay();
        if (HasSelection())
            selectedObject.Deselect();
        selectedObject = null;
    }

    internal void OnBackgroundClicked()
    {
        ClearSelection();
        UpdateUpgradeText();
    }

    private bool HasSelection()
    {
        return selectedObject != null;
    }

    private bool ArmySelected()
    {
        return selectedObject is Army;
    }

    private Army GetSelectedArmy()
    {
        return selectedObject as Army;
    }

    void ProcessGesturForSelectedArmy()
    {
        if (Input.GetMouseButtonDown(0) && MouseOverGameBoard())
        {
            if (swipeStartTime < 0)
            {
                swipeStartTime = Time.time;
            }
        }

        if (swipeStartTime < 0)
            return;

        if (Input.GetMouseButton(0))
        {
            if (nextMousePoll <= Time.time)
            {
                nextMousePoll = Time.time + MOUSE_POLL_RATE;
                swipePath.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (DidGesture())
            {
                GetSelectedArmy().ChangeTravelPath(swipePath);
            }
            else if (!changedSelectionThisMouseEvent)
            {
                SelectTileUnderSelectedArmy();
            }
            swipePath = new List<Vector2>();
            swipeStartTime = -1;
            changedSelectionThisMouseEvent = false;
        }
    }

    private void SelectTileUnderSelectedArmy()
    {
        var pos = GetSelectedArmy().transform.position;
        var tileUnderObj = gameBoard.GetTile((int)pos.y, (int)pos.x);
        OnSelection(tileUnderObj);
    }

    private bool DidGesture()
    {
        return swipeStartTime + MIN_SWIPE_TIME <= Time.time;
    }

    private bool MouseOverGameBoard()
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return pos.x >= 0 && pos.y >= 0 &&
            pos.x < gameBoard.GetColsCount() &&
            pos.y < gameBoard.GetRowsCount();
    }

    public void TryUpgradeSelected()
    {
        if (!HasSelection())
            return;

        if (ArmySelected())
        {
            var armyType = Army.ArmyToArmyType(GetSelectedArmy());
            if (p1.TryUpgrade(armyType))
            {
                UpdateUpgradeText();
            }
        }
        else if (TileSelected())
        {
            if (p1.TryUpgrade(GetSelectedTile()))
            {
                UpdateUpgradeText();
            }
        }
    }

    private Tile GetSelectedTile()
    {
        return selectedObject as Tile;
    }

    private bool TileSelected()
    {
        return selectedObject is Tile;
    }
}
