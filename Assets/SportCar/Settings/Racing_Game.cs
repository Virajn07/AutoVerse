#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
    static Startup()    
    {
        EditorPrefs.SetInt("showCounts_sportcarcgb11", EditorPrefs.GetInt("showCounts_sportcarcgb11") + 1);

        if (EditorPrefs.GetInt("showCounts_sportcarcgb11") == 1)       
        {
            Application.OpenURL("https://assetstore.unity.com/packages/slug/348584");
            // System.IO.File.Delete("Assets/SportCar/Racing_Game.cs"); 
        }
    }     
}
#endif
