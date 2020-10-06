using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragonWizardValidationModule : IDragonWizard
{
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
            ProcessValidation();
        }
        EditorGUILayout.EndHorizontal();
    }

    void ProcessValidation()
    {
        Debug.Log("Validating dragon: " + DragonWizard.dragonSku[DragonWizard.dragonSkuIndex]);
    }

    public void TestPassedGUI(string title)
    {
        GUIContent content = DragonWizard.GetIcon(DragonWizard.IconType.TestPassed);
        content.text = "\n " + title + "\n";
        EditorGUILayout.HelpBox(content, true);
    }

    public void TestFailedGUI(string title)
    {
        GUIContent content = DragonWizard.GetIcon(DragonWizard.IconType.TestFailed);
        content.text = "\n " + title + "\n";
        EditorGUILayout.HelpBox(content, true);
    }
}

