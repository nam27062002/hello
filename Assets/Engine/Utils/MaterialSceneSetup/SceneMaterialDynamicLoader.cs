using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneMaterialDynamicLoader : MonoBehaviour
{
	//TODO move to spawner level
	Dictionary<string,Material> materialDictionary = new Dictionary<string,Material> ();
	List<string>blackList=new List<string>();
	void Awake ()
	{
		// blackList.Add ("Treasure1");
		// if (DeviceQualityManager.HasBumpMap()) 
		{
#if !UNITY_EDITOR
			SwapMaterial ();
#endif
		}
	}

	public void CheckMaterials()
	{
		string stringOut = "";
		foreach (Material material in materialDictionary.Values) 
		{
			bool hasNormalMap=false;
            for (int j=0; j<OptimizationDataInfo.instance.PropertiesNameTofind.Length; j++) 
			{
				string propertyNameTofind = OptimizationDataInfo.instance.PropertiesNameTofind [j];
				if (material.HasProperty (propertyNameTofind)) 
				{
					if(material.GetTexture(propertyNameTofind)!=null)hasNormalMap=true;
				}
			}
			stringOut+="Material "+material.name+" normal present: "+hasNormalMap+"\n";
		}
		Debug.Log (stringOut);
	}
	
	void  SwapMaterial()
	{
		Debug.Log ("SwapMaterial");
		Renderer[] renderers = GameObject.FindObjectsOfType (typeof(Renderer)) as Renderer[];
		if (renderers != null && renderers.Length > 0) 
		{
			for (int i = 0; i < renderers.Length; i++) 
			{
				Renderer renderer = renderers [i];
				processRenderer(renderer);
			}
		}
		Debug.Log ("End SwapMaterial");
	}

    protected void processRenderer (Renderer renderer)
    {
        Material[] materials = renderer.sharedMaterials;
        for (int i=0; i<materials.Length; i++) 
        {
            Material material = materials [i];
            if (material != null && !blackList.Contains(material.name)) 
            {
                if (!materialDictionary.ContainsKey (material.name)) 
                {
					for (int j=0; j<OptimizationDataInfo.instance.PropertiesNameTofind.Length; j++) 
                    {
						string propertyNameTofind = OptimizationDataInfo.instance.PropertiesNameTofind [j];
                        if (material.HasProperty (propertyNameTofind)) 
                        {
                            if (!materialDictionary.ContainsKey (material.name)) 
                            {
                                string materialName = material.name;
                                string materialPath = OptimizationDataInfo.MaterialFolderName+ OptimizationDataInfo.DirectorySeparator+OptimizationDataInfo.HDSufix + OptimizationDataInfo.DirectorySeparator + materialName ;
                                Material materialLoaded = Resources.Load (materialPath, typeof(Material)) as Material;
                                if (materialLoaded != null) 
                                {
                                    materialDictionary [material.name] = materialLoaded;
                                    UnityEngine.Debug.Log("material loaded " + materialPath+" materialID "+material.GetInstanceID());
                                } 
                                else 
                                {
                                    UnityEngine.Debug.Log("material null " + materialPath+" materialID "+material.GetInstanceID());
                                    blackList.Add(material.name);
                                }
                            }
                            if (materialDictionary.ContainsKey (material.name)) 
                            {
                                materials [i] = materialDictionary [material.name];
                            }
                        }
                    }
                } 
                else 
                {
                    materials [i] = materialDictionary [material.name];
                }
                
            }
        }
        renderer.sharedMaterial = materials [0];
        renderer.sharedMaterials = materials;
    }
}
