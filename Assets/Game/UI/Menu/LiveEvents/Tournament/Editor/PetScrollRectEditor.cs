//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEditor;
using UnityEditor.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the SnapScrollRect class.
/// </summary>
[CustomEditor(typeof(PetScrollRect), true)]
[CanEditMultipleObjects]
public class PetScrollRectEditor : OptimizedScrollRectEditor { 
	private SerializedProperty m_pillPrefabs;
	private SerializedProperty m_filterButtons;

	protected override void OnEnable() {		
		base.OnEnable();

		// Acquire properties
		m_pillPrefabs = serializedObject.FindProperty("m_pillPrefabs");
		m_filterButtons = serializedObject.FindProperty("m_filterButtons");
	}

	public override void OnInspectorGUI() {		
		base.OnInspectorGUI();

		serializedObject.Update();
		EditorGUILayout.PropertyField(m_pillPrefabs, true);
		EditorGUILayout.PropertyField(m_filterButtons, true);
		serializedObject.ApplyModifiedProperties();
	}
}