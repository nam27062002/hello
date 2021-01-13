using UnityEditor;
using UnityEngine;
using System;

public class SearchBigTextures : Editor
{
    [MenuItem("Hungry Dragon/Tools/Optimization/Search big textures in project")]
    public static void StartSearchBigTextures()
    {
        EditorUtility.DisplayProgressBar("Big textures", "Searching for big textures in project...", 0);
        string[] guid = AssetDatabase.FindAssets("t:texture2D");
        for (int i = 0; i < guid.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid[i]);

            // Allow only "gameplay" paths
            if (path.IndexOf("gameplay", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (texture == null)
                continue;

            // Ignore "dragon" and "lightmap" textures
            // Show textures greater than 512x512
            if (texture.name.IndexOf("dragon", StringComparison.OrdinalIgnoreCase) < 0
             && texture.name.IndexOf("lightmap", StringComparison.OrdinalIgnoreCase) < 0
             && texture.width > 512 && texture.height > 512)
                Debug.LogWarning("Texture: " + texture.name + ": " + texture.width + "x" + texture.height + " -- " + path);

            float progress = (float)i / guid.Length;
            EditorUtility.DisplayProgressBar("Big textures", "Searching for big textures in project...", progress);
        }

        EditorUtility.ClearProgressBar();
    }
}
