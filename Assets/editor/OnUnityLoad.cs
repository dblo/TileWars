// *******************************************************
// Copyright 2013 Daikon Forge, all rights reserved under 
// US Copyright Law and international treaties
// *******************************************************

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class OnUnityLoad
{
    static OnUnityLoad()
    {
        EditorApplication.playmodeStateChanged = () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                var currenScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Debug.Log("Auto-Saving scene before entering Play mode: " + currenScene);
                EditorSceneManager.SaveScene(currenScene);
                AssetDatabase.SaveAssets();
            }
        };
    }
}