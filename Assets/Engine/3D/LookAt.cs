// LookAtPoint.cs
// 
// Created by Alger Ortín Castellví on 29/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Serialization;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Define a lookAt point for a game object (e.g. a camera).
/// The target object will always look towards the defined point.
/// If lookAtObject is defined, lookAtPoint will be linked to the position of the object
/// </summary>
[ExecuteInEditMode]
public class LookAt : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum EditMode {
		INDEPENDENT,
		LINKED,
		LOOK_AT_FOLLOWS_POSITION,
		POSITION_FOLLOWS_LOOK_AT
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// For convenience, store in local coord space (that way, if any parent is moved, the lookAt point will move accordingly
	[SerializeField] private Vector3 m_lookAtPointLocal = Vector3.forward;
	public Vector3 lookAtPointLocal {
		get { return m_lookAtPointLocal; }
		set { 
			m_lookAtPointLocal = value;
			if(m_lookAtObject != null) m_lookAtObject.position = lookAtPointGlobal;
			m_dirty = true;
		}
	}

	public Vector3 lookAtPointRelative {
		get { return (m_lookAtPointLocal - m_transform.localPosition); }
		set { lookAtPointLocal = m_transform.localPosition + value; }
	}

	public Vector3 lookAtPointGlobal {
		get {
			if(m_transform.parent != null) {
				return m_transform.parent.TransformPoint(m_lookAtPointLocal);
			} else {
				return m_lookAtPointLocal;
			}
			return m_transform.TransformPoint(m_lookAtPointLocal); 
		}
		set {
			if(m_transform.parent != null) {
				lookAtPointLocal = m_transform.parent.InverseTransformPoint(value); 
			} else {
				lookAtPointLocal = value;
			}
		}
	}

	// Optionally link to an object
	[SerializeField] private Transform m_lookAtObject = null;
	public Transform lookAtObject {
		get { return m_lookAtObject; }
		set { 
			m_lookAtObject = value; 
			if(m_lookAtObject != null) lookAtPointGlobal = m_lookAtObject.position;
		}
	}

	// Just for the editor
	[SerializeField] private EditMode m_editMode = EditMode.INDEPENDENT;
	public EditMode editMode {
		get { return m_editMode; }
	}

	// Internal
	private Transform __transform = null;
	private Transform m_transform { 
		get { 
			if (__transform == null) {
				__transform = transform;
			}
			return __transform;
		}
	}
	private bool m_dirty = true;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		m_dirty = true;
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	public void Update() {
		// Only if active!
		if(!isActiveAndEnabled) return;

		// Make sure lookAtPoint is linked to the object (if any)
		if(m_lookAtObject != null && m_lookAtObject.hasChanged) {
			lookAtPointGlobal = m_lookAtObject.position;
			m_dirty = true;
		}

		// Check also that the object transform hasn't change
		if(m_transform.hasChanged) {
			m_dirty = true;
		}

		// If cahnges occurred, modify our own rotation
		if(m_dirty) {
			m_transform.LookAt(lookAtPointGlobal);
			m_dirty = false;
		}
	}

	/// <summary>
	/// Update loop after the standard update.
	/// </summary>
	public void LateUpdate() {
		// Only if active!
		if(!isActiveAndEnabled) return;

		// Clear lookAt object's transform hasChanged flag
		// We must do it here cause other LookAt components might be using the same LookAt object and depend on its hasChanged flag
		if(m_lookAtObject != null && m_lookAtObject.hasChanged) {
			m_lookAtObject.hasChanged = false;
		}

		// Clear transform's hasChanged flag
		m_transform.hasChanged = false;
	}
}
