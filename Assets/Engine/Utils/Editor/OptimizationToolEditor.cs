using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
// using FGOL;
// using FGOL.Build;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

public class OptimizationToolEditor
{
	[MenuItem("Hungry Dragon/OptimizationUtility/Enable All")]
	public static void EnableAll()
	{
		SceneMaterialSetupEditor.GenerateSceneMaterialFromEditor();
		SceneMaterialSetupEditor.SetAllBumpMapToNull();

		// Broken
		//SceneLightmapSetupEditor.UpdateAllLightMapDataInfosEditor();
		//SceneLightmapSetupEditor.SetAllSetLightmapNearToNull();

		PrefabsSetupEditor.GeneratePrefabsLOD();
	}

	[MenuItem("Hungry Dragon/OptimizationUtility/Disable All")]
	public static void DisableAll()
	{
		SceneMaterialSetupEditor.RestoreOriginalMaterialFromEditor();
		SceneMaterialSetupEditor.DeleteFoldersEditor();

		// Broken
		//SceneLightmapSetupEditor.RestoreAllLightMapDataInfosEditor();
		//SceneLightmapSetupEditor.DeleteFoldersEditor();

		PrefabsSetupEditor.DeleteFoldersEditor();
	}
}
/*
public class DisableShadowsOnPrefabs : EditorWindow
{
	private static List<string>					m_results;
	private static Vector2						m_scrollPos;
	private static readonly string				PrefabsPath = Application.dataPath + "/Prefabs";

	[MenuItem("FGOL/OptimizationUtility/Disable shadows on prefabs")]
	public static void Scan()
	{
		m_results = new List<string>();

		EditorUtility.DisplayProgressBar("Scanning", "Searching for prefabs", 0);

		// Find all prefabs
		List<string> files = FileUtility.GetAllFilesInDirectory(PrefabsPath, true, true, false);

		for(int i = 0;i < files.Count;i++)
		{
			string fullPath = files.ElementAt(i);
			EditorUtility.DisplayProgressBar("Reading prefabs", fullPath, (float)i / files.Count);

			string[] lines = File.ReadAllLines(fullPath);
			for(int j = 0; j < lines.Length; j++)
			{
				if((lines[j].Contains("m_CastShadows: 1") || lines[j].Contains("m_ReceiveShadows: 1")) && !m_results.Contains(fullPath))
				{
					m_results.Add(fullPath);
					continue;
				}			
			}
		}

		EditorUtility.ClearProgressBar();

		m_scrollPos = Vector2.zero;

		var window = EditorWindow.GetWindow(typeof(DisableShadowsOnPrefabs));
		window.titleContent.text = "Disable shadows";
		window.Show();
	}

	private void OnGUI()
	{
		if(m_results != null)
		{
			if(GUILayout.Button("Disable shadows for all objects"))
			{
				Disable();
				return;
			}

			GUILayout.Label("Perforce plugins sometimes fails to pick changes correctly \nto be safe, checkout Prefabs folder and revert unchanged after disabling shadows\n"); 

			GUILayout.Label("Total prefabs with shadows = " +  m_results != null ? m_results.Count.ToString() : "0");
	
			// Display all empty folders paths
			m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
			GUILayout.BeginVertical();
			for(int i = 0; i < m_results.Count; i++)
			{
				GUILayout.Label(m_results[i]);
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}
		else
		{
			GUILayout.Label("Done");
		}
	}

	private static void Disable()
	{
		for(int i = 0;i < m_results.Count;i++)
		{
			EditorUtility.DisplayProgressBar("Updating prefabs", m_results[i], (float)i / m_results.Count);

			string relativePath = "Assets" + m_results[i].Substring(Application.dataPath.Length);
			FGOL.Perforce.PerforceManager.Checkout(new List<string>() { relativePath }, false);

			string text = File.ReadAllText(m_results[i]);
			text = text.Replace("m_CastShadows: 1", "m_CastShadows: 0");
			text = text.Replace("m_ReceiveShadows: 1", "m_ReceiveShadows: 0");
			File.WriteAllText(m_results[i], text);

			// If change was made, print to console and update prefab
			Debug.Log("Disabled shadows on " + m_results[i]);
		}

		// Save prohect to reflect changes in perforce
		EditorApplication.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
		m_results = null;
	}
}
*/

public class SceneMaterialSetupEditor
{
	[MenuItem("Hungry Dragon/OptimizationUtility/SceneMaterial/GenerateSceneMaterial")]
    public static void GenerateSceneMaterialFromEditor()
    {
        // List<string> foldersToCheckOut = OptimizationDataEditor.instance.GetSceneMaterialFolders();
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        GenerateSceneMaterial();
		SceneMaterialSetupEditor.SetAllBumpMapToNull();
		// FGOL.Perforce.PerforceManager.Revert(foldersToCheckOut);
    }

    public static void GenerateSceneMaterial()
    {
        OptimizationDataEditor.instance.ResetSceneMaterialFolder();

        MaterialsSceneProcessor materialScene = new MaterialsSceneProcessor();
        materialScene.Reset();
        AssetDatabase.StartAssetEditing();
        string[] scenes;

        OptimizationDataInfo test = OptimizationDataInfo.instance;
        scenes = OptimizationDataInfo.instance.GetArtScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            materialScene.StoreHiDetailMaterialInTheResourceFolder();
        }

		scenes = OptimizationDataInfo.instance.GetSpawnerScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetSpawnerLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            materialScene.StoreHiDetailMaterialInTheResourceFolder();
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

    }
    
	[MenuItem("Hungry Dragon/OptimizationUtility/SceneMaterial/RestoreOriginalMaterial")]
    public static void RestoreOriginalMaterialFromEditor()
    {
        RestoreOriginalMaterial();
        OptimizationDataEditor.instance.DeleteSceneMaterialFolder();
    }

	[MenuItem("Hungry Dragon/OptimizationUtility/SceneMaterial/DeleteFolders")]
    public static void DeleteFoldersEditor()
    {
        // List<string> foldersToCheckOut = new List<string>(){OptimizationDataInfo.LightmapResourceSaveFolderPath};
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        DeleteFolders();
    }
    
    public static void DeleteFolders()
    {
        List<string> directoryToDelete=OptimizationDataEditor.instance.GetSceneMaterialFolders();
        foreach(string path in directoryToDelete)
        {
            OptimizationDataEditor.instance.DeleteADir(path);
        }
    }

    public static void SetAllBumpMapToNull()
    {
    	string[] scenes;
		scenes = OptimizationDataInfo.instance.GetArtScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            SetBumpMapToNullCurrentLevel();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        scenes = OptimizationDataInfo.instance.GetSpawnerScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetSpawnerLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            SetBumpMapToNullCurrentLevel();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.Refresh();
    }

    public static void SetBumpMapToNullCurrentLevel()
    {
        SceneMaterialDynamicLoader loader = GameObject.FindObjectOfType<SceneMaterialDynamicLoader>();
        if (loader != null)
        {
            MaterialsSceneProcessor materialScene = new MaterialsSceneProcessor();
            materialScene.SetMaterialBumpMapToNull();
        }
    }

    public static void RestoreOriginalMaterial()
    {
        AssetDatabase.StartAssetEditing();
        string[] scenes;

        scenes = OptimizationDataInfo.instance.GetArtScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            Debug.Log("sceneFile " + sceneFile);
            EditorSceneManager.OpenScene(sceneFile);
            SceneMaterialDynamicLoader loader = GameObject.FindObjectOfType<SceneMaterialDynamicLoader>();
            if (loader != null)
            {
                MaterialsSceneProcessor materialScene = new MaterialsSceneProcessor();
                materialScene.RestoreHiDetailMaterialFromTheResourceFolder();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }

        scenes = OptimizationDataInfo.instance.GetSpawnerScenes();
        foreach (string level in scenes)
        {
            string sceneFile = OptimizationDataEditor.instance.GetSpawnerLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            SceneMaterialDynamicLoader loader = GameObject.FindObjectOfType<SceneMaterialDynamicLoader>();
            if (loader != null)
            {
                MaterialsSceneProcessor materialScene = new MaterialsSceneProcessor();
                materialScene.RestoreHiDetailMaterialFromTheResourceFolder();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }
}
// TODO (MALH): Recover this class?
/* 
public class SceneLightmapSetupEditor
{
    //[MenuItem("FGOL/OptimizationUtility/Lightmap/UpdateAllLightMapDataInfos")]
    public static void UpdateAllLightMapDataInfosEditor()
    {
        List<string> foldersToCheckOut = new List<string>(){OptimizationDataInfo.LightmapResourceSaveFolderPath};
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        UpdateAllLightMapDataInfos();
        // FGOL.Perforce.PerforceManager.Revert(foldersToCheckOut);
    }

    public static void UpdateAllLightMapDataInfos()
    {
        AssetDatabase.StartAssetEditing();
        foreach (string level in OptimizationDataInfo.instance.levelToLoad)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            Debug.Log("sceneFile " + sceneFile);
            EditorSceneManager.OpenScene(sceneFile);
            UpdateCurrentLevelLightMapDataInfos();
        }
        
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }

   // [MenuItem("FGOL/OptimizationUtility/Lightmap/UpdateCurrentLevelLightMapDataInfos")]
    public static void UpdateCurrentLevelLightMapDataInfosEditor()
    {
        List<string> foldersToCheckOut = LightMapDataInfosGetAllFilesTocheckOut();
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        AssetDatabase.StartAssetEditing();
        UpdateCurrentLevelLightMapDataInfos();
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
        // FGOL.Perforce.PerforceManager.Revert(foldersToCheckOut);
    }

    //[MenuItem("FGOL/OptimizationUtility/Lightmap/DeleteFolders")]
    public static void DeleteFoldersEditor()
    {
        List<string> foldersToCheckOut = new List<string>(){OptimizationDataInfo.LightmapResourceSaveFolderPath};
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        DeleteFolders();
    }

    public static void DeleteFolders()
    {
        List<string> directoryToDelete=new List<string>();
        directoryToDelete.Add(OptimizationDataInfo.LightmapResourceSaveFolderPath);
        foreach(string path in directoryToDelete)
        {
            OptimizationDataEditor.instance.DeleteADir(path);
        }
    }

    public static void UpdateCurrentLevelLightMapDataInfos()
    {
        LightMapDynamicLoader lightmapDynamicLoader = GameObject.FindObjectOfType<LightMapDynamicLoader>();
        if (lightmapDynamicLoader != null)
        {
            lightmapDynamicLoader.lightMapsDataInfo = null;
            LightMapsDataInfo lightMapsDataInfo = NewLightMapsDataInfoFromCurrentLevel();
            lightmapDynamicLoader.lightMapsDataInfo = lightMapsDataInfo;
            EditorUtility.SetDirty(lightmapDynamicLoader);
        }
    }

    public static void SetAllSetLightmapNearToNull()
    {
        AssetDatabase.StartAssetEditing();
        foreach (string level in OptimizationDataInfo.instance.levelToLoad)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            SetLightmapNearCurrentLeveToNull();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }

    public static void RestoreAllLevelesLightmapNearFromResource()
    {
        AssetDatabase.StartAssetEditing();
        foreach (string level in OptimizationDataInfo.instance.levelToLoad)
        {
            string sceneFile = OptimizationDataEditor.instance.GetLevelFileURLFromName(level);
            EditorSceneManager.OpenScene(sceneFile);
            RestoreCurrentLevelLightmapNearFromResource();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
    }

    public static void SetLightmapNearCurrentLeveToNull()
    {
        if (LightmapSettings.lightmaps.Length > 0)
        {
            for (int i=0; i<LightmapSettings.lightmaps.Length; i++)
            {
                LightmapData lightmapData = LightmapSettings.lightmaps [i];
                lightmapData.lightmapNear = null;
            }
        }
    }

    public static void RestoreCurrentLevelLightmapNearFromResource()
    {
        LightMapDynamicLoader lightmapDynamicLoader = GameObject.FindObjectOfType<LightMapDynamicLoader>();
        if (lightmapDynamicLoader != null)
        {
            lightmapDynamicLoader.SetHiResLightMap(true);
        }
    }

    public static List<string> LightMapDataInfosGetAllFilesTocheckOut()
    {
        List<string> files = new List<string>();
        if (LightmapSettings.lightmaps.Length > 0)
        {
            string destinationfolder = OptimizationDataInfo.LightmapResourceSaveFolderPath + "/" + System.IO.Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().name);
            if (System.IO.Directory.Exists(destinationfolder))
            {
                files.Add(destinationfolder);
                for (int i=0; i<LightmapSettings.lightmaps.Length; i++)
                {
                    LightmapData lightmapData = LightmapSettings.lightmaps [i];
                    if (lightmapData.lightmapNear != null)
                    {
                        string lightmapTextureOriginalFile = UnityEditor.AssetDatabase.GetAssetPath(lightmapData.lightmapNear);
                        if (lightmapTextureOriginalFile != null)
                        {
                            string destinationPath = destinationfolder + "/" + System.IO.Path.GetFileName(lightmapTextureOriginalFile);
                            if (System.IO.File.Exists(destinationPath))
                            {
                                files.Add(destinationPath);
                            }
                        }
                    }
                }
            }
        }
        return files;
    }

    public static LightMapsDataInfo NewLightMapsDataInfoFromCurrentLevel()
    {
        LightMapsDataInfo lightMapDataInfos = null;
        if (LightmapSettings.lightmaps.Length > 0)
        {
            lightMapDataInfos = new LightMapsDataInfo();
            LightMapDataLinks[] lightMapDataLinksArray = new LightMapDataLinks[LightmapSettings.lightmaps.Length];
            for (int i=0; i<LightmapSettings.lightmaps.Length; i++)
            {
                LightMapDataLinks lightMapDataLinks = new LightMapDataLinks();
                lightMapDataLinksArray [i] = lightMapDataLinks;
                LightmapData lightmapData = LightmapSettings.lightmaps [i];
                if (lightmapData.lightmapNear != null)
                {
                    lightMapDataLinks.lightmapTextureOriginalFile = UnityEditor.AssetDatabase.GetAssetPath(lightmapData.lightmapNear);
                    if (lightMapDataLinks.lightmapTextureOriginalFile != null)
                    {
                        UnityEngine.Debug.Log("lightmapData.lightmapNear " + lightMapDataLinks.lightmapTextureOriginalFile + " mode " + LightmapSettings.lightmapsMode);
                        string destinationfolder = OptimizationDataInfo.LightmapResourceSaveFolderPath + "/" + System.IO.Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().name);
                        OptimizationDataEditor.instance.EmptyOrCreateADir(destinationfolder);
                        bool directoryExist=System.IO.Directory.Exists(destinationfolder);
                        UnityEngine.Debug.Log("Directory.Exists "+ directoryExist+" AssetDatabase.IsValidFolder "+AssetDatabase.IsValidFolder(destinationfolder));
                        if (directoryExist)
                        {
                            lightMapDataLinks.lightmapTextureResourcePath = destinationfolder + "/" + System.IO.Path.GetFileName(lightMapDataLinks.lightmapTextureOriginalFile);
                            UnityEngine.Debug.Log("Destination " + lightMapDataLinks.lightmapTextureResourcePath);
                            
                            if (!System.IO.File.Exists(lightMapDataLinks.lightmapTextureResourcePath))
                            {
                                bool copied = UnityEditor.AssetDatabase.CopyAsset(lightMapDataLinks.lightmapTextureOriginalFile, lightMapDataLinks.lightmapTextureResourcePath);
                                UnityEngine.Debug.Log("Copied Asset From " + lightMapDataLinks.lightmapTextureOriginalFile + " To " + lightMapDataLinks.lightmapTextureResourcePath + " copied " + copied);
                                
                            } 
                            else
                            {
                                UnityEditor.FileUtil.ReplaceFile(lightMapDataLinks.lightmapTextureOriginalFile, lightMapDataLinks.lightmapTextureResourcePath);
                                UnityEngine.Debug.Log("Asset Replaced From " + lightMapDataLinks.lightmapTextureOriginalFile + " To " + lightMapDataLinks.lightmapTextureResourcePath);
                            }
                            lightMapDataLinks.lightmapTextureResourceLink = OptimizationDataEditor.Instance.GetResourceLinkFromPath(lightMapDataLinks.lightmapTextureResourcePath);
                            UnityEditor.AssetDatabase.ImportAsset(lightMapDataLinks.lightmapTextureResourcePath);
                        }
                    }
                }
            }
            lightMapDataInfos.lightMapDataInfos = lightMapDataLinksArray;
            lightMapDataInfos.lightmapMode = LightmapSettings.lightmapsMode;
        }
        return lightMapDataInfos;    
    }
}
*/

public class SpawnerScenesVariationsEditor
{
	public static void GenerateSpawnerScenesWithoutNormals()
	{

	}
}

public class PrefabsSetupEditor
{
    static bool showDialog = false;
	static Dictionary<int,Material> materialDictionary = new Dictionary<int,Material> ();

    [MenuItem("FGOL/OptimizationUtility/Prefabs/GeneratePrefabsLOD")]
    public static void GeneratePrefabsEditorLODEditor()
    {
        // List<string> foldersToCheckOut = OptimizationDataEditor.instance.GetPrefabsFolderToBeReset();
        // FGOL.Perforce.PerforceManager.Checkout(foldersToCheckOut, true);
        GeneratePrefabsLOD();
        // FGOL.Perforce.PerforceManager.Revert(foldersToCheckOut);
    }

    public static void GeneratePrefabsLOD()
    {
		materialDictionary.Clear();

		string prefabsDestinationFolder = Application.dataPath + "/Resources/" + IEntity.ENTITY_PREFABS_LOW_PATH;
		OptimizationDataEditor.instance.DeleteAndCreateADir( prefabsDestinationFolder );
		string materialsDestinationFolder = Application.dataPath + "/Resources/" + IEntity.ENTITY_PREFABS_LOW_PATH + "/Materials";
		OptimizationDataEditor.instance.DeleteAndCreateADir( materialsDestinationFolder );

		string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
		OptimizationDataEditor.instance.DeleteAndCreateADir( projectPath + OptimizationDataInfo.MaterialPrefabsSavePath );

		string prefabOriginalFolder = Application.dataPath + "/Resources/" + IEntity.ENTITY_PREFABS_PATH;
		string[] files = Directory.GetFiles(prefabOriginalFolder, "*.prefab", SearchOption.AllDirectories);

		foreach (string f in files) 
		{
			
			string resoucePath = f.Substring( (Application.dataPath + "/Resources/").Length );
			resoucePath = resoucePath.Substring( 0, resoucePath.Length - ".prefab".Length);
			GameObject prefab =  Resources.Load<GameObject>( resoucePath) ;
			if ( prefab != null )
			{
				// Create a prefab copy
				string destinationPrefabPath = "Assets/Resources/" + IEntity.ENTITY_PREFABS_LOW_PATH + f.Substring( prefabOriginalFolder.Length);
				Directory.CreateDirectory( Path.GetDirectoryName(destinationPrefabPath) );
				UnityEngine.Object newPrefab = PrefabUtility.CreateEmptyPrefab( destinationPrefabPath );

				if (newPrefab != null) 
				{
					GameObject updatedPrefab = PrefabUtility.ReplacePrefab (
						prefab,
						newPrefab,
						ReplacePrefabOptions.ConnectToPrefab
					);

					// Create low quality material
					CreateLowMaterials( updatedPrefab );
				}
			}
		}
    }

    private static void CreateLowMaterials( GameObject prefab )
    {
		Renderer[] renderers = prefab.GetComponentsInChildren<Renderer> ();
		if (renderers != null && renderers.Length > 0)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				processRenderer( renderers[i] );
			}
		}
    }

	protected static bool processRenderer (Renderer renderer)
	{
		bool materialChanged = false;
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i=0; i<sharedMaterials.Length; i++)
		{
			Material material = sharedMaterials[i];
			if (material != null)
			{
				int materialInstanceID = material.GetInstanceID ();
				if (!materialDictionary.ContainsKey (materialInstanceID))
				{
					for (int j=0; j<OptimizationDataInfo.instance.PropertiesNameTofind.Length; j++) {
						string propertyNameTofind = OptimizationDataInfo.instance.PropertiesNameTofind [j];
						if (material.HasProperty (propertyNameTofind))
						{
							Material clonedMaterial = new Material (material);
							clonedMaterial.SetTexture (propertyNameTofind, null);
							clonedMaterial.name = material.name + OptimizationDataInfo.LDSufix;
							materialDictionary [materialInstanceID] = clonedMaterial;

							string materialName = clonedMaterial.name + OptimizationDataInfo.MaterialFileExtension;
							string materialSVPath = OptimizationDataInfo.MaterialPrefabsSavePath + "/" + materialName;
							AssetDatabase.CreateAsset (clonedMaterial, materialSVPath);
							sharedMaterials [i] = materialDictionary [materialInstanceID];
							materialChanged = true;
							break;
						}
					}
				}
				else
				{
					sharedMaterials [i] = materialDictionary [materialInstanceID];
					materialChanged = true;
				}
			}
		}
		renderer.sharedMaterial = sharedMaterials[0];
		renderer.sharedMaterials = sharedMaterials;
		return materialChanged;
	}

    [MenuItem("FGOL/OptimizationUtility/Prefabs/DeleteFolders")]
    public static void DeleteFoldersEditor()
    {
        DeleteFolders();
    }

    public static void DeleteFolders()
    {
		string prefabsDestinationFolder = Application.dataPath + "/Resouces/" + IEntity.ENTITY_PREFABS_LOW_PATH;
		OptimizationDataEditor.instance.DeleteADir( prefabsDestinationFolder );
		string materialsDestinationFolder = Application.dataPath + "/Resouces/" + IEntity.ENTITY_PREFABS_LOW_PATH + "/Materials";
		OptimizationDataEditor.instance.DeleteADir( materialsDestinationFolder );
    }


}