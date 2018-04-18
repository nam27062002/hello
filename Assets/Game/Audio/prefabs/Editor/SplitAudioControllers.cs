using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

public class SplitAudioControllers : MonoBehaviour {

	[MenuItem("Hungry Dragon/Split AudioController")]
	public static void SpliAudioControllers()
	{
		const string prefabsFolder = "Assets/Game/Audio/prefabs/";
		const string textFilesFolder = "Assets/Game/Audio/prefabs/";

		string[] files = new string[]{ "village.txt","castle.txt","dark.txt" };
		string[] prefabNames = new string[]{ "AudioController_Gameplay_Village.prefab","AudioController_Gameplay_Castle.prefab","AudioController_Gameplay_Dark.prefab" };
		int max = files.Length;
		for (int i = 0; i < max; i++) 
		{
			// Duplicate original prefab
			if (AssetDatabase.CopyAsset( prefabsFolder + "AudioController_Gameplay.prefab", prefabsFolder + prefabNames[i]))
			{
				GameObject go = AssetDatabase.LoadAssetAtPath( prefabsFolder + prefabNames[i],typeof(GameObject)) as GameObject;
				if ( go != null )
				{
					// Remove all unused audios
					AudioController audioController = go.GetComponent<AudioController>();
					List<string> regexs = new List<string>{};

					using (StreamReader reader = new StreamReader(textFilesFolder + files[i]))
		            {
						while (reader.Peek () >= 0)
		                {
							string line = reader.ReadLine ();
							if ( !string.IsNullOrEmpty(line) )
								regexs.Add( line );
		                }
		            }

					int maxCategories = audioController.AudioCategories.Length;
					for (int categoryIndex = 0; categoryIndex < maxCategories; categoryIndex++) 
					{
						AudioCategory audioCategory = audioController.AudioCategories[categoryIndex];
						int maxAudioItems = audioCategory.AudioItems.Length;
						for (int itemIndex = maxAudioItems - 1; itemIndex >= 0; itemIndex--) 
						{
							string id = audioCategory.AudioItems[itemIndex].Name;
							bool valid = false;
							for (int regexIndex = 0; regexIndex < regexs.Count && !valid; regexIndex++) {
								valid = Regex.IsMatch( id, regexs[regexIndex]);
							}

							// if id is not in list then remove it
							if (!valid)
							{
								ArrayHelper.DeleteArrayElement( ref audioCategory.AudioItems, itemIndex );
							}
						}
					}

					// Save
					EditorUtility.SetDirty( go );
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
				else
				{
					Debug.Log("Cannot load " + prefabNames[i]);
				}
			}
			else
			{
				Debug.Log("Cannot duplicate AudioController_Gameplay to " + prefabNames[i]);
			}

		}
		
	}
}
