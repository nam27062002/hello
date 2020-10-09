using System.Collections.Generic;
using System.IO;
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
    GameObject gameplayPrefab;
    GameObject resultsPrefab;

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
        if (GUILayout.Button("Run tests", GUILayout.Height(35)))
        {
            RunTests();
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

    void RunTests()
    {
        results.Clear();

        // Run prefab tests
        MainMenuTests();
        GameplayTests();
        ResultsTests();
    }

    string GetSkuUpperCase()
    {
        string[] split = SelectedSku.Split('_');
        string first = split[0];
        string second = split[1];
        return char.ToUpper(first[0]) + first.Substring(1) + char.ToUpper(second[0]) + second.Substring(1);
    }

    string GetMainMenuPrefabName()
    {
        return "PF_" + GetSkuUpperCase() + "Menu";
    }

    string GetGameplayPrefabName()
    {
        return "PF_" + GetSkuUpperCase();
    }

    string GetResultsPrefabName()
    {
        return "PF_" + GetSkuUpperCase() + "Results";
    }

    void MainMenuTests()
    {
        // Title
        results.Add(new DragonTest("Main menu prefab " + GetMainMenuPrefabName()));

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
        results.Add(new DragonTest(MainMenuTestDragonPreview(), "MenuDragonPreview script was added"));
        results.Add(new DragonTest(MainMenuTestSku(), "MenuDragonPreview sku matches " + SelectedSku));
        results.Add(new DragonTest(MainMenuTestDragonEquip(), "DragonEquip script was added"));
        results.Add(new DragonTest(MainMenuTestAssetBundle(), "Asset bundle prefab set to: " + SelectedSku + "_local"));
    }

    void GameplayTests()
    {
        // Title
        results.Add(new DragonTest("Gameplay prefab " + GetGameplayPrefabName()));

        // Load main menu prefab
        string[] gameplayGuid = AssetDatabase.FindAssets(GetGameplayPrefabName());
        if (gameplayGuid.Length == 0)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GetGameplayPrefabName()));
            return;
        }

        bool found = false;
        for (int i = 0; i < gameplayGuid.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(gameplayGuid[i]);
            if (Path.GetFileNameWithoutExtension(assetPath) == GetGameplayPrefabName())
            {
                found = true;
                gameplayPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                break;
            }
        }

        if (!found)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GetGameplayPrefabName()));
            return;
        }
        
        // Unit tests for gameplay prefab
        results.Add(new DragonTest(GameplayTestDragonPlayer(), "DragonPlayer script was added"));
        results.Add(new DragonTest(GameplayTestSku(), "DragonPlayer sku matches " + SelectedSku));
        results.Add(new DragonTest(GameplayTestHoldPreyPoints(), "HoldPreyPoints are set"));
    }

    void ResultsTests()
    {
        // Title
        results.Add(new DragonTest("Results prefab " + GetResultsPrefabName()));

        // Load results prefab
        string[] guid = AssetDatabase.FindAssets(GetResultsPrefabName());
        if (guid.Length != 1)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GetResultsPrefabName()));
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        resultsPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

        // Unit tests for results prefab
        results.Add(new DragonTest(ResultsTestAnimationController(), "Animation controller is not set"));
        results.Add(new DragonTest(ResultsTestAssetBundle(), "Asset bundle prefab set to: " + SelectedSku + "_local"));
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

    bool MainMenuTestAssetBundle()
    {
        string assetPath = AssetDatabase.GetAssetPath(mainMenuPrefab);
        return AssetImporter.GetAtPath(assetPath).assetBundleName == SelectedSku + "_local";
    }

    bool MainMenuTestDragonPreview()
    {
        return mainMenuPrefab.GetComponent<MenuDragonPreview>() != null ? true : false;
    }

    bool MainMenuTestDragonEquip()
    {
        return mainMenuPrefab.GetComponent<DragonEquip>() != null ? true : false;
    }

    bool MainMenuTestSku()
    {
        if (!MainMenuTestDragonPreview())
            return false;

        MenuDragonPreview menuDragonPreview = mainMenuPrefab.GetComponent<MenuDragonPreview>();
        return menuDragonPreview.sku == SelectedSku;
    }

    bool GameplayTestDragonPlayer()
    {
        return gameplayPrefab.GetComponent<DragonPlayer>() != null ? true : false;
    }

    bool GameplayTestSku()
    {
        if (!GameplayTestDragonPlayer())
            return false;

        DragonPlayer dragonPlayer = gameplayPrefab.GetComponent<DragonPlayer>();
        return dragonPlayer.sku == SelectedSku;
    }

    bool GameplayTestHoldPreyPoints()
    {
        Transform points = gameplayPrefab.FindTransformRecursive("points");
        if (points == null)
            return false;

        HoldPreyPoint[] holdPreyPoints = points.GetComponentsInChildren<HoldPreyPoint>();
        return holdPreyPoints.Length > 0;
    }

    bool ResultsTestAnimationController()
    {
        Transform view = resultsPrefab.transform.Find("view");
        Animator anim = view.GetComponent<Animator>();
        if (anim == null)
            return false;

        return anim.runtimeAnimatorController == null ? true : false;
    }

    bool ResultsTestAssetBundle()
    {
        string assetPath = AssetDatabase.GetAssetPath(resultsPrefab);
        return AssetImporter.GetAtPath(assetPath).assetBundleName == SelectedSku + "_local";
    }

    void DrawTestTitleGUI(string title)
    {
        EditorGUILayout.LabelField(title, DragonWizard.titleStyle);
    }

    void DrawTestResultGUI(bool success, string title)
    {
        GUIContent content = DragonWizard.GetIcon(success ? DragonWizard.IconType.TestPassed : DragonWizard.IconType.TestFailed);
        content.text = "\n " + title + "\n";
        EditorGUILayout.HelpBox(content, true);
    }
}
