using UnityEngine;
using UnityEditor;
using System.Collections;

public class SwapGameObjects : ScriptableWizard
{
	public GameObject NewPrefab;
	public string nameSearch;

	[MenuItem("Hungry Dragon/Tools/Swap GameObjects")]


	static void CreateWizard() {
		ScriptableWizard.DisplayWizard("Swap GameObjects", typeof(SwapGameObjects), "Swap");
	}

	void OnWizardCreate()
	{
		//Transform[] Replaces;
		//Replaces = Replace.GetComponentsInChildren<Transform>();

		GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();

		for (var i = 0; i < gameObjects.Length; ++i) {
			GameObject go = gameObjects[i];
			if (go.name.Contains(nameSearch)) {
				GameObject newObject;
				newObject = (GameObject)EditorUtility.InstantiatePrefab(NewPrefab);
				newObject.transform.position = go.transform.position;
				newObject.transform.rotation = go.transform.rotation;
				newObject.transform.parent = go.transform.parent;
				DestroyImmediate(go);
			}
		}

	}
}