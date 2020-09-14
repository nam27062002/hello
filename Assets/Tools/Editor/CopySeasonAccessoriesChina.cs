using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

public class CopySeasonAccessoriesChina : Editor
{
	const string PREFABS_CHINA_PATH = "Assets/Flavours/China/Art/3D/Gameplay/Entities/Prefabs/Surface";
	const string PREFABS_PATH = "Assets/Art/3D/Gameplay/Entities/Prefabs/Surface";

	[MenuItem("Hungry Dragon/Tools/Gameplay/Copy season accessories on China", false, -150)]
	static void Init()
	{
        // Extract prefab list to search
		string[] guid = AssetDatabase.FindAssets("PF_", new[] { PREFABS_CHINA_PATH });
        for (int i = 0; i < guid.Length; i++)
        {
			// Get China prefab
			string chAssetPath = AssetDatabase.GUIDToAssetPath(guid[i]);
			GameObject chinaPrefab = AssetDatabase.LoadAssetAtPath(chAssetPath, typeof(GameObject)) as GameObject;
			if (chinaPrefab == null)
				continue;

			// Check if their WW version have any seasonal accessory equipped
			string wwAssetPath = Path.Combine(PREFABS_PATH, chinaPrefab.name + ".prefab");
			GameObject wwPrefab = AssetDatabase.LoadAssetAtPath(wwAssetPath, typeof(GameObject)) as GameObject;
			if (wwPrefab == null)
				continue;

			EntityEquip entityEquip = wwPrefab.GetComponent<EntityEquip>();
			if (entityEquip == null)
				continue;

			// Copy EntityEquip from WW and paste it to China prefab
			ComponentUtility.CopyComponent(entityEquip);
			EntityEquip entityEquipChina = chinaPrefab.GetComponent<EntityEquip>();
			if (entityEquipChina == null)
				ComponentUtility.PasteComponentAsNew(chinaPrefab);
			else
				ComponentUtility.PasteComponentValues(entityEquipChina);

			Debug.Log("Copied EntityEquip to: " + chinaPrefab.name);
		}
	}
}
