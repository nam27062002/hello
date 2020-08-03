using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEditor;

[System.Serializable]
public class DragonCommonSettings : ScriptableObject
{

	[SerializeField]
	public AnimationCurve m_reviveScaleCurve;


	/*
	[MenuItem("Assets/Create/DragonCommonSettings")]
	public static DragonCommonSettings  Create()
    {
		DragonCommonSettings asset = ScriptableObject.CreateInstance<DragonCommonSettings>();

		AssetDatabase.CreateAsset(asset, "Assets/DragonCommonSettings.asset");
        AssetDatabase.SaveAssets();
        return asset;
    }
    */
}
