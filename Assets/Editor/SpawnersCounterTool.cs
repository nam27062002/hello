using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;

class SpawnerCounter : EditorWindow
{
    int currentSceneCount;
    private bool respawnTime = false;
    private bool growthPointToAppear = false;
    private bool timeToAppear = false;
    private bool minTierToAppear = false;

    private string previewLine0 = "";
    private string previewLine2 = "";
    private string previewLine65 = "";
     

    Spawner[] spawnersArray;
    string[] textReadyToExport = new string[0];

    public Scene spawnerScene;
    string currentSceneName = "none";

    [MenuItem("Hungry Dragon/Tools/Spawners Counter")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SpawnerCounter));
    }

    void OnGUI()
    {
        GUILayout.Label("Scene selection", EditorStyles.boldLabel);

        //Let the user select the scene in which the seatch should be perfomed
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
        if (GUILayout.Button("Select all"))
        {
            respawnTime = true;
            growthPointToAppear = true;
            timeToAppear = true;
            minTierToAppear = true;

        }
        if (GUILayout.Button("Unselect all"))
        {
            respawnTime = false;
            growthPointToAppear = false;
            timeToAppear = false;
            minTierToAppear = false;

        }
        GUILayout.EndHorizontal();
        //Toggles to select what to export
        GUILayout.BeginHorizontal();
        respawnTime = GUILayout.Toggle(respawnTime, "Export Respawn Time?");
        growthPointToAppear = GUILayout.Toggle(growthPointToAppear, "Export Growth points condition?");
        timeToAppear = GUILayout.Toggle(timeToAppear, "Export Time condition?");
        minTierToAppear = GUILayout.Toggle(minTierToAppear, "Export Tier condition?");
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

        //Count needed Columns
        int countColumns = 0;
        if (respawnTime) countColumns++;
        if (growthPointToAppear) countColumns++;
        if (timeToAppear) countColumns++;
        if (minTierToAppear) countColumns++;


        finalArray[0] = "spawner_sku;entity_spawned (AVG);activation_gp;activation_time;respawn_time;activating_chance;minTierToActivate";
        for (int i = 0; i < totalObjects; i++)
        {
            //Get activation GP & time
            for (int j = 0; j < initalArray[i].m_activationTriggers.Length; j++)
            {
                // Is this condition satisfied?
                switch (initalArray[i].m_activationTriggers[j].type)
                {
                    case Spawner.SpawnCondition.Type.XP:
                        {
                            gpCondition = initalArray[i].m_activationTriggers[j].value;
                        }
                        break;

                    case Spawner.SpawnCondition.Type.TIME:
                        {
                            timeCondition = initalArray[i].m_activationTriggers[j].value;
                        }
                        break;
                }
            }

			finalArray[i+1] = initalArray[i].m_entityPrefabList[0].name + ";" + initalArray[i].m_quantity.center + ";" + gpCondition + ";" + timeCondition + ";" + initalArray[i].m_spawnTime.center + ";" + initalArray[i].m_activationChance;
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