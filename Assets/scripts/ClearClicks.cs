using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearClicks : MonoBehaviour {
    private void OnMouseDown()
    {
        GameManager.Get().OnBackgroundClicked();
    }
}
