
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OptimizeScene))]
public class OptimizeSceneEditor : Editor
{

	public override void OnInspectorGUI()
	{		
		if (GUILayout.Button("Optimize"))
		{
			(target as OptimizeScene).DoOptimize();
		}
	}
}
