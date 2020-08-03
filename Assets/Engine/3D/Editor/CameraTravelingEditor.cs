// CameraTravelingEditor.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 03/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the MonoBehaviourTemplate class.
/// </summary>
[CustomEditor(typeof(CameraTraveling))]	
public class CameraTravelingEditor : Editor {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    SerializedProperty m_targetCamera, m_startingPosition, m_finalPosition, m_value;

    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//


    void OnEnable()
    {
        m_targetCamera = serializedObject.FindProperty("m_targetCamera");
        m_startingPosition = serializedObject.FindProperty("m_startingPosition");
        m_finalPosition = serializedObject.FindProperty("m_finalPosition");
        m_value = serializedObject.FindProperty("m_value");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_targetCamera);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_startingPosition);
        EditorGUILayout.PropertyField(m_finalPosition);

        if (m_targetCamera != null)
        {
            if (GUILayout.Button("Set starting pos from Camera"))
            {
                Vector3 newPos = ((Camera)m_targetCamera.objectReferenceValue).transform.position;
                m_startingPosition.vector3Value = newPos;

                serializedObject.ApplyModifiedProperties();

            }

            if (GUILayout.Button("Set final pos from Camera"))
            {
                Vector3 newPos = ((Camera)m_targetCamera.objectReferenceValue).transform.position;
                m_finalPosition.vector3Value = newPos;

                serializedObject.ApplyModifiedProperties();
            }
        }


        EditorGUILayout.Space();

        EditorGUILayout.Slider(m_value, 0f, 1f, "Value");
        if (m_targetCamera != null)
        {
            if (GUILayout.Button("Update camera pos from Value"))
            {
                // Update the camera position
                ((CameraTraveling)serializedObject.targetObject).UpdateCameraPosition();
            }
        }

        serializedObject.ApplyModifiedProperties();


    }

}