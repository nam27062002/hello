using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
        
        // All game objects regardless they are active or not      
        List<GameObject> gameObjects = GameObjectExt.FindAllGameObjects(true);

        for (var i = 0; i < gameObjects.Count; ++i) {
			GameObject go = gameObjects[i];
			if (go.name.Contains(nameSearch)) {
				GameObject newObject;
				newObject = (GameObject)EditorUtility.InstantiatePrefab(NewPrefab);
				newObject.transform.position = go.transform.position;
				newObject.transform.rotation = go.transform.rotation;
				newObject.transform.parent = go.transform.parent;
                newObject.transform.localScale = go.transform.localScale;                                
                DestroyImmediate(go);

                // The scene is marked as dirty so it's noticeable that it's been changed
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(newObject.scene);
            }
		}        
	}
}