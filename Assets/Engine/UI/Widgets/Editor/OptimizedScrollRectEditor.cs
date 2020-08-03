//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the SnapScrollRect class.
/// </summary>
public class OptimizedScrollRectEditor : ScrollRectEditor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Cache serialized properties
	private SerializedProperty m_autoScrollTime;
	private SerializedProperty m_padding;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Acquire properties
		m_autoScrollTime = serializedObject.FindProperty("m_autoScrollTime");
		m_padding = serializedObject.FindProperty("m_padding");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	protected override void OnDisable() {
		// Call parent
		base.OnDisable();
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Call parent - this draws the default inspector for scroll rect
		base.OnInspectorGUI();

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Just draw our custom properties one by one
		// This keeps all custom attributes defined in each property such as Separators and Comments
		EditorGUILayout.PropertyField(m_autoScrollTime);
		EditorGUILayout.PropertyField(m_padding, true);

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}
}