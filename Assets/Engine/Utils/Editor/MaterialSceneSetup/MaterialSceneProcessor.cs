using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

class MaterialsSceneProcessor
{
	Dictionary<string,Material> materialDictionary = new Dictionary<string,Material> ();
	List<string>blackList=new List<string>();
	public delegate bool HasToBeCheckFunction(Material material);
	public delegate Material MaterialProcessorFunction(Material material);
	public delegate void  RendererProcessor(Renderer renderer,HasToBeCheckFunction materialFilterFunction,MaterialProcessorFunction materialProcessorFunction);
		
	public void Reset ()
	{
		materialDictionary.Clear ();
		blackList.Clear ();
		// blackList.Add ("Treasure1");
	}

	public void StoreHiDetailMaterialInTheResourceFolder()
	{
		Execute (GetInfoFromMaterial,BumpMapHasToBeCheckFunction, SaveHiDetailMaterialFunctionInResourceFolder);
	}

	public void SetMaterialBumpMapToNull()
	{
		Execute (ProcessMaterial, BumpMapHasToBeCheckFunction, SetBumpMapToNull);
	}

	public void RestoreHiDetailMaterialFromTheResourceFolder()
	{
		Execute (ProcessMaterial,BumpMapHasToBeCheckFunctionAndNotInBlackList, RestoreMaterialFunctionFromResourceFolder);
	}


	public void Execute (RendererProcessor rendererProcessor,HasToBeCheckFunction materialFilterFunction,MaterialProcessorFunction materialProcessorFunction)
	{
		Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
		if (renderers != null && renderers.Length > 0) 
		{
			for (int i = 0; i < renderers.Length; i++) 
			{
				rendererProcessor (renderers [i],materialFilterFunction,materialProcessorFunction);
			}
		}
		AssetDatabase.SaveAssets ();
	}


	public bool BumpMapHasToBeCheckFunctionAndNotInBlackList(Material material)
	{
	
		return (!blackList.Contains(material.name) && BumpMapHasToBeCheckFunction(material));
	}

	public bool BumpMapHasToBeCheckFunction(Material material)
	{
		bool materialOk = false;
		for (int j=0; j<OptimizationDataInfo.instance.PropertiesNameTofind.Length; j++) 
		{
			string propertyNameTofind = OptimizationDataInfo.instance.PropertiesNameTofind [j];
			if (material.HasProperty (propertyNameTofind))materialOk=true;
		}
		return materialOk;
	}

	protected Material SaveHiDetailMaterialFunctionInResourceFolder(Material material)
	{
		string uniqueName = material.name;
		Material hiDetailMaterial = new Material (material);
		hiDetailMaterial.name = uniqueName ;
		
		string materialName = hiDetailMaterial.name + OptimizationDataInfo.MaterialFileExtension;
		string materialResourcePath = OptimizationDataInfo.MaterialResourceSaveFolderPath+ OptimizationDataInfo.DirectorySeparator+OptimizationDataInfo.HDSufix + OptimizationDataInfo.DirectorySeparator + materialName;
		AssetDatabase.CreateAsset (hiDetailMaterial, materialResourcePath);
		
		return hiDetailMaterial;
	}

	//
	protected Material RestoreMaterialFunctionFromResourceFolder(Material material)
	{
		string materialName = material.name;
		string materialPath = OptimizationDataInfo.MaterialFolderName+ OptimizationDataInfo.DirectorySeparator+OptimizationDataInfo.HDSufix  + OptimizationDataInfo.DirectorySeparator + materialName;
		Material materialLoaded = Resources.Load (materialPath, typeof(Material)) as Material;
		if (materialLoaded != null) 
		{
			material.CopyPropertiesFromMaterial(materialLoaded);
			materialDictionary [material.name] = material;
			//UnityEngine.Debug.LogError ("material loaded " + materialPath+" materialID "+material.GetInstanceID());
		} 
		else 
		{
			//UnityEngine.Debug.LogError ("material null " + materialPath+" materialID "+material.GetInstanceID());
			blackList.Add(material.name);
		}
		EditorUtility.SetDirty (material);
		return material;
	}

	protected Material SetBumpMapToNull(Material material)
	{
		for (int j=0; j<OptimizationDataInfo.instance.PropertiesNameTofind.Length; j++) 
		{
			string propertyNameTofind = OptimizationDataInfo.instance.PropertiesNameTofind [j];
			if (material.HasProperty (propertyNameTofind)) 
			{
				material.SetTexture (propertyNameTofind, null);
			}
		}
		
		return material;
	}

	
	public void GetInfoFromMaterial (Renderer renderer,HasToBeCheckFunction materialFilterFunction,MaterialProcessorFunction materialProcessorFunction)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i=0; i<sharedMaterials.Length; i++) 
		{
			Material material = sharedMaterials [i];
			if (material != null) 
			{
				if (!materialDictionary.ContainsKey (material.name)) 
				{
						if (materialFilterFunction(material)) 
							{
							materialDictionary [material.name] = materialProcessorFunction(material);
							break;
						}

				}
			}
		}
	}

	public void ProcessMaterial (Renderer renderer,HasToBeCheckFunction materialFilterFunction,MaterialProcessorFunction materialProcessorFunction)
	{
		Material[] sharedMaterials = renderer.sharedMaterials;
		for (int i=0; i<sharedMaterials.Length; i++) 
		{
			Material material = sharedMaterials [i];
			if (material != null) 
			{
				if (!materialDictionary.ContainsKey (material.name)) 
				{
					
					if (materialFilterFunction(material)) 
					{
						
						sharedMaterials [i] = materialDictionary [material.name] =materialProcessorFunction(material);
						break;
					}
					
				}
				else 
				{
					sharedMaterials [i] = materialDictionary [material.name];
				}
			}
		}
		renderer.sharedMaterial = sharedMaterials [0];
		renderer.sharedMaterials = sharedMaterials;
		UnityEditor.EditorUtility.SetDirty(renderer);
		UnityEditor.EditorUtility.SetDirty(renderer.gameObject);
	}
	

}



