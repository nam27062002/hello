using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpawnerIdAssign
{
	[MenuItem("Hungry Dragon/Spawners/AssignIds")]
	static void AssignIds()
    {
		int id = 0;
		AbstractSpawner[] spawners = Object.FindObjectsOfType<AbstractSpawner>();
		for (int i = 0; i < spawners.Length; i++)
        {
			spawners[i].AssignSpawnerID( id );
			EditorSceneManager.MarkSceneDirty( spawners[i].gameObject.scene );
			id++;
        }

    }
}
