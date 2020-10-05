// DragonWizard.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 22/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.using System.Collections;

using UnityEngine;
using UnityEditor;

public class DragonWizard : EditorWindow
{
	// Toolbar
	int m_toolbarInt = 0;
	static string[] m_toolbarStrings;
	static IDragonWizard[] m_modules;
	Vector2 m_scrollView;

	// Menu
	[MenuItem("Hungry Dragon/Tools/Creation/Dragon Wizard...", false)]
	static void Init()
	{
		// Prepare window
        EditorWindow window = GetWindow<DragonWizard>();
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_btn_animoji.png");
		window.titleContent = new GUIContent(" Dragon wizard", icon);

		// Prepare modules
		m_modules = new IDragonWizard[3];
		m_modules[0] = CreateInstance<DragonWizardCreateDragonModule>();
		m_modules[1] = CreateInstance<DragonWizardSkinsModule>();
		m_modules[2] = CreateInstance<DragonWizardValidationModule>();

		m_toolbarStrings = new string[m_modules.Length];
        for (int i = 0; i < m_modules.Length; i++)
        {
			m_toolbarStrings[i] = m_modules[i].GetToolbarTitle();
        }
        
		// Show window
		window.Show();
	}

	// GUI 
	void OnGUI()
	{
		// Editor checks
		if (IsEditorBusy())
			return;

        // Toolbar
		m_toolbarInt = GUILayout.Toolbar(m_toolbarInt, m_toolbarStrings, GUILayout.Height(35));
		EditorGUILayout.Space();

		m_scrollView = EditorGUILayout.BeginScrollView(m_scrollView);
		m_modules[m_toolbarInt].OnGUI();
		EditorGUILayout.EndScrollView();
	}

    bool IsEditorBusy()
    {
		bool isEditorBusy = false;
		if (EditorApplication.isCompiling)
		{
			EditorGUILayout.HelpBox("Cannot be used while the editor is compiling scripts", MessageType.Warning, true);
			isEditorBusy = true;
		}

		if (EditorApplication.isUpdating)
		{
			EditorGUILayout.HelpBox("Cannot be used while refreshing the AssetDatabase", MessageType.Warning, true);
			isEditorBusy = true;
		}

		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			EditorGUILayout.HelpBox("Cannot be used in play mode", MessageType.Warning, true);
			isEditorBusy = true;
		}

		return isEditorBusy;
	}
}
