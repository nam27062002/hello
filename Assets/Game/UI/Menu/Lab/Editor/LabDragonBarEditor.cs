using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LabDragonBar))]
public class LabDragonBarEditor : Editor {
    	
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayoutExt.Separator(new SeparatorAttribute("Exp Bar Simulator"));

        LabDragonBar xpBar = target as LabDragonBar;
        if (GUILayout.Button("Build")) {
            xpBar.BuildUsingDebugValues();
        }

        if (GUILayout.Button("Destroy")) {
            xpBar.DestroyElements();
        }

        EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug"));

        if (GUILayout.Button("Rebuild from current dragon"))
        {
            xpBar.BuildFromDragonData(DragonManager.currentSpecialDragon);
        }
    }

}
