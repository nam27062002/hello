using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SeasonalSpawner), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SeasonalSpawnerEditor : SpawnerEditor {

	public override bool HasCustomPropertyDraw(SerializedProperty _p) {
		if (_p.name == "m_entityPrefabList") {

			SerializedProperty sc = serializedObject.FindProperty("m_spawnConfigs");
			EditorGUILayout.PropertyField(sc, true);

			return true;
		} else if (_p.name == "m_spawnConfigs") {

			return true;
		}
		return false;
	}
}
