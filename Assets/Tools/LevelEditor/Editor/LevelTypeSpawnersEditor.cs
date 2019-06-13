using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelEditor.LevelTypeSpawners))]
public class LevelTypeSpawnersEditor : Editor {


	string selected_dragon = "dragon_classic";


	private LevelEditor.LevelTypeSpawners m_target;

	// Use this for initialization
	void Awake() {
		m_target = (LevelEditor.LevelTypeSpawners)target;
	}

/*
	public override void OnInspectorGUI() {
		// Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
		bool dirty = false;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.PrefixLabel("Dragon:");

		// Dragon selector
		string[] options = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DRAGONS).ToArray();
		int oldIdx = ArrayUtility.IndexOf<string>(options, selected_dragon);
		int newIdx = EditorGUILayout.Popup(Mathf.Max(oldIdx, 0), options);	// Select first dragon if saved dragon was not found (i.e. sku changes)
		if(oldIdx != newIdx) {
			selected_dragon = options[newIdx];
			dirty = true;
		}

		int index = -1;
		for( int i =0 ; i<m_target.m_spawnsData.Count; i++ ){
			if ( m_target.m_spawnsData[i].m_dragonSku == selected_dragon ){
				index = i;
				break;
			}
		}


		if ( index < 0 ){
			index = m_target.m_spawnsData.Count;
			m_target.m_spawnsData.Add( new LevelEditor.LevelTypeSpawners.SpawnData( selected_dragon, "" ) );
			dirty = true;
		}

		LevelEditor.LevelTypeSpawners.SpawnData spData = m_target.m_spawnsData[index];

		EditorGUILayout.PrefixLabel("Spawner:");
		string str = EditorGUILayout.TextField( spData.m_prefabName );
		if ( str != spData.m_prefabName )
		{
			spData.m_prefabName = str;
			dirty = true;
		}

		if (dirty){
			EditorSceneManager.MarkSceneDirty( m_target.gameObject.scene );
		}
			
			
	}
	*/
}
