﻿// SpawnerEditor.cs
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

    private readonly float INPUTDELAY = 0.5f;


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
    private SerializedProperty m_groupBonus = null; 


    // Warning messages
    private bool m_repeatedActivationTriggerTypeError = false;
	private bool m_repeatedActivationKillTriggerTypeError = false;
	private bool m_repeatedDeactivationTriggerTypeError = false;
	private bool m_incompatibleValuesError = false;

/*
    // Time of last input
    private float m_timeOfLastInput;
    public float currentTime
    {
        get { return (float)EditorApplication.timeSinceStartup; }
    }
*/
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

        m_groupBonus = serializedObject.FindProperty("m_hasGroupBonus");

        // Do an initial check for errors
        CheckForErrors();


//        m_timeOfLastInput = 0.0f;
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

	public virtual bool HasCustomPropertyDraw(SerializedProperty _p) {
		return false;
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
			if (!(HasCustomPropertyDraw(p))) {
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
						SerializedProperty min = p.FindPropertyRelative("min");
						SerializedProperty max = p.FindPropertyRelative("max");
						float ovMin = min.floatValue;
						float ovMax = max.floatValue;
	                    EditorGUI.BeginChangeCheck();
	                    EditorGUILayout.PropertyField(p, true);
	                    if (EditorGUI.EndChangeCheck())
	                    {
							if (ovMin != min.floatValue)
							{
	                            if (max.floatValue < min.floatValue)
		                        {
		                            max.floatValue = min.floatValue;
		                        }
	                        }
	/*                        else
							{
	                            if (max.floatValue < min.floatValue || (ovMin == ovMax && ((currentTime - m_timeOfLastInput) < INPUTDELAY)))
								{
									min.floatValue = max.floatValue;
	                                m_timeOfLastInput = currentTime;
								}
	                        }*/
	                    }

	                }
	                else if (isRangeRegisteredProperty(p, true))
	                {
						SerializedProperty min = p.FindPropertyRelative("min");
						SerializedProperty max = p.FindPropertyRelative("max");
						int ovMin = min.intValue;
						int ovMax = max.intValue;
	                    EditorGUI.BeginChangeCheck();
	                    EditorGUILayout.PropertyField(p, true);
	                    if (EditorGUI.EndChangeCheck())
	                    {
							if (ovMin != min.intValue)
							{
	                            if (max.intValue < min.intValue)
								{
									max.intValue = min.intValue;
								}
	                        }

	                        if (p.name == m_quantityProp.name)
	                        {
	                            m_groupBonus.boolValue = ovMin >= 5;
	                        }


	/*                        else
							{
								if (max.intValue < min.intValue || (ovMin == ovMax && ((currentTime - m_timeOfLastInput) < INPUTDELAY)))
	                            {
									min.intValue = max.intValue;
	                                m_timeOfLastInput = currentTime;
	                            }
	                        }*/
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
		foreach(Spawner.SkuKillCondition trigger1 in m_targetSpawner.activationKillTriggers) {
			foreach(Spawner.SkuKillCondition trigger2 in m_targetSpawner.activationKillTriggers) {
				// Start value is higher than end value for the same trigger type
				if (trigger1 != trigger2
				&& trigger1.sku == trigger2.sku) {
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
		m_repeatedDeactivationTriggerTypeError = false;
		foreach(Spawner.SkuKillCondition trigger1 in m_targetSpawner.deactivationKillTriggers) {
			foreach(Spawner.SkuKillCondition trigger2 in m_targetSpawner.deactivationKillTriggers) {
				// Start value is higher than end value for the same trigger type
				if (trigger1 != trigger2
				&& trigger1.sku == trigger2.sku) {
					m_repeatedDeactivationTriggerTypeError = true;
					break;	// Only show message once! :D
				}
			}

			// Break from outer loop as well
			if(m_repeatedDeactivationTriggerTypeError) break;
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