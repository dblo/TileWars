using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityStandardAssets._2D
{
    public class Restarter : MonoBehaviour
    {
        private void OnMouseDown()
        {
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
        }
    }
}
