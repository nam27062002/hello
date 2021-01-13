using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Catapult))]
public class CatapultEditor : Editor {
	private Catapult m_targetCatapult = null;

	private SerializedProperty m_angleProp = null;
	private SerializedProperty m_initialVelocityProp = null;



	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetCatapult = target as Catapult;

		// Store a reference of interesting properties for faster access
		m_angleProp = serializedObject.FindProperty("m_angle");
		m_initialVelocityProp = serializedObject.FindProperty("m_initialVelocity");
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff


	}
}
