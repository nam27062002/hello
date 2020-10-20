using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DragonWizardValidationModule : IDragonWizard
{
    enum PrefabType
    {
        MainMenu,
        Gameplay,
        Results,
        Corpse
    }

    enum Severity
    {
        Error,
        Warning
    }

    class DragonTest
    {
        public bool isTitle;
        public string text;
        public bool success;
        public PrefabType prefabType;
        public Severity severity;

        public DragonTest(bool _success, string _text, Severity _severity = Severity.Error)
        {
            isTitle = false;
            success = _success;
            text = _text;
            severity = _severity;

            if (_success)
                testsSuccessCounter++;
            else
            {
                if (_severity == Severity.Error)
                    testsFailedCounter++;
                else
                    testWarningCounter++;
            }
        }

        public DragonTest(string _title, PrefabType _prefabType)
        {
            isTitle = true;
            text = _title;
            prefabType = _prefabType;
        }
    }

    readonly List<DragonTest> results = new List<DragonTest>();
    GameObject mainMenuPrefab;
    GameObject gameplayPrefab;
    GameObject resultsPrefab;
    GameObject corpsePrefab;
    int oldSelection;
    static int testsSuccessCounter;
    static int testsFailedCounter;
    static int testWarningCounter;

    string SelectedSku
    {
        get { return DragonWizard.dragonSku[DragonWizard.dragonSkuIndex]; }
    }

    public string GetToolbarTitle()
    {
        return "Validation";
    }

    string MainMenuPrefabName { get { return "PF_" + GetSkuUpperCase() + "Menu"; } }
    string GameplayPrefabName { get { return "PF_" + GetSkuUpperCase(); } }
    string ResultsPrefabName { get { return "PF_" + GetSkuUpperCase() + "Results"; } }
    string CorpsePrefabName { get { return "PF_" + GetSkuUpperCase() + "Corpse"; } }

    string GetSkuUpperCase()
    {
        string[] split = SelectedSku.Split('_');
        string first = split[0];
        string second = split[1];
        return char.ToUpper(first[0]) + first.Substring(1) + char.ToUpper(second[0]) + second.Substring(1);
    }

    public void OnGUI()
    {
        EditorGUILayout.HelpBox("Check if dragon is correctly configured with all required points, bones, animations, etc.", MessageType.Info, true);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        int currentSelection = DragonWizard.DrawDragonSelection();

        if (GUILayout.Button("Run tests", GUILayout.Height(35)))
        {
            oldSelection = currentSelection;
            RunTests();
        }

        EditorGUILayout.EndHorizontal();

        if (results.Count > 0)
        {
            EditorGUILayout.Space();

            if (testsFailedCounter > 0)
                EditorGUILayout.HelpBox(testsFailedCounter + " " + (testsFailedCounter == 1 ? "test" : "tests") + " failed to pass", MessageType.Error);

            if (testWarningCounter > 0)
                EditorGUILayout.HelpBox(testWarningCounter + " " + (testWarningCounter == 1 ? "test" : "tests") + " may require your attention", MessageType.Warning);
            else if (testsFailedCounter == 0)
                EditorGUILayout.HelpBox(testsSuccessCounter + " " + (testsSuccessCounter == 1 ? "test" : "tests") + " have been passed", MessageType.Info);

            EditorGUILayout.Space();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].isTitle)
                    DrawTestTitleGUI(results[i].text, results[i].prefabType);
                else
                    DrawTestResultGUI(results[i].success, results[i].text, results[i].severity);
            }

            EditorGUILayout.Space();
        }

        if (oldSelection != currentSelection)
        {
            ClearResults();
        }
    }

    void DrawTestTitleGUI(string title, PrefabType prefabType)
    {
        if (GUILayout.Button(title, DragonWizard.titleStyle))
        {
            GameObject prefab = null;
            switch (prefabType)
            {
                case PrefabType.MainMenu:
                    prefab = mainMenuPrefab;
                    break;
                case PrefabType.Gameplay:
                    prefab = gameplayPrefab;
                    break;
                case PrefabType.Results:
                    prefab = resultsPrefab;
                    break;
                case PrefabType.Corpse:
                    prefab = corpsePrefab;
                    break;
            }

            Selection.activeGameObject = prefab;
        }
    }

    void DrawTestResultGUI(bool success, string title, Severity severity)
    {
        GUIContent content = DragonWizard.GetIcon(success ? DragonWizard.IconType.TestPassed : severity == Severity.Error ? DragonWizard.IconType.TestFailed : DragonWizard.IconType.TestWarning);
        content.text = "\n " + title + "\n";
        EditorGUILayout.HelpBox(content, true);
    }

    void RunTests()
    {
        ClearResults();

        // Run prefab tests
        MainMenuTests();
        GameplayTests();
        ResultsTests();
        CorpseTests();
    }

    void ClearResults()
    {
        results.Clear();

        testsSuccessCounter = 0;
        testsFailedCounter = 0;
        testWarningCounter = 0;

        mainMenuPrefab = null;
        gameplayPrefab = null;
        resultsPrefab = null;
        corpsePrefab = null;
    }

    void MainMenuTests()
    {
        // Title
        results.Add(new DragonTest("Main menu prefab " + MainMenuPrefabName, PrefabType.MainMenu));

        // Load main menu prefab
        string[] guid = AssetDatabase.FindAssets(MainMenuPrefabName);
        if (guid.Length != 1)
        {
            results.Add(new DragonTest(false, "Asset not found: " + MainMenuPrefabName));
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        mainMenuPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

        // Unit tests for main menu prefab
        results.Add(new DragonTest(MainMenuTestViewGameObject(), "View gameobject exists"));
        results.Add(new DragonTest(MainMenuTestAnimationController(), "Animation controller is set", Severity.Warning));
        results.Add(new DragonTest(MainMenuTestDragonPreview(), "MenuDragonPreview script was added"));
        results.Add(new DragonTest(MainMenuTestSku(), "MenuDragonPreview sku matches " + SelectedSku));
        results.Add(new DragonTest(MainMenuTestDragonEquip(), "DragonEquip script was added"));
        results.Add(new DragonTest(MainMenuTestAssetBundle(), "Asset bundle prefab set to: " + SelectedSku + "_local", Severity.Warning));
        results.Add(new DragonTest(MainMenuTestBodyWingsTags(), "Body and wings tags are set"));
    }

    void GameplayTests()
    {
        // Title
        results.Add(new DragonTest("Gameplay prefab " + GameplayPrefabName, PrefabType.Gameplay));

        // Load main menu prefab
        string[] gameplayGuid = AssetDatabase.FindAssets(GameplayPrefabName);
        if (gameplayGuid.Length == 0)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GameplayPrefabName));
            return;
        }

        bool found = false;
        for (int i = 0; i < gameplayGuid.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(gameplayGuid[i]);
            if (Path.GetFileNameWithoutExtension(assetPath) == GameplayPrefabName)
            {
                found = true;
                gameplayPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                break;
            }
        }

        if (!found)
        {
            results.Add(new DragonTest(false, "Asset not found: " + GameplayPrefabName));
            return;
        }
        
        // Unit tests for gameplay prefab
        results.Add(new DragonTest(GameplayTestDragonPlayer(), "DragonPlayer script was added"));
        results.Add(new DragonTest(GameplayTestSku(), "DragonPlayer sku matches " + SelectedSku));
        results.Add(new DragonTest(GameplayTestHoldPreyPoints(), "HoldPreyPoints are set"));
        results.Add(new DragonTest(GameplayTestAnimationController(), "Animation controller is set"));
        results.Add(new DragonTest(GameplayTestSensors(), "Sensors are set"));
        results.Add(new DragonTest(GameplayTestCollisions(), "Collisions are set"));
        results.Add(new DragonTest(GameplayTestParticles(), "Particles are set"));
        results.Add(new DragonTest(GameplayTestMapMarker(), "MapMarker is set"));
        results.Add(new DragonTest(GameplayTestBodyWingsTags(), "Body and wings tags are set"));
        results.Add(new DragonTest(GameplayTestMegaFireRush(), "Mega fire rush is set"));
        results.Add(new DragonTest(GameplayTestMegaFireRushAnchor(), "Mega fire rush anchor is set", Severity.Warning));
    }

    void ResultsTests()
    {
        // Title
        results.Add(new DragonTest("Results prefab " + ResultsPrefabName, PrefabType.Results));

        // Load results prefab
        string[] guid = AssetDatabase.FindAssets(ResultsPrefabName);
        if (guid.Length != 1)
        {
            results.Add(new DragonTest(false, "Asset not found: " + ResultsPrefabName));
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        resultsPrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

        // Unit tests for results prefab
        results.Add(new DragonTest(ResultsTestAnimationController(), "Animation controller is not set", Severity.Warning));
        results.Add(new DragonTest(ResultsTestAssetBundle(), "Asset bundle prefab set to: " + SelectedSku + "_local", Severity.Warning));
        results.Add(new DragonTest(ResultsTestBodyWingsTags(), "Body and wings tags are set"));
    }

    void CorpseTests()
    {
        // Title
        results.Add(new DragonTest("Corpse prefab " + CorpsePrefabName, PrefabType.Corpse));

        // Load results prefab
        string[] guid = AssetDatabase.FindAssets(CorpsePrefabName);
        if (guid.Length != 1)
        {
            results.Add(new DragonTest(false, "Asset not found: " + CorpsePrefabName));
            return;
        }

        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        corpsePrefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

        // Unit tests for corpse prefab
        results.Add(new DragonTest(CorpseTestIsDefined(), "Corpse defined on gameplay prefab to: " + CorpsePrefabName));
    }

    #region MAIN_MENU_TESTS
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

    bool MainMenuTestBodyWingsTags()
    {
        return TestBodyWingsTags(mainMenuPrefab);
    }
    #endregion

    #region GAMEPLAY_TESTS
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

    bool GameplayTestAnimationController()
    {
        Transform view = gameplayPrefab.transform.Find("view");
        Animator anim = view.GetComponent<Animator>();
        if (anim == null)
            return false;

        return anim.runtimeAnimatorController != null ? true : false;
    }

    bool GameplayTestSensors()
    {
        Transform sensors = gameplayPrefab.transform.Find("sensors");
        if (sensors == null)
            return false;

        return sensors.childCount < 2 ? false : true;
    }

    bool GameplayTestCollisions()
    {
        Transform collisions = gameplayPrefab.transform.Find("collisions");
        if (collisions == null)
            return false;

        return collisions.childCount > 0 ? true : false;
    }

    bool GameplayTestParticles()
    {
        Transform particles = gameplayPrefab.transform.Find("particles");
        if (particles == null)
            return false;

        return particles.childCount > 0 ? true : false;
    }

    bool GameplayTestMapMarker()
    {
        Transform mapMarker = gameplayPrefab.transform.Find("MapMarker");
        if (mapMarker == null)
            return false;

        InstantiateInSeconds mapMarkerScript = mapMarker.GetComponent<InstantiateInSeconds>();
        if (mapMarkerScript == null)
            return false;

        if (mapMarkerScript.prefab == null)
            return false;

        if (mapMarkerScript.targetParent != mapMarker)
            return false;

        return true;
    }

    bool GameplayTestBodyWingsTags()
    {
        return TestBodyWingsTags(gameplayPrefab);
    }

    bool GameplayTestMegaFireRush()
    {
        Transform particles = gameplayPrefab.transform.Find("particles");
        if (particles == null)
            return false;

        DragonParticleController dragonParticleController = particles.GetComponent<DragonParticleController>();
        if (dragonParticleController == null)
            return false;

        return !string.IsNullOrEmpty(dragonParticleController.m_megaFireRush);
    }

    bool GameplayTestMegaFireRushAnchor()
    {
        Transform particles = gameplayPrefab.transform.Find("particles");
        if (particles == null)
            return false;

        DragonParticleController dragonParticleController = particles.GetComponent<DragonParticleController>();
        if (dragonParticleController == null)
            return false;

        return dragonParticleController.m_megaFireRushAnchor != null;
    }
    #endregion

    #region RESULTS_TESTS
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

    bool ResultsTestBodyWingsTags()
    {
        return TestBodyWingsTags(resultsPrefab);
    }
    #endregion

    #region CORPSE_TESTS
    bool CorpseTestIsDefined()
    {
        DragonParticleController particleController = gameplayPrefab.FindComponentRecursive<DragonParticleController>();
        if (particleController == null)
            return false;

        return particleController.m_corpseAsset == CorpsePrefabName;
    }
    #endregion

    #region HELPER_TESTS
    bool TestBodyWingsTags(GameObject prefab)
    {
        Transform view = prefab.transform.Find("view");
        if (view == null)
            return false;

        bool wingsTag = false;
        bool bodyTag = false;

        List<SkinnedMeshRenderer> renderers = view.FindComponentsRecursive<SkinnedMeshRenderer>();
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].CompareTag("DragonBody"))
                bodyTag = true;
            else if (renderers[i].CompareTag("DragonWings"))
                wingsTag = true;
        }

        if (renderers.Count == 1 && (bodyTag || wingsTag))
            return true;

        return wingsTag && bodyTag;
    }
    #endregion
}
