using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailMeshBuilder))]
public class RailMeshBuilderEditor : Editor {

	public override void OnInspectorGUI() {		
		RailMeshBuilder builder = target as RailMeshBuilder;

		DrawDefaultInspector();

		if (GUILayout.Button("Refresh Mesh")) {			
			builder.dirty = true;
		}

		if (GUILayout.Button("Build Light Map UVS")) {
			List<MeshFilter> meshFilters = builder.transform.FindComponentsRecursive<MeshFilter>();
			UpdateUVs(meshFilters);
		}

		if (builder.lightmapUVsDirty) {
			List<MeshFilter> meshFilters = builder.transform.FindComponentsRecursive<MeshFilter>();
			UpdateUVs(meshFilters);
		}
	}

	private void OnSceneGUI() {
		RailMeshBuilder builder = target as RailMeshBuilder;
		if (builder.lightmapUVsDirty) {
			List<MeshFilter> meshFilters = builder.transform.FindComponentsRecursive<MeshFilter>();
			UpdateUVs(meshFilters);
		}
	}

	private void UpdateUVs(List<MeshFilter> _meshFilters) {
		foreach (MeshFilter mf in _meshFilters) {
			UnityEditor.GameObjectUtility.SetStaticEditorFlags(mf.gameObject, UnityEditor.StaticEditorFlags.LightmapStatic);		
			UnityEditor.Unwrapping.GenerateSecondaryUVSet(mf.sharedMesh);
		}

		RailMeshBuilder builder = target as RailMeshBuilder;
		builder.lightmapUVsDirty = false;
	}
}
