using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

class MaterialPrefabSetupObjects
{
	Dictionary<int,Material> materialDictionary = new Dictionary<int,Material> ();
	Dictionary<string,GameObject> prefabsDictionary = new Dictionary<string,GameObject> ();

	public  void Reset ()
	{
		materialDictionary.Clear ();
		prefabsDictionary.Clear ();
	}

	// TODO (MALH): Recover this class?
	/*
	public void ProcessPrefabs(PoolsManager poolManager,PoolsManager.PoolEntityLinkData data,UnityEngine.Object originalPrefab,GameObject entityPoolgameObject,OptimizationDataInfo.QualityLevelsType qualityLevelsType,bool prepass)
	{
		if (qualityLevelsType == OptimizationDataInfo.QualityLevelsType.Medium)
			return;

		if (prepass && qualityLevelsType != OptimizationDataInfo.QualityLevelsType.High) {
			return;
		}


		EntityPool entityPool = entityPoolgameObject.GetComponent<EntityPool> ();
		GameObject entityPoolprefabGameObject=entityPool.gameObject;

		//if the hi quality prefab is already inside resource we don't need to copy it again
		if (prepass && qualityLevelsType == OptimizationDataInfo.QualityLevelsType.High)
		{
			if(originalPrefab!=null)
			{
				string prefabURL=AssetDatabase.GetAssetPath(originalPrefab);
				if(prefabURL!=null && prefabURL!="")
				{
					poolManager.OriginalPrefabsPathFiles.Add( prefabURL);
				}
			}

		}

		string entityPoolPrefabsName = entityPool.gameObject.name+OptimizationDataInfo.instance.GetSufix(qualityLevelsType);
		OptimizationDataEditor.instance.CreateADir (OptimizationDataInfo.ResourcePoolsFolderPath);
		UnityEngine.Object entityPoolNewPrefab = PrefabUtility.CreateEmptyPrefab (OptimizationDataInfo.ResourcePoolsFolderPath+OptimizationDataInfo.DirectorySeparator+entityPoolPrefabsName+".prefab");
		if (entityPoolNewPrefab != null) {
			PrefabUtility.ReplacePrefab (
				entityPoolprefabGameObject,
				entityPoolNewPrefab,
				ReplacePrefabOptions.ConnectToPrefab
			);
			string resourceFileLink = OptimizationDataInfo.PoolsFolderName +OptimizationDataInfo.DirectorySeparator + entityPoolPrefabsName;
			data.SetData (qualityLevelsType, resourceFileLink);
		}
		else
		{
			return;
		}

		if (prepass)
			return;
		if (qualityLevelsType == OptimizationDataInfo.QualityLevelsType.High)
			return;

		GameObject prefabsInstanceRoot = new GameObject ("prefabsInstanceRoot");
		EntityPool.PoolSize[] poolSizes = entityPool.poolSizes;
		for (int i = 0; i < poolSizes.Length; i++)
		{
			EntityPool.PoolSize poolSize = poolSizes [i];
			GameObject prefab = poolSize.prefab;
			if (prefab != null)
			{
				string prefabFile = AssetDatabase.GetAssetPath (prefab);
				string prefabDestinationPath = OptimizationDataEditor.Instance.GetPrefabFile (prefabFile,qualityLevelsType, true);
				if (prefabDestinationPath != null)
				{
					if (!prefabsDictionary.ContainsKey (prefabDestinationPath))
					{
						bool materialChanged = false;
						GameObject instance = ChangeMaterialToPrefab (prefab, ref materialChanged);
						if (instance != null)
						{
							instance.name = prefab.name + "_"+OptimizationDataInfo.Instance.GetSufix(qualityLevelsType);
							instance.transform.parent = prefabsInstanceRoot.transform;
							if (materialChanged)
							{
								UnityEngine.Object clonedPrefab = PrefabUtility.CreateEmptyPrefab (prefabDestinationPath);
								if (clonedPrefab != null)
								{
									GameObject updatedPrefab = PrefabUtility.ReplacePrefab(
										instance,
										clonedPrefab,
										ReplacePrefabOptions.ConnectToPrefab
										);
									poolSize.prefab = updatedPrefab;
									if (updatedPrefab != null)
									{
										poolSize.prefab = updatedPrefab;
										prefabsDictionary [prefabDestinationPath] = poolSize.prefab;

									}
									else
									{
										UnityEngine.Debug.LogError ("prefabLD is null " + instance.name + " clonedPrefab " + clonedPrefab);
									}
								}
							}
						}
					}
					else
					{
						poolSize.prefab = prefabsDictionary [prefabDestinationPath];
					}
				}
			}
		}

		GameObject.DestroyImmediate (prefabsInstanceRoot);
		PrefabUtility.ReplacePrefab (
			entityPoolprefabGameObject,
			entityPoolNewPrefab,
			ReplacePrefabOptions.ConnectToPrefab
			);
	}

	public void ProcessEntityInstance(PoolsManager poolManager,EntityPool entityPool,bool prepass)
	{
		PoolsManager.PoolEntityLinkData data=new PoolsManager.PoolEntityLinkData();

		foreach (OptimizationDataInfo.QualityLevelsType qualityLevelsType in Enum.GetValues(typeof(OptimizationDataInfo.QualityLevelsType)))
		{
			UnityEngine.Object prefab=PrefabUtility.GetPrefabParent(entityPool.gameObject);
			//UnityEngine.Object prefab=PrefabUtility.GetPrefabObject(prefabRoot);
			if(prefab!=null)
			{
			GameObject obj=PrefabUtility.InstantiatePrefab(prefab) as GameObject;

			int indexLenght=2;
			string lastDigits=obj.name.Substring(obj.name.Length-indexLenght,indexLenght);
				if(OptimizationDataInfo.Instance.IsASufix(lastDigits))
			{
			obj.name=obj.name.Substring(0,obj.name.Length-indexLenght);
			}

			ProcessPrefabs(poolManager,data,prefab,obj,qualityLevelsType,prepass);
			GameObject.DestroyImmediate(obj);
			}

		}


		poolManager.AddPrefabLinkDataFiles(data);
	}

	public void Execute ()
	{
		Execute (true);
		Execute (false);
	}

	public void Execute (bool prepass)
	{
		PoolsManager poolManager = GameObject.FindObjectOfType (typeof(PoolsManager)) as PoolsManager;
        AssetDatabase.StartAssetEditing();
		poolManager.UpdatePrefabs ();
		EntityPool[] pools = null;
		if (!prepass) {
			pools=poolManager.GetEntityPoolByPrefabs ();
			poolManager.ResetPrefabLinkDataFiles ();
		} else {
			pools = GameObject.FindObjectsOfType<EntityPool> ();
		}
		if (pools.Length > 0)
		{
			foreach (EntityPool entityPool in pools) {
				ProcessEntityInstance (poolManager, entityPool,prepass);
				GameObject.DestroyImmediate(entityPool.gameObject);
			}
		}
		EditorUtility.SetDirty (poolManager);
        AssetDatabase.StopAssetEditing();
		//EditorApplication.DirtyHierarchyWindowSorting ();
		//EditorApplication.RepaintProjectWindow ();
		//EditorApplication.RepaintHierarchyWindow ();
		AssetDatabase.SaveAssets ();
	}

	public void RestoreOriginalPrefabs()
	{
		PoolsManager poolManager = GameObject.FindObjectOfType (typeof(PoolsManager)) as PoolsManager;
		foreach(string prefabLink in poolManager.OriginalPrefabsPathFiles)
		{
			GameObject obj=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabLink)) as GameObject;
			obj.transform.parent=poolManager.transform;
		}

		poolManager.PrefabsFiles=null;
		poolManager.OriginalPrefabsPathFiles=null;
		EditorUtility.SetDirty (poolManager);
	}

	public  void RemovePrefabsInstanceAndSave ()
	{
#if UNITY_5_3_OR_NEWER
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
#else
        EditorApplication.SaveScene();
#endif
	}

	protected GameObject ChangeMaterialToPrefab (UnityEngine.Object prefab, ref bool materialChanged)
	{
		GameObject obj = PrefabUtility.InstantiatePrefab (prefab)as GameObject;
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer> ();
		if (renderers != null && renderers.Length > 0)
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				if (processRenderer (renderers [i]))
					materialChanged = true;
			}
		}
		return obj;
	}

	protected bool processRenderer (Renderer renderer)
	{
		bool materialChanged = false;
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i=0; i<sharedMaterials.Length; i++)
		{
			Material material = sharedMaterials [i];
			if (material != null)
			{
				int materialInstanceID = material.GetInstanceID ();
				if (!materialDictionary.ContainsKey (materialInstanceID))
				{
					for (int j=0; j<OptimizationDataInfo.Instance.PropertiesNameTofind.Length; j++) {
						string propertyNameTofind = OptimizationDataInfo.Instance.PropertiesNameTofind [j];
						if (material.HasProperty (propertyNameTofind))
						{
							string uniqueName = material.name + materialInstanceID;
							Material lowDetailMaterial = new Material (material);
							lowDetailMaterial.name = uniqueName + OptimizationDataInfo.LDSufix;

							Material clonedMaterial = new Material (material);
							clonedMaterial.SetTexture (propertyNameTofind, null);
							clonedMaterial.name = uniqueName;
							materialDictionary [materialInstanceID] = clonedMaterial;

							string materialName = lowDetailMaterial.name + OptimizationDataInfo.MaterialFileExtension;
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
		renderer.sharedMaterial = sharedMaterials [0];
		renderer.sharedMaterials = sharedMaterials;
		return materialChanged;
	}
	*/
}
