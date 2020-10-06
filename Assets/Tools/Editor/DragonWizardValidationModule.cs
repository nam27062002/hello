using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragonWizardValidationModule : EditorWindow, IDragonWizard
{
    public string GetToolbarTitle()
    {
        return "Validation";
    }

    public void OnGUI()
    {
        EditorGUILayout.HelpBox("Check if dragon is correctly configured with all required points, bones, animations, etc.", MessageType.Info, true);
        EditorGUILayout.Space();

        TestPassedGUI("Test 1");
        TestFailedGUI("Test 2");
    }

    public void TestPassedGUI(string title)
    {
        GUIContent content = DragonWizard.GetIcon(DragonWizard.IconType.TestPassed);
        content.text = title;
        EditorGUILayout.HelpBox(content, true);
    }

    public void TestFailedGUI(string title)
    {
        GUIContent content = DragonWizard.GetIcon(DragonWizard.IconType.TestFailed);
        content.text = title;
        EditorGUILayout.HelpBox(content, true);
    }
}

