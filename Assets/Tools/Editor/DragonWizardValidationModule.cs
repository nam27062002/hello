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
    }
}

