using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpawnerIdAssign
{
	[MenuItem("Hungry Dragon/Spawners/Check Ids")]
	static void AssignIds()
    {
		int id = 0;
		AbstractSpawner[] spawners = Object.FindObjectsOfType<AbstractSpawner>();
		List<int> ids = new List<int>();
		for (int i = 0; i < spawners.Length; i++)
        {
			if ( ids.Contains( spawners[i].GetSpawnerID() ) )
			{
				// ERROR!!
				EditorUtility.DisplayDialog( "Spawner Collision!", "This spawner "+spawners[i].name+" has a collision!", "OK");

			}
			ids.Add( spawners[i].GetSpawnerID() );
			id++;
        }

    }
}
