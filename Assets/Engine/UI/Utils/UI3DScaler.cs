// CanvasScaleCompensator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script meant for 3D objects within a UI canvas.
/// Helper component to attach 3D objects to a rect transform and work as expected.
/// Usually makes sense that the attached rect transform is the parent of the 3D object.
/// </summary>
[ExecuteInEditMode]
public class UI3DScaler : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Tooltip("Reference rect transform which will determine the size and position of this object.")]
	[SerializeField] private RectTransform m_rectTransform = null;
	[Tooltip("Reference boundaries of the 3D object. Kind of the \"Viewport\" in which to fit the 3D object.")]
	[SerializeField] private Bounds m_referenceBounds = new Bounds(Vector3.zero, Vector3.one);
	[Space]
	[Tooltip("Highly recommended, scales all axis of the 3D object by the same amount.")]
	[SerializeField] private bool m_preserveAspect = true;
	[Tooltip("Activate only if the scaler's size is prompt to change during gameplay. No need for other basic transformations such as position, rotation or scale.")]
	[SerializeField] private bool m_constantUpdate = false;

	// Debug
	[Space]
	[SerializeField] private bool m_showBounds = true;
	[SerializeField] private Color m_debugColor = new Color(1f, 0f, 0f, 0.5f);

	// Internal
	private Vector3 m_scaleFactor = Vector3.one;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		Refresh();
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		// Min reference size values
		m_referenceBounds.size = Vector3.Max(Vector3.one * 0.001f, m_referenceBounds.size);

		// Apply new scale factor
		Refresh();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Only if live update is active
		if(m_constantUpdate) {
			Refresh();
		}
	}

	/// <summary>
	/// Debug only.
	/// </summary>
	private void OnDrawGizmosSelected() {
		// Refresh
		Refresh();

		// Show reference size box
		if(!m_showBounds) return;
		if(m_rectTransform == null) return;
		Gizmos.color = m_debugColor;
		Gizmos.matrix = this.transform.localToWorldMatrix;
		Gizmos.DrawCube(m_referenceBounds.center, m_referenceBounds.size);
		Gizmos.matrix = Matrix4x4.identity;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh object's scale to match current canvas and rectTransform.
	/// </summary>
	private void Refresh() {
		// Make sure we have all the required stuff
		if(m_rectTransform == null) return;

		// Do the magic!
		Rect scalerBounds = m_rectTransform.rect;
		m_scaleFactor.x = scalerBounds.width / m_referenceBounds.size.x;
		m_scaleFactor.y = scalerBounds.height / m_referenceBounds.size.y;
		m_scaleFactor.z = m_referenceBounds.size.z;

		// Keep aspect ratio?
		if(m_preserveAspect) {
			// Fit into scaler's bounds, find out which side of the scaler overflows the reference aspect ratio and correct it
			float referenceAspectRatio = Mathf.Clamp(m_referenceBounds.size.x / m_referenceBounds.size.y, 0.001f, 1000f);
			if(scalerBounds.height * referenceAspectRatio > scalerBounds.width) {
				m_scaleFactor.y = m_scaleFactor.x;
				m_scaleFactor.z = m_scaleFactor.x;
			} else {
				m_scaleFactor.x = m_scaleFactor.y;
				m_scaleFactor.z = m_scaleFactor.y;
			}
		}

		// Apply new scale factor!
		this.transform.localScale = m_scaleFactor;

		// Fit target into rect transform!
		Vector3 targetLocalPos = new Vector3(scalerBounds.center.x, scalerBounds.center.y, m_rectTransform.position.z) - Vector3.Scale(m_referenceBounds.center, m_scaleFactor);
		this.transform.position = m_rectTransform.localToWorldMatrix.MultiplyPoint(targetLocalPos);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}