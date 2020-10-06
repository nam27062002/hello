﻿// DragonWizard.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 22/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.using System.Collections;

using UnityEngine;
using UnityEditor;

public class DragonWizard : EditorWindow
{
	public enum IconType
	{
		TestPassed,
		TestFailed
	}

	public static class Icons
	{
		public static GUIContent testPassed = EditorGUIUtility.IconContent("TestPassed");
		public static GUIContent testFailed = EditorGUIUtility.IconContent("TestFailed");
	}

	public static GUIContent GetIcon(IconType iconType)
	{
		switch (iconType)
		{
			case IconType.TestPassed:
				return Icons.testPassed;
			case IconType.TestFailed:
				return Icons.testFailed;
			default:
				break;
		}

		return new GUIContent();
	}

	// Toolbar
	int m_toolbarInt = 0;
	string[] m_toolbarStrings;
	IDragonWizard[] m_modules;
	Vector2 m_scrollView;

	// Menu
	[MenuItem("Hungry Dragon/Tools/Creation/Dragon Wizard...", false)]
	static void Init()
	{
		// Prepare window
        EditorWindow window = GetWindow<DragonWizard>();
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_btn_animoji.png");
		window.titleContent = new GUIContent(" Dragon wizard", icon);

		// Show window
		window.Show();
	}

    void OnEnable()
    {
		// Prepare modules
		m_modules = new IDragonWizard[3];
		m_modules[0] = new DragonWizardCreateDragonModule();
		m_modules[1] = new DragonWizardSkinsModule();
		m_modules[2] = new DragonWizardValidationModule();

		m_toolbarStrings = new string[m_modules.Length];
		for (int i = 0; i < m_modules.Length; i++)
		{
			m_toolbarStrings[i] = m_modules[i].GetToolbarTitle();
		}
	}

    void OnDestroy()
    {
		m_modules = null;
		m_toolbarStrings = null;
    }

    // GUI 
    public void OnGUI()
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

	public static DragonWizardXML GetDragonWizardXMLWindow()
	{
        return GetWindow<DragonWizardXML>();
	}
}
