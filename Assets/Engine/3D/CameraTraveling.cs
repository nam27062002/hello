// CameraScroller.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 03/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Moves a camera between two positions based in a value [0,1]
/// </summary>
public class CameraTraveling : MonoBehaviour {

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    public Camera m_targetCamera;

    public Vector3 m_startingPosition;
    public Vector3 m_finalPosition;

    // From 0 to 1. Interpolated position between starting and final points
    public float m_value;


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    /// <summary>
    /// Updates the camera position based on the current value
    /// </summary>
    public void UpdateCameraPosition ()
    {
        if (m_targetCamera != null && m_targetCamera.isActiveAndEnabled)
        {
            m_targetCamera.transform.position = Vector3.Lerp(m_startingPosition, m_finalPosition, m_value);
        }
    }

    /// <summary>
    /// Updates the camera position based on the given value
    /// </summary>
    public void UpdateCameraPosition(float _value)
    {
        m_value = Mathf.Clamp01( _value );

        UpdateCameraPosition();
    }
    
}