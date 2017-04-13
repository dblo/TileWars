// *******************************************************
// Copyright 2013 Daikon Forge, all rights reserved under 
// US Copyright Law and international treaties
// *******************************************************

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class OnUnityLoad
{
    static OnUnityLoad()
    {
        EditorApplication.playmodeStateChanged = () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                Debug.Log("Auto-Saving scene before entering Play mode: " + EditorApplication.currentScene);
                EditorApplication.SaveScene();
                AssetDatabase.SaveAssets();
            }
        };
    }
}