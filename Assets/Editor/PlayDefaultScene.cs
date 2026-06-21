using UnityEngine;
using UnityEditor;                  // <-- This is the line that fixes your error!
using UnityEditor.SceneManagement;  // <-- This one is needed to manage scenes

public class PlayDefaultScene 
{
    // This creates a new button at the top of your Unity Editor
    [MenuItem("Tools/Play Default Scene")]
    public static void PlayScene()
    {
        // Replace with the exact path to your start scene!
        // Example: "Assets/Scenes/Main Menu.unity"
        string scenePath = "Assets/Scenes/TestConnectToServer/QuickJoinLobby.unity"; 
        
        SceneAsset startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        
        if (startScene != null)
        {
            EditorSceneManager.playModeStartScene = startScene;
            EditorApplication.isPlaying = true;
        }
        else
        {
            Debug.LogError("Could not find the Scene! Double-check the exact path and spelling in 'scenePath'.");
        }
    }
}