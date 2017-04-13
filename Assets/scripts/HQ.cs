using UnityEngine;
using UnityEngine.UI;

public enum Direction { N, W, S, E };

public class HQ : MonoBehaviour
{
    private Text cashText;
    private Player player;

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
        player.AttemptBuyArmy(Direction.S);
    }
}
