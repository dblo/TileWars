using System;
using UnityEngine;
using UnityEngine.UI;

public class HQ : MonoBehaviour
{
    private Text cashText;
    private Player player;
    private bool buying;

    private const float MIN_LONG_PRESS_DURATION = 0.4f;
    private float swipeStartTime = -1;

    void Awake()
    {
        player = GetComponentInParent<Player>();
        cashText = GetComponentInChildren<Text>();
    }

    internal void UpdateUI()
    {
        cashText.text = "$" + player.GetCash();
    }

    internal Vector2 GetSpawnPoint()
    {
        return transform.position;
    }
    
    private void OnMouseDown()
    {
        GameManager.Get().OnHQClicked();
        swipeStartTime = Time.time;
    }

    private void OnMouseUp()
    {
        if(swipeStartTime + MIN_LONG_PRESS_DURATION <= Time.time)
        {
            OnLongPress();
        }
        else
        {
            OnShortPress();
        }
        swipeStartTime = -1;
        buying = !buying;
    }

    private void OnShortPress()
    {
        if(buying)
            player.AttemptBuyArmy(ArmyType.Infantry);
    }

    private void OnLongPress()
    {
        if(buying)
            player.AttemptBuyArmy(ArmyType.Artillery);
    }
}
