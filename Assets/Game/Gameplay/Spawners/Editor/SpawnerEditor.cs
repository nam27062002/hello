// SpawnerEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/07/2016.
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
/// Custom editor for the Spawner class.
/// </summary>
[CustomEditor(typeof(Spawner), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SpawnerEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private Spawner m_targetSpawner = null;

	// Store a reference of interesting properties for faster access
	private SerializedProperty m_maxTierProp = null;
	private SerializedProperty m_activationTriggersProp = null;
	private SerializedProperty m_activationKillTriggersProp = null;
	private SerializedProperty m_deactivationTriggersProp = null;

    private SerializedProperty m_quantityProp = null;
    private SerializedProperty m_speedFactorRangeProp = null;
    private SerializedProperty m_scaleProp = null;
    private SerializedProperty m_spawnTimeProp = null;


    // Warning messages
    private bool m_repeatedActivationTriggerTypeError = false;
	private bool m_repeatedActivationKillTriggerTypeError = false;
	private bool m_repeatedDeactivationTriggerTypeError = false;
	private bool m_incompatibleValuesError = false;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetSpawner = target as Spawner;

		// Store a reference of interesting properties for faster access
		m_maxTierProp = serializedObject.FindProperty("m_maxTier");
		m_activationTriggersProp = serializedObject.FindProperty("m_activationTriggers");
		m_activationKillTriggersProp = serializedObject.FindProperty("m_activationKillTriggers");
		m_deactivationTriggersProp = serializedObject.FindProperty("m_deactivationTriggers");

        m_quantityProp = serializedObject.FindProperty("m_quantity");
        m_speedFactorRangeProp = serializedObject.FindProperty("m_speedFactorRange");
        m_scaleProp = serializedObject.FindProperty("m_scale");
        m_spawnTimeProp = serializedObject.FindProperty("m_spawnTime");

        // Do an initial check for errors
        CheckForErrors();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetSpawner = null;

		// Clear properties
		m_activationTriggersProp = null;
		m_activationKillTriggersProp = null;
		m_deactivationTriggersProp = null;
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
		bool checkForErrors = false;

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			if (p.name == m_maxTierProp.name) {

			} else if (p.name == "m_checkMaxTier") {
				EditorGUILayout.PropertyField(p, true);
				if (p.boolValue) {	
					EditorGUILayout.PropertyField(m_maxTierProp, true);
				}
            }
            else if (p.name == m_activationTriggersProp.name) { 
				// Draw activation properties
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(m_activationTriggersProp, true);
				EditorGUILayout.PropertyField(m_activationKillTriggersProp, true);
				EditorGUILayout.PropertyField(m_deactivationTriggersProp, true);
				if(EditorGUI.EndChangeCheck()) {
					// Check for wrong setups (once the property changes have been applied!)
					checkForErrors = true;
				}

				// Show feedback messages
				if(m_repeatedActivationTriggerTypeError) {
					EditorGUILayout.HelpBox("Two or more activation triggers are of the same type. Only the one with the lower value will be effective.", MessageType.Warning);
				}

				if(m_repeatedActivationKillTriggerTypeError) {
					EditorGUILayout.HelpBox("Two or more activation kill triggers are of the same type. Only the one with the lower value will be effective.", MessageType.Warning);
				}

				if(m_repeatedDeactivationTriggerTypeError) {
					EditorGUILayout.HelpBox("Two or more deactivation triggers are of the same type. Only the one with the lower value will be effective.", MessageType.Warning);
				}

				if(m_incompatibleValuesError) {
					EditorGUILayout.HelpBox("A deactivation trigger's value is lower than an activation trigger of the same type!\nSpawner will never be active.", MessageType.Error);
				}
			}

            else if (p.name == m_activationKillTriggersProp.name) { 
				// Do nothing
			}

            // Ignore both activation triggers (we're showing them manually)
            else if (p.name == m_deactivationTriggersProp.name) { 
				// Do nothing
			}

			// Default
			else {

                if (isRangeRegisteredProperty(p))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(p, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SerializedProperty min = p.FindPropertyRelative("min");
                        SerializedProperty max = p.FindPropertyRelative("max");
                        if (max.floatValue < min.floatValue)
                        {
//                            float t = max.floatValue;
                            max.floatValue = min.floatValue;
//                            min.floatValue = t;
                        }
                    }

                }
                else if (isRangeRegisteredProperty(p, true))
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(p, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SerializedProperty min = p.FindPropertyRelative("min");
                        SerializedProperty max = p.FindPropertyRelative("max");
                        if (max.intValue < min.intValue)
                        {
//                            int t = max.intValue;
                            max.intValue = min.intValue;
//                            min.intValue = t;
                        }
                    }

                }
                else if (p.name == m_spawnTimeProp.name)
                {
                    SerializedProperty min = p.FindPropertyRelative("min");
                    SerializedProperty max = p.FindPropertyRelative("max");
                    float ovMin = min.floatValue;
                    float ovMax = max.floatValue;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(p, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (ovMax == max.floatValue && ovMin != min.floatValue)
                        {
                            max.floatValue = min.floatValue;
                        }


                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(p, true);
                }
            }
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		EditorGUILayout.LabelField("Id: " + m_targetSpawner.GetSpawnerID());

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();

		// Check for wrong setups
		if(checkForErrors) {
			CheckForErrors();
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
	/// Checks current values of the activation/deactivation triggers looking for errors.
	/// Initializes the internal error control vars.
	/// </summary>
	private void CheckForErrors() {
		// Check duplicated activation triggers
		m_repeatedActivationTriggerTypeError = false;
		foreach(Spawner.SpawnCondition trigger1 in m_targetSpawner.activationTriggers) {
			foreach(Spawner.SpawnCondition trigger2 in m_targetSpawner.activationTriggers) {
				// Start value is higher than end value for the same trigger type
				if(trigger1 != trigger2
				&& trigger1.type == trigger2.type) {
					m_repeatedActivationTriggerTypeError = true;
					break;	// Only show message once! :D
				}
			}

			// Break from outer loop as well
			if(m_repeatedActivationTriggerTypeError) break;
		}

		m_repeatedActivationKillTriggerTypeError = false;
		foreach(Spawner.SpawnKillCondition trigger1 in m_targetSpawner.activationKillTriggers) {
			foreach(Spawner.SpawnKillCondition trigger2 in m_targetSpawner.activationKillTriggers) {
				// Start value is higher than end value for the same trigger type
				if (trigger1 != trigger2
				&& trigger1.category == trigger2.category) {
					m_repeatedActivationKillTriggerTypeError = true;
					break;	// Only show message once! :D
				}
			}

			// Break from outer loop as well
			if(m_repeatedActivationKillTriggerTypeError) break;
		}

		// Check duplicated deactivation triggers
		m_repeatedDeactivationTriggerTypeError = false;
		foreach(Spawner.SpawnCondition trigger1 in m_targetSpawner.deactivationTriggers) {
			foreach(Spawner.SpawnCondition trigger2 in m_targetSpawner.deactivationTriggers) {
				// Start value is higher than end value for the same trigger type
				if(trigger1 != trigger2
				&& trigger1.type == trigger2.type) {
					m_repeatedDeactivationTriggerTypeError = true;
					break;	// Only show message once! :D
				}
			}

			// Break from outer loop as well
			if(m_repeatedDeactivationTriggerTypeError) break;
		}

		// Check incompatible values
		m_incompatibleValuesError = false;
		foreach(Spawner.SpawnCondition startTrigger in m_targetSpawner.activationTriggers) {
			foreach(Spawner.SpawnCondition endTrigger in m_targetSpawner.deactivationTriggers) {
				// Start value is higher than end value for the same trigger type
				if(startTrigger.type == endTrigger.type
				&& startTrigger.value > endTrigger.value) {
					m_incompatibleValuesError = true;
					break;	// Only show message once! :D
				}
			}

			// Break from outer loop as well
			if(m_incompatibleValuesError) break;
		}
	}

    /// <summary>
    /// Checks for specific Range properties
    /// </summary>
    /// 
    bool isRangeRegisteredProperty(SerializedProperty property, bool isInteger = false)
    {
        return isInteger ? (property.name == m_quantityProp.name) : (property.name == m_speedFactorRangeProp.name) || (property.name == m_scaleProp.name);// || (property.name == m_spawnTimeProp.name);
    }

}