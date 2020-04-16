// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by  on 31/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(ToggleByFlavour), true)]   // True to be used by heir classes as well
[CanEditMultipleObjects]
public class ToggleByFlavourEditor : Editor {
	//------------------------------------------------------------------------//
	// ENUMS    															  //
	//------------------------------------------------------------------------//
	public enum BoolValues {
		FALSE = 0,
		TRUE = 1
	}

	public enum ShowValues
	{
		HIDE = 0,
		SHOW = 1
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private ToggleByFlavour m_target = null;

	// Parameters
	private SerializedProperty m_scriptProp = null;
	private SerializedProperty m_settingKey = null;
	private SerializedProperty m_settingValue = null;
	private SerializedProperty m_show = null;

	private BoolValues boolValue;
	private ShowValues showValue;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {

		// Get target object
		m_target = target as ToggleByFlavour;


		m_scriptProp = serializedObject.FindProperty("m_Script");

		m_settingKey = serializedObject.FindProperty("m_settingKey");
		m_settingValue = serializedObject.FindProperty("m_settingValue");
		m_show = serializedObject.FindProperty("m_show");

	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_target = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {

		GUIContent content = new GUIContent();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Unity's "script" property
		if (m_scriptProp != null)
		{
			// Draw the property, disabled
			EditorGUI.BeginDisabledGroup(true);
			{
				EditorGUILayout.PropertyField(m_scriptProp, true);
			}
			EditorGUI.EndDisabledGroup();
		}


		content.text ="If property";
		EditorGUILayout.PropertyField(m_settingKey, content, true);


		boolValue = m_target.settingValue? BoolValues.TRUE: BoolValues.FALSE;
		BoolValues newBoolValue = (BoolValues)EditorGUILayout.EnumPopup("Equals to ", boolValue);
		m_target.settingValue =  (newBoolValue == BoolValues.TRUE) ? true : false;
        if (newBoolValue != boolValue)
        {
            // Make sure the prefab understands there was a change
			MarkAsDirty();
        }


		showValue = m_target.showValue ? ShowValues.SHOW : ShowValues.HIDE;
		ShowValues newShowValue = (ShowValues)EditorGUILayout.EnumPopup("Then ", showValue);
		m_target.showValue = (newShowValue == ShowValues.SHOW) ? true : false;
		if (newShowValue != showValue)
		{
			// Make sure the prefab understands there was a change
			MarkAsDirty();
		}


		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	public static void MarkAsDirty()
	{
#if UNITY_EDITOR

		var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
		if (prefabStage != null)
		{
			EditorSceneManager.MarkSceneDirty(prefabStage.scene);
		}
#endif
	}
}

