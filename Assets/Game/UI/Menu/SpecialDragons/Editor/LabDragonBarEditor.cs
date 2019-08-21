using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpecialDragonBar))]
public class LabDragonBarEditor : Editor {
    	
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayoutExt.Separator(new SeparatorAttribute("Exp Bar Simulator"));

        SpecialDragonBar xpBar = target as SpecialDragonBar;
        if (GUILayout.Button("Build")) {
            xpBar.BuildUsingDebugValues();
        }

        if (GUILayout.Button("Destroy")) {
            xpBar.DestroyElements();
        }

        EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug"));

        if (GUILayout.Button("Rebuild from current dragon"))
        {
            xpBar.BuildFromDragonData(DragonManager.CurrentDragon as DragonDataSpecial);
        }
    }

}
