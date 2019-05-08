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
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the Spawner class.
/// </summary>
[CustomEditor(typeof(SpawnerStar), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class SpawnerStarEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private SpawnerStar m_targetSpawner = null;
	private SerializedProperty m_entityCountProp = null;



	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetSpawner = target as SpawnerStar;
		m_entityCountProp = serializedObject.FindProperty("m_quantity");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetSpawner = null;
		m_entityCountProp = null;

	}


    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(5f);
        if (GUILayout.Button("Circle Distribution")) {
            List<Vector3> points = m_targetSpawner.points;
            int pointCount = points.Count;

            // generate a default distribution (circle)
            float a = (2f * Mathf.PI) / pointCount;
            float r = 3f * pointCount / 3f;
            for (int i = 0; i < pointCount; ++i) {
                Vector3 p = points[i];

                p.x = r * Mathf.Cos(a * i);
                p.y = r * Mathf.Sin(a * i);

                points[i] = p;
            }
        }
    }
 
    /// <summary>
    /// The scene is being refreshed.
    /// </summary>
    public void OnSceneGUI() {		
        List<Vector3> points = m_targetSpawner.points;
        int entityCount = m_entityCountProp.intValue;
        int pointCount = points.Count;

        if (entityCount < 1) {
        	m_entityCountProp.intValue = 1;
        	m_entityCountProp.serializedObject.ApplyModifiedProperties();
        }

        if (entityCount != pointCount) {
            points.Resize(entityCount);

        	if (entityCount == 1) {
        		points[0] = Vector3.zero;
        	}
        }

        float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.15f;
        for (int i = 0; i < entityCount; ++i) {
            Vector3 p = Handles.FreeMoveHandle(points[i] + m_targetSpawner.transform.position, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);
            p -= m_targetSpawner.transform.position;
            p.z = 0f;

            points[i] = p;
        }

        m_targetSpawner.points = points;
        m_targetSpawner.UpdateBounds();
	}
}