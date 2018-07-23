using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;

class SpawnerCounterToolExtended : EditorWindow
{
    int currentSceneCount;

    private string previewLine0 = "";
    private string previewLine2 = "";
    private string previewLine65 = "";
     

    Spawner[] spawnersArray;
    string[] textReadyToExport = new string[0];

    public Scene spawnerScene;

    string currentSceneName = "none";

    [MenuItem("Hungry Dragon/Balancing/Spawners All Info")]

    public static void ShowWindow()
    {
		EditorWindow.GetWindow(typeof(SpawnerCounterToolExtended));
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

		finalArray[0] = "spawner_sku;entity_spawned (AVG);respawn_time;activating_chance;xp;total_xp;agressive;min_entities;max_entities;hasBonus";
        for (int i = 0; i < totalObjects; i++)
        {
			if ((initalArray [i].m_entityPrefabList [0].name == "Junk/PF_BadJunkEye") || (initalArray [i].m_entityPrefabList [0].name == "Junk/PF_BadJunkFrog") || (initalArray [i].m_entityPrefabList [0].name == "Junk/PF_BadJunkMagicBottle"))
				finalArray [i + 1] = "Junk/PF_BadJunkBone;" + (((decimal)(initalArray [i].m_quantity.min + initalArray [i].m_quantity.max) / 2)) + ";" + initalArray [i].m_spawnTime.center + ";" + initalArray [i].m_activationChance + ";" + ";;;" + initalArray [i].m_quantity.min + ";" + initalArray [i].m_quantity.max + ";" + initalArray [i].HasGroupBonus;
			else {
				if ((initalArray [i].m_entityPrefabList [0].name == "Surface/PF_RatSmall") || (initalArray [i].m_entityPrefabList [0].name == "Surface/PF_RatSmall_Static") || (initalArray [i].m_entityPrefabList [0].name == "Surface/PF_RatSmall_Wall"))
					finalArray [i + 1] = "Surface/PF_Rat;" + (((decimal)(initalArray [i].m_quantity.min + initalArray [i].m_quantity.max) / 2)) + ";" + initalArray [i].m_spawnTime.center + ";" + initalArray [i].m_activationChance + ";" + ";;;" + initalArray [i].m_quantity.min + ";" + initalArray [i].m_quantity.max + ";" + initalArray [i].HasGroupBonus;
				else
					finalArray [i + 1] = initalArray [i].m_entityPrefabList [0].name + ";" + (((decimal)(initalArray [i].m_quantity.min + initalArray [i].m_quantity.max) / 2)) + ";" + initalArray [i].m_spawnTime.center + ";" + initalArray [i].m_activationChance + ";" + ";;;" + initalArray [i].m_quantity.min + ";" + initalArray [i].m_quantity.max + ";" + initalArray [i].HasGroupBonus;
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