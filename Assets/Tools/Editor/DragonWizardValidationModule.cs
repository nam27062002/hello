using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragonWizardValidationModule : IDragonWizard
{
    class DragonTest
    {
        public bool isTitle;
        public string text;
        public bool success;

        public DragonTest(bool _success, string _text)
        {
            isTitle = false;
            success = _success;
            text = _text;
        }

        public DragonTest(string _title)
        {
            isTitle = true;
            text = _title;
        }
    }

    readonly List<DragonTest> results = new List<DragonTest>();
    GameObject mainMenuPrefab;

    string SelectedSku
    {
        get { return DragonWizard.dragonSku[DragonWizard.dragonSkuIndex]; }
    }

    public string GetToolbarTitle()
    {
        return "Validation";
    }

    public void OnGUI()
    {
        EditorGUILayout.HelpBox("Check if dragon is correctly configured with all required points, bones, animations, etc.", MessageType.Info, true);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        DragonWizard.DrawDragonSelection();
        if (GUILayout.Button("Start validation", GUILayout.Height(35)))
        {
            ProcessTests();
        }
        EditorGUILayout.EndHorizontal();

        if (results.Count > 0)
        {
            EditorGUILayout.Space();
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].isTitle)
                    DrawTestTitleGUI(results[i].text);
                else
                    DrawTestResultGUI(results[i].success, results[i].text);
            }
        }
    }

    void ProcessTests()
    {
        results.Clear();

        MainMenuTests();
    }

    string GetMainMenuPrefabName()
    {
        string[] split = SelectedSku.Split('_');
        string first = split[0];
        string second = split[1];
        string skuUpperCase = char.ToUpper(first[0]) + first.Substring(1) + char.ToUpper(second[0]) + second.Substring(1);
        return "PF_" + skuUpperCase + "Menu";
    }

    void MainMenuTests()
    {
        // Title
        results.Add(new DragonTest("Main menu tests"));

        // Load main menu prefab
        string[] guid = AssetDatabase.FindAssets(GetMainMenuPrefabName());
        if (guid.Length != 1)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GetMainMenuPrefabName()));
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        mainMenuPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

        // Unit tests for main menu prefab
        results.Add(new DragonTest(MainMenuTestViewGameObject(), "View gameobject exists"));
        results.Add(new DragonTest(MainMenuTestAnimationController(), "Animation controller is set"));
    }

    bool MainMenuTestViewGameObject()
    {
        return mainMenuPrefab.transform.Find("view") != null ? true : false;
    }
    
    bool MainMenuTestAnimationController()
    {
        Transform view = mainMenuPrefab.transform.Find("view");
        Animator anim = view.GetComponent<Animator>();
        if (anim == null)
            return false;

        return anim.runtimeAnimatorController != null ? true : false;
    }

    void DrawTestTitleGUI(string title)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    void DrawTestResultGUI(bool success, string title)
    {
        GUIContent content = DragonWizard.GetIcon(success ? DragonWizard.IconType.TestPassed : DragonWizard.IconType.TestFailed);
        content.text = "\n " + title + "\n";
        EditorGUILayout.HelpBox(content, true);
    }
}
