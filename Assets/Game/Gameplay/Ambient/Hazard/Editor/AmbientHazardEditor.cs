// AmbientHazardEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the AmbientHazard class.
/// Will manage the collider creation and editing.
/// </summary>
[CustomEditor(typeof(AmbientHazard), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class AmbientHazardEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	AmbientHazard m_targetAmbientHazard = null;

	// Cache interesting properties
	// Cone setup will only be used if selected collider shape is Cone
	SerializedProperty m_collisionShapeProp = null;
	SerializedProperty m_coneOriginProp = null;
	SerializedProperty m_coneRotationProp = null;
	SerializedProperty m_coneLengthProp = null;
	SerializedProperty m_coneArcProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetAmbientHazard = target as AmbientHazard;

		// Initialize cached properties
		m_collisionShapeProp = serializedObject.FindProperty("m_collisionShape");
		m_coneOriginProp = serializedObject.FindProperty("m_coneOrigin");
		m_coneRotationProp = serializedObject.FindProperty("m_coneRotation");
		m_coneLengthProp = serializedObject.FindProperty("m_coneLength");
		m_coneArcProp = serializedObject.FindProperty("m_coneArc");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetAmbientHazard = null;

		// Clear cached properties
		m_collisionShapeProp = null;
		m_coneOriginProp = null;
		m_coneRotationProp = null;
		m_coneLengthProp = null;
		m_coneArcProp = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Aux vars
		bool collisionShapeChanged = false;
		bool updateConeCollider = false;

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// State timers - irrelevant if always active
			if(p.name == "m_initialState") {
				// Show "always active" checkbox based on "active duration" property
				SerializedProperty activeDurationProp = serializedObject.FindProperty("m_activeDuration");
				bool alwaysActive = activeDurationProp.floatValue < 0f;

				EditorGUI.BeginChangeCheck();
				alwaysActive = EditorGUILayout.ToggleLeft(" Always Active", alwaysActive);

				// If checked, store negative value to the "active duration" property
				if(EditorGUI.EndChangeCheck()) {
					if(alwaysActive) {
						activeDurationProp.floatValue = -1;
						p.enumValueIndex = (int)AmbientHazard.State.ACTIVATING;
					} else {
						activeDurationProp.floatValue = 5;	// Default value
						p.enumValueIndex = (int)AmbientHazard.State.IDLE;
					}
				}

				// Show related properties, indented and enabled/disabled based on alwaysActive flag
				GUI.enabled = !alwaysActive;
				EditorGUI.indentLevel++;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_initialState"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_initialDelay"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_idleDuration"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_activationDuration"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_activeDuration"));

				// Restore indentation and enabled
				GUI.enabled = true;
				EditorGUI.indentLevel--;
			}

			// Group all the collision edition stuff together
			else if(p.name == m_collisionShapeProp.name) {
				// Draw the shape property and detect changes
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(p);
				collisionShapeChanged = EditorGUI.EndChangeCheck();

				// If any of the selected hazards has no collider, show an error message and skip the rest of the setup
				bool noColliderError = false;
				for(int i = 0; i < serializedObject.targetObjects.Length; i++) {
					AmbientHazard targetHazard = (AmbientHazard)serializedObject.targetObjects[i];
					if(targetHazard.GetComponent<Collider>() == null) {
						noColliderError = true;
						break;
					}
				}

				// If shape is CUSTOM, show error box and don't do anything else
				if(noColliderError && p.enumValueIndex == (int)AmbientHazard.CollisionShape.CUSTOM) {
					EditorGUILayout.HelpBox("Some of the selected objects don't have a collider, which is required by the AmbientHazard component.\nMake sure to select a preset shape from the dropdown list above or to manually add a collider component if the selected option is \"CUSTOM\".", MessageType.Error);
				} else {
					// If there is no collider, simulate collision shape change to re-create the collider
					if(noColliderError) collisionShapeChanged = true;

					// If it's a cone, expose extra setup
					/*if(p.enumValueIndex == (int)AmbientHazard.CollisionShape.CONE) {
						// Indent in to draw the shape settings and detect changes
						EditorGUI.indentLevel++;
						EditorGUI.BeginChangeCheck();

						// Show cone properties
						EditorGUILayout.PropertyField(m_coneOriginProp);
						EditorGUILayout.PropertyField(m_coneRotationProp);
						EditorGUILayout.PropertyField(m_coneLengthProp);
						EditorGUILayout.PropertyField(m_coneArcProp);

						// Should we update the collision?
						updateConeCollider = EditorGUI.EndChangeCheck();

						// Update collider manually or automatically?
						autoUpdateCollider = EditorGUILayout.Toggle(new GUIContent("Auto-update Collider", "Turn off for better editor performance"), autoUpdateCollider);

						// Show manual update button and, if necessary, override update flag
						GUI.enabled = !autoUpdateCollider;
						bool buttonPressed = GUILayout.Button("Update Cone Collider");
						if(!autoUpdateCollider) updateConeCollider = buttonPressed;
						GUI.enabled = true;

						// Indent back out
						EditorGUI.indentLevel--;
					}*/
				}
			}

			// Properties to ignore - are already displayed manually under the "Collider" section
			else if(p.name == m_coneOriginProp.name 
				 || p.name == m_coneRotationProp.name 
				 || p.name == m_coneLengthProp.name 
				 || p.name == m_coneArcProp.name
				 || p.name == "m_initialDelay"
				 || p.name == "m_idleDuration"
				 || p.name == "m_activationDuration"
				 || p.name == "m_activeDuration") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this at the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// Perform all required updates on the collider, AFTER having applied the modified properties
		if(collisionShapeChanged) {
			CreateCollider();
		}

		if(updateConeCollider) {
			UpdateConeCollider();
		}
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new collider using current setup.
	/// </summary>
	private void CreateCollider() {
		// Support multi-selection!
		for(int i = 0; i < serializedObject.targetObjects.Length; i++) {
			// Aux vars
			AmbientHazard targetHazard = (AmbientHazard)serializedObject.targetObjects[i];

			// Destroy current colliders
			Collider[] oldColliders = targetHazard.GetComponents<Collider>();
			for(int j = 0; j < oldColliders.Length; j++) {
				GameObject.DestroyImmediate(oldColliders[j]);
				EditorGUIUtility.ExitGUI();	// We've destroyed some components within the same game object that will cause an exception when trying to draw the GUI for them
			}

			// Create new collider
			Collider newCollider = null;
			switch((AmbientHazard.CollisionShape)m_collisionShapeProp.enumValueIndex) {
				case AmbientHazard.CollisionShape.SPHERE: {
					SphereCollider sc = targetHazard.gameObject.AddComponent<SphereCollider>();
					sc.radius = 1f;	// Default size
					sc.center = new Vector3(sc.center.x, sc.radius, sc.center.z);	// Default position
					newCollider = sc;
				} break;

				case AmbientHazard.CollisionShape.CUBOID: {
					BoxCollider bc = targetHazard.gameObject.AddComponent<BoxCollider>();
					bc.size = new Vector3(1f, 6f, 1f);	// Default size
					bc.center = new Vector3(bc.center.x, bc.size.y/2f, bc.center.z);	// Default position
					newCollider = bc;
				} break;

				/*case AmbientHazard.CollisionShape.CONE: {
					// Create mesh collider
					MeshCollider mc = targetHazard.gameObject.AddComponent<MeshCollider>();
					mc.convex = true;
					newCollider = mc;

					// Force an update of the shape
					UpdateConeCollider();
				} break;*/
			}

			// Shared setup
			if(newCollider != null) {
				newCollider.isTrigger = true;
			}
		}
	}

	/// <summary>
	/// Initialize the given mesh collider with a cone mesh using the current parameters.
	/// </summary>
	private void UpdateConeCollider() {
		// Skip if selected shape is not conic
		//if((AmbientHazard.CollisionShape)m_collisionShapeProp.enumValueIndex != AmbientHazard.CollisionShape.CONE) return;
		return;

		// Support multi-selection!
		for(int i = 0; i < serializedObject.targetObjects.Length; i++) {
			// Aux vars
			AmbientHazard targetHazard = (AmbientHazard)serializedObject.targetObjects[i];

			// Get collider's mesh or create a new one if the collider has no mesh assigned yet
			MeshCollider collider = targetHazard.GetComponent<MeshCollider>();
			Mesh mesh = collider.sharedMesh;
			if(mesh == null) {
				mesh = new Mesh();
				mesh.name = "ConeMesh";
			}

			// Do some maths
			// [AOC] Black Magic from FGOL
			float corner = 2f * Mathf.Tan(Mathf.Deg2Rad * m_coneArcProp.floatValue / 2f);
			Quaternion rot = Quaternion.Inverse(Quaternion.Euler(m_coneRotationProp.vector3Value));

			// Initialize mesh vertices
			Vector3 apexPoint = m_coneOriginProp.vector3Value;
			mesh.vertices = new Vector3[5] {
				apexPoint,
				apexPoint + (rot * new Vector3(-corner, m_coneLengthProp.floatValue, -corner)),
				apexPoint + (rot * new Vector3( corner, m_coneLengthProp.floatValue, -corner)),
				apexPoint + (rot * new Vector3(-corner, m_coneLengthProp.floatValue,  corner)),
				apexPoint + (rot * new Vector3( corner, m_coneLengthProp.floatValue,  corner))
			};

			// Initialize mesh triangles
			mesh.triangles = new int[18] {
				0, 1, 2,
				0, 2, 4,
				0, 4, 3,
				0, 3, 1,
				1, 4, 2,
				1, 3, 4
			};

			// Finalize mesh and assign it to the collider
			mesh.RecalculateNormals();
			mesh.Optimize();
			collider.sharedMesh = null;
			collider.sharedMesh = mesh;
		}
	}

	//------------------------------------------------------------------------//
	// PREFERENCES															  //
	//------------------------------------------------------------------------//
	// Whether to update the collider manually via button or automatically upon change detection
	private bool autoUpdateCollider {
		get { return EditorPrefs.GetBool("AmbientHazardEditor.autoUpdateCollider", true); }
		set { EditorPrefs.SetBool("AmbientHazardEditor.autoUpdateCollider", value); }
	}
}