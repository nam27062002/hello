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
}
