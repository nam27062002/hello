using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;

class SpawnerCounterToolPrefabCheck : EditorWindow
{
    int currentSceneCount;

    private string previewLine0 = "";
    private string previewLine2 = "";
    private string previewLine65 = "";
     

    Spawner[] spawnersArray;
    string[] textReadyToExport = new string[0];

    public Scene spawnerScene;

    string currentSceneName = "none";

    [MenuItem("Hungry Dragon/Balancing/Spawners Prefab Check")]

    public static void ShowWindow()
    {
		EditorWindow.GetWindow(typeof(SpawnerCounterToolPrefabCheck));
    }

    void OnGUI()
    {
        GUILayout.Label("Scene selection", EditorStyles.boldLabel);

        //Let the user select the scene in which the search should be performed
        GUI.enabled = false;
        GUI.enabled = true;
        //Button add current Scene
        if (GUILayout.Button("Add current spawner scene"))
            {
                currentSceneCount = EditorSceneManager.loadedSceneCount;
                for(int i = 0; i < currentSceneCount; i++)
                {
                    if(EditorSceneManager.GetSceneAt(i).name.Contains("SP_"))
                    {
                        spawnerScene = EditorSceneManager.GetSceneAt(i);
                        currentSceneName = spawnerScene.name;
                    }
                }
            }
        //Spawner counter
        EditorGUILayout.LabelField("Selected Scene", currentSceneName, EditorStyles.textField);
        GUILayout.Label("Spawners Counter", EditorStyles.boldLabel);

        spawnersArray = GameObject.FindObjectsOfType<Spawner>();
        GUILayout.Label("Spawners found : " + spawnersArray.Length, EditorStyles.textField);

        GUILayout.Label("Exporter", EditorStyles.boldLabel);
        //Button add all / remove all
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        //Toggles to select what to export
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();

        //Results
        if (GUILayout.Button("EXPORT LEVEL DATAS"))
        {
            textReadyToExport = new string[spawnersArray.Length+1];
            textReadyToExport = concatenateFinalArray(spawnersArray, spawnersArray.Length);
            previewLine0 = textReadyToExport[0];
            previewLine0 = textReadyToExport[2];
            previewLine0 = textReadyToExport[65];
        }
		 
        
        //Display Preview
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Preview line 1", previewLine0, EditorStyles.textField);
        EditorGUILayout.LabelField("Preview line 2", previewLine2, EditorStyles.textField);
        EditorGUILayout.LabelField("Preview line 66", previewLine65, EditorStyles.textField);
        EditorGUILayout.EndVertical();
        EditorGUILayout.LabelField("READY TO USE TEXT", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(arrayToCVSReady(textReadyToExport), EditorStyles.textField, GUILayout.Height(300));
        if (GUILayout.Button("COPY TO CLIPBOARD"))
        {
            GUIUtility.systemCopyBuffer = arrayToCVSReady(textReadyToExport);
        }

    }

    string[] concatenateFinalArray(Spawner[] initalArray, int totalObjects)
    {

        string[] finalArray = new string[totalObjects+1];
        float gpCondition = 0f;
        float timeCondition = 0f;

		finalArray[0] = "spawner;prefabs";
        for (int i = 0; i < totalObjects; i++)
        {
			finalArray [i + 1] = initalArray [i].name;
			for (int j = 0; j < initalArray [i].m_entityPrefabList.Length; j++)
			{
				finalArray [i + 1] += ";" + initalArray [i].m_entityPrefabList [j].name;
			}
			finalArray[i+1] = finalArray[i+1].Replace(".",",");
        }
        return (finalArray);
    }

    string arrayToCVSReady(string[] inputArray)
    {
        string finalString;
        finalString = "";

        for (int i = 0; i < inputArray.Length-1; i++)
        {
            finalString = finalString + inputArray[i] + "\n";
        }
        return finalString;
    }
	      
}