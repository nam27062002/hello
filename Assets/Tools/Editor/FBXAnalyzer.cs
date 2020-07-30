using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FBXAnalyzer : EditorWindow
{
    [MenuItem("Hungry Dragon/Tools/Gameplay/FBX Analyzer...", false, -150)]
    static void Init()
    {
        EditorUtility.DisplayProgressBar("FBX analyzer", "Scanning FBXs...", 0.0f);
        List<string> noCompression = new List<string>();
        List<string> readWrite = new List<string>();
        List<string> noBundle = new List<string>();

        string[] guid = AssetDatabase.FindAssets("t:Model");
		for (int i = 0; i < guid.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid[i]);
			ModelImporter model = (ModelImporter)AssetImporter.GetAtPath(path);
            if (model.meshCompression == ModelImporterMeshCompression.Off)
            {
                noCompression.Add(path);

                //model.meshCompression = ModelImporterMeshCompression.High;
                //AssetDatabase.ImportAsset(path);
            }

            if (model.isReadable)
            {
                readWrite.Add(path);
            }

            if (string.IsNullOrEmpty(model.assetBundleName) || model.assetBundleName == "None")
            {
                noBundle.Add(path);
            }

            float progress = (float) i / guid.Length;
            EditorUtility.DisplayProgressBar("FBX analyzer", "Scanning FBXs...", progress);
        }

        noCompression.Sort();
        for (int i = 0; i < noCompression.Count; i++)
        {
            Debug.LogWarning("FBX without Mesh Compression enabled: " + noCompression[i]);
        }

        readWrite.Sort();
        for (int i = 0; i < readWrite.Count; i++)
        {
            Debug.LogWarning("FBX with Read / Write enabled: " + readWrite[i]);
        }

        noBundle.Sort();
        for (int i = 0; i < noBundle.Count; i++)
        {
            Debug.LogWarning("FBX without bundle assigned: " + noBundle[i]);
        }

        Debug.LogWarning("--- RESULTS ---");
        Debug.LogWarning("Found: " + noCompression.Count + " FBXs without Mesh Compression enabled");
        Debug.LogWarning("Found: " + readWrite.Count + " FBXs with Read/Write enabled");
        Debug.LogWarning("Found: " + noBundle.Count + " FBXs without asset bundle assigned");
        Debug.LogWarning("Total FBXs analyzed: " + guid.Length);

        EditorUtility.ClearProgressBar();
        //AssetDatabase.SaveAssets();
	}
}
