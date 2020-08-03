using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BatchReferenceSetter {

	[MenuItem("Hungry Dragon/Spawners/Assign References")]
	static void AssignReferences()
    {
		string[] assetPaths = AssetDatabase.GetAllAssetPaths();
		for (int i = 0; i < assetPaths.Length; i++)
		{
			if ( assetPaths[i].EndsWith(".prefab") )
			{
				string prefab = assetPaths[i];
				GameObject go = AssetDatabase.LoadMainAssetAtPath( prefab ) as GameObject;
				if ( go != null )
				{
					ViewControl[] vcs = go.GetComponentsInChildren<ViewControl>(true);
					for (int j = 0; j < vcs.Length; j++)
					{
						vcs[j].GetReferences();
					}

					IEntity[] ies = go.GetComponentsInChildren<IEntity>(true);
					for (int j = 0; j < ies.Length; j++)
					{
						ies[j].GetReferences();
					}

					EditorUtility.SetDirty(go);
				}
			}
		}
		AssetDatabase.SaveAssets();

    }

}

