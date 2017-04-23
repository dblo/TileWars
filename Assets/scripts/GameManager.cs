using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameBoard gameBoard;
    private ISelectableObject selectedObject;
    private int logicCounter;
    private const int LOGIC_TICKS = 50;
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

    private static GameManager instance = null;
    private int SCORE_TO_WIN = 10000;

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
        if (p2GO != null)
            p2 = p2GO.GetComponent<Player>();

        nextMousePoll = Time.time;
        SetInitialSelection();
    }

    private void SetInitialSelection()
    {
        var p1Armies = p1.GetArmies();
        if (p1Armies.Count > 0)
            OnSelection(p1Armies[0]);
    }

    private void Update()
    {
        if (nextMousePoll <= Time.time)
        {
            nextMousePoll = Time.time + MOUSE_POLL_RATE;
            if (ArmySelected())
                ProcessGesturForSelectedArmy();
        }
    }

    private void FixedUpdate()
    {
        if (logicCounter <= 0)
        {
            logicCounter = LOGIC_TICKS;

            if (p2 && p1)
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
        //CheckIfGameOver();
        UpdateUI();
    }

    private void CheckIfGameOver()
    {
        if (p1.GetScore() >= SCORE_TO_WIN || p2.GetScore() >= SCORE_TO_WIN)
            RestartLevel();
    }

    private void UpdateUI()
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
                army.DoCombat();
        }
        foreach (var army in p1.GetArmies())
        {
            if (army.IsInCombat())
                army.DoCombat();
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
            return;

        ClearSelection();
        if (!SelectionAllowed(obj))
            return;

        selectedObject = obj;
        selectedObject.Select();
        UpdateUpgradeText();
    }

    private bool SelectionAllowed(ISelectableObject obj)
    {
        if(obj is Army)
        {
            return (obj as Army).GetTeam() == Team.Blue;
        } else if(obj is Tile)
        {
            return obj is TraversableTile && (obj as TraversableTile).ControlledBy() == Team.Blue;
        }
        return true;
    }

    private void UpdateUpgradeText()
    {
        if (selectedObject != null)
            upgradeText.text = "Upgrade\n" + selectedObject.GetUpgradeDescriptor();
        else
            upgradeText.text = "Upgrade\n-";
    }

    private void ClearSelection()
    {
        if (HasSelection())
            selectedObject.Deselect();
        selectedObject = null;
    }

    internal void OnBackgroundClicked()
    {
        ClearSelection();
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
        if (Input.GetMouseButton(0))
        {
            if (!MouseOverGameBoard())
                return;

            if (swipeStartTime < 0)
            {
                swipeStartTime = Time.time;
            }
            swipePath.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (swipeStartTime > 0)
        {
            if (swipeStartTime + MIN_SWIPE_TIME <= Time.time)
            {
                GetSelectedArmy().ChangeTravelPath(swipePath);
            }
            swipePath = new List<Vector2>();
            swipeStartTime = -1;
            return;
        }
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
            if(p1.TryUpgrade(armyType))
            {
                UpdateUpgradeText();
            }
        }
        else if (TileSelected())
        {

        }
    }

    private bool TileSelected()
    {
        return selectedObject is Tile;
    }
}
