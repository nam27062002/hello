using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class Startup {
    static Startup()
    {
		ReloadInitReferences();
    }

	private static InitRefObject CreateInitialReferences()
    {
		InitRefObject refObject = (InitRefObject)ScriptableObject.CreateInstance(typeof(InitRefObject));

		if(refObject != null)
		{
			AssetDatabase.CreateAsset(refObject, "Assets/Resources/InitReferences.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
			return refObject;
        }

        return null;
    }

	[MenuItem("Hungry Dragon/Tools/## Reload Init References")]
    public static void ReloadInitReferences()
    {
		// Update texts and content references
		InitRefObject referencesObject = (InitRefObject)Resources.Load("InitReferences");
		if ( referencesObject == null )
		{
			referencesObject = CreateInitialReferences();
		}

		if ( referencesObject != null )
		{
			// Update values!!
			string resourcesPath = Application.dataPath + "/Resources/";

			referencesObject.m_settings = (CaletySettings)Resources.Load("CaletySettings");

			// Rules
			string rulesPath = resourcesPath + "Rules/";
			referencesObject.m_definitions.Clear();
			string[] rules = Directory.GetFiles(rulesPath, "*.xml");
			foreach (string rule in rules) 
			{
				string r = rule.Substring(resourcesPath.Length);
				r = r.Substring(0, r.Length - 4);
				TextAsset t = Resources.Load<TextAsset>( r );
				if ( t != null )
				{
					referencesObject.m_definitions.Add( t );
				}
			}

			// Localization
			string localizationPath = resourcesPath + "Localization/";
			referencesObject.m_languages.Clear();
			string[] files = Directory.GetFiles(localizationPath, "*.txt");
			foreach (string file in files) 
			{
				string r = file.Substring(resourcesPath.Length);
				r = r.Substring(0, r.Length - 4);
				TextAsset t = Resources.Load<TextAsset>( r );
				if ( t != null )
				{
					referencesObject.m_languages.Add( t );
				}
			}

			// Save value
			EditorUtility.SetDirty( referencesObject );
		}
    }
}