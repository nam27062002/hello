using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragonWizardSkinsModule : IDragonWizard
{
    public string GetToolbarTitle()
    {
        return "Skins";
    }

    public void OnGUI()
    {
        EditorGUILayout.HelpBox("TODO", MessageType.Warning, true);
    }
}

