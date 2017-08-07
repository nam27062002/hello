using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailMeshBuilder))]
public class RailMeshBuilderEditor : Editor {

	public override void OnInspectorGUI() {		
		DrawDefaultInspector();

		if (GUILayout.Button("Build Light Map UVS")) {
			RailMeshBuilder builder = target as RailMeshBuilder;
			List<MeshFilter> meshFilters = builder.transform.FindComponentsRecursive<MeshFilter>();
			UpdateUVs(meshFilters);
		}
	}

	private void UpdateUVs(List<MeshFilter> _meshFilters) {
		foreach (MeshFilter mf in _meshFilters) {
			UnityEditor.GameObjectUtility.SetStaticEditorFlags(mf.gameObject, UnityEditor.StaticEditorFlags.LightmapStatic);		
			UnityEditor.Unwrapping.GenerateSecondaryUVSet(mf.sharedMesh);
		}
	}
}
