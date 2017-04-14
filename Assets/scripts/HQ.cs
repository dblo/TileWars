using UnityEngine;
using UnityEngine.UI;

public class HQ : MonoBehaviour
{
    private Text cashText;
    private Player player;
    private bool buying;

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

    private void HandleMouseInteraction()
    {
        if (Input.GetMouseButton(1) && buying)
        {
            player.AttemptBuyArmy(ArmyType.Artillery);
        }
        else if (Input.GetMouseButton(0) && buying)
        {
            player.AttemptBuyArmy(ArmyType.Infantry);
        }
    }

    private void OnMouseDown()
    {
        if (buying)
        {
            HandleMouseInteraction();
        }
        buying = !buying;
    }
}
