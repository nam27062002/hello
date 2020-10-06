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
        //Debug.Log("Validating dragon: " + DragonWizard.dragonSku[DragonWizard.dragonSkuIndex]);
        results.Clear();

        MainMenuTests();
    }

    void MainMenuTests()
    {
        results.Add(new DragonTest("Main menu tests"));
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
