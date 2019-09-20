using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpecialDragonBar))]
public class SpecialDragonBarEditor : Editor {
    	
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

		GUI.color = Colors.paleGreen;
		if(GUILayout.Button("Rebuild")) {
			xpBar.DestroyElements();
			xpBar.BuildUsingDebugValues();
		}
		GUI.color = Color.white;

		EditorGUILayoutExt.Separator(new SeparatorAttribute("Debug"));

        if (GUILayout.Button("Rebuild from current dragon"))
        {
            xpBar.BuildFromDragonData(DragonManager.CurrentDragon as DragonDataSpecial);
        }
    }

}
