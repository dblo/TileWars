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
    [SerializeField]
    private Player bluePlayer;
    [SerializeField]
    private AIPlayer redPlayer;
    private List<Vector2> swipePath = new List<Vector2>();
    private float nextMousePoll;
    private const float MOUSE_POLL_RATE = 0.1f;
    private const float MIN_SWIPE_TIME = 0.1f;
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
    private bool winnable = true;

    private void Awake()
    {
        if (instance != null)
            throw new System.Exception("GameManager can only exist on a single GameObject");

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
        SetGamePaused(false);
        gameBoard = FindObjectOfType<GameBoard>();

        var slider = GameObject.Find("AIArmyCountSlider").GetComponent<Slider>();
        var maxArmyCount = PlayerPrefs.GetInt(AIPlayer.MAX_AI_ARMIES, AIPlayer.DEFAULT_MAX_ARMIES);
        slider.value = maxArmyCount;
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
        nextMousePoll = Time.time;
        var p1pos = bluePlayer.transform.position;
        var intialTileSelection = gameBoard.GetTile((int)p1pos.y, (int)p1pos.x);
        OnSelectionChange(intialTileSelection);
        InitPlayerTileControl();
    }

    private void InitPlayerTileControl()
    {
        foreach (var tile in gameBoard.GetTraversableTiles())
        {
            if (tile.ControlledBy() == Team.Blue)
                bluePlayer.IncrementControlledTiles();
            else if (tile.ControlledBy() == Team.Red)
                redPlayer.IncrementControlledTiles();
        }
    }

    private void Update()
    {
        if (ArmySelected())
            ProcessGesturForSelectedArmy();

        logicCounter -= Time.deltaTime;
        if (logicCounter <= 0)
        {
            logicCounter = COMBAT_LOGIC_INTERVAL;
            if (bluePlayer && redPlayer) // TODO disable for release?
            {
                RunCombatLogic();
                UpdateCashScore();
                CheckIfGameOverByScore();
                UpdateStandingsTexts();
            }
        }
        // TODO Do this only after spending or gaining cash
        UpdateButtonsInteractable();
    }

    internal void OnTileControlChanged(TraversableTile tile, Team formerControllingTeam)
    {
        if (GetSelectedTile() == tile)
            OnSelectionChange(null);

        if (tile.ControlledBy() == Team.Blue)
        {
            bluePlayer.IncrementControlledTiles();
            if (formerControllingTeam == Team.Red)
                redPlayer.DecementControlledTiles();
            CheckIfGameOverByTiles();
        }
        else if (tile.ControlledBy() == Team.Red)
        {
            redPlayer.IncrementControlledTiles();
            if (formerControllingTeam == Team.Blue)
                bluePlayer.DecementControlledTiles();
            CheckIfGameOverByTiles();
        }
        else
            throw new ArgumentException();
    }

    public void ToggleWinnable()
    {
        winnable = !winnable;
    }

    private void SetGamePaused(bool paused)
    {
        Time.timeScale = paused ? 0 : 1;
    }

    private void UpdateButtonsInteractable()
    {
        if (bluePlayer.CanAffordArmy(ArmyType.Infantry) && TileSelected())
            buyInfantryButton.interactable = true;
        else
            buyInfantryButton.interactable = false;

        if (bluePlayer.CanAffordArmy(ArmyType.Cavalry) && TileSelected())
            buyCavalryButton.interactable = true;
        else
            buyCavalryButton.interactable = false;

        if (bluePlayer.CanAffordArmy(ArmyType.Artillery) && TileSelected())
            buyArtilleryButton.interactable = true;
        else
            buyArtilleryButton.interactable = false;

        if (ArmySelected())
        {
            if (bluePlayer.CanAffordArmyUpgrade(Army.ArmyToArmyType(GetSelectedArmy())) && !GetSelectedArmy().UpgradeMaxed())
                upgradeButton.interactable = true;
            else
                upgradeButton.interactable = false;
        }
        else if (TileSelected())
        {
            if(bluePlayer.GetCash() >= GetSelectedTile().UpgradeCost() && !GetSelectedTile().UpgradeMaxed())
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
                    redPlayer.AddCash(tile.modifiers.CashValue);
                    redPlayer.AddScore(tile.modifiers.ScoreValues);
                    break;
                case Team.Blue:
                    bluePlayer.AddCash(tile.modifiers.CashValue);
                    bluePlayer.AddScore(tile.modifiers.ScoreValues);
                    break;
            }
        }
    }

    private void CheckIfGameOverByTiles()
    {
        if (!winnable)
            return;
        var controllableTilesCount = gameBoard.GetTraversableTiles().Count;
        if (bluePlayer.ControllingTilesCount == controllableTilesCount || 
            redPlayer.ControllingTilesCount == controllableTilesCount)
            RestartLevel();
    }

    private void CheckIfGameOverByScore()
    {
        if (!winnable)
            return;
        if (bluePlayer.GetScore() >= SCORE_TO_WIN || redPlayer.GetScore() >= SCORE_TO_WIN)
            RestartLevel();
    }

    private void UpdateStandingsTexts()
    {
        p1CashText.text = "$ " + bluePlayer.GetCash();
        p1ScoreText.text = "Blue " + bluePlayer.GetScore();
        p2ScoreText.text = "Red " + redPlayer.GetScore();
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
    }

    private void RunCombatLogic()
    {
        foreach (var army in redPlayer.GetArmies())
        {
            if (army.IsInCombat())
                army.AttackIfAble();
        }
        foreach (var army in bluePlayer.GetArmies())
        {
            if (army.IsInCombat())
                army.AttackIfAble();
        }
        PostCombatCleanup(bluePlayer, redPlayer);
        PostCombatCleanup(redPlayer, bluePlayer);
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

    internal void OnSelectionChange(ISelectableObject obj)
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
        if (obj == null)
            return false;

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
        OnSelectionChange(tileUnderObj);
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

    public void TryBuyInfantry()
    {
        if(TileSelected())
            bluePlayer.TryBuyArmy(ArmyType.Infantry, GetSelectedTile().transform.position);
    }

    public void TryBuyCavalry()
    {
        if (TileSelected())
            bluePlayer.TryBuyArmy(ArmyType.Cavalry, GetSelectedTile().transform.position);
    }

    public void TryBuyArtillery()
    {
        if (TileSelected())
            bluePlayer.TryBuyArmy(ArmyType.Artillery, GetSelectedTile().transform.position);
    }

    public void ContinueGame()
    {
        GameObject.Find("MenuCanvas").GetComponent<Canvas>().enabled = false;
        SetGamePaused(false);
    }

    public void SetAIMaxArmies()
    {
        var slider = GameObject.Find("AIArmyCountSlider").GetComponent<Slider>();
        redPlayer.SetMaxArmiesCount((int)slider.value); // TODO yeha
    }

    public void ShowInGameMenu() 
    {
        SetGamePaused(true);
        GameObject.Find("MenuCanvas").GetComponent<Canvas>().enabled = true;
    }

    public void TryUpgradeSelected()
    {
        if (!HasSelection())
            return;

        if (ArmySelected())
        {
            var armyType = Army.ArmyToArmyType(GetSelectedArmy());
            if (bluePlayer.TryUpgrade(armyType))
            {
                UpdateUpgradeText();
            }
        }
        else if (TileSelected())
        {
            if (bluePlayer.TryUpgrade(GetSelectedTile()))
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
