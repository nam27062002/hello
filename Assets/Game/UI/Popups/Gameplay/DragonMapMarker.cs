// DragonMapMarker.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the map marker for the dragon icon.
/// </summary>
public class DragonMapMarker : MapMarker {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform m_iconRoot = null;
	[SerializeField] private Transform m_arrowPivot = null;
	[SerializeField] private Bounds m_bounds = new Bounds(Vector3.zero, new Vector3(10f, 10f, 1f));
	[Space]
	[SerializeField] private float m_scrollToDragonSpeed = 0.75f;
	[SerializeField] private float m_arrowSqrDistanceThreshold = 700f;

	// Internal
	private MapCamera m_mapCamera = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame, after all the Update() calls.
	/// </summary>
	protected void LateUpdate() {
		// Using LateUpdate so all the camera scroll logic have already been applied
		// Ignore if map camera is not initialized
		if(m_mapCamera == null) {
			m_mapCamera = InstanceManager.mapCamera;
			if(m_mapCamera == null) return;
		}

		// Ignore if map camera is not enabled (no need to wast cpu updating the markers)
		if(!m_mapCamera.camera.isActiveAndEnabled) return;

		// Snap to viewport bounds!
		// Reset marker rotation to properly compute bounds
		Transform markerTransform = GetMarkerTransform();
		Vector3 targetPos = markerTransform.position;
		Quaternion rotationBackup = markerTransform.rotation;
		markerTransform.rotation = Quaternion.identity;

		// Compute bounds viewport coordinates
		Vector3 viewportCenter = m_mapCamera.camera.WorldToViewportPoint(targetPos); // (0,0) = top-left -> (1,1) = bottom-right
		Vector3 viewportTopLeft = m_mapCamera.camera.WorldToViewportPoint(markerTransform.TransformPoint(m_bounds.min));
		Vector3 viewportBotRight = m_mapCamera.camera.WorldToViewportPoint(markerTransform.TransformPoint(m_bounds.max));

		// Compute margin to the viewport edges
		Vector3 viewportMargins = (viewportBotRight - viewportTopLeft)/2f;	// Half the size of the bounds
		viewportMargins.x = Mathf.Abs(viewportMargins.x);
		viewportMargins.y = Mathf.Abs(viewportMargins.y);

		// Compute final position!
		Vector3 snapViewportPos = new Vector3(
			Mathf.Clamp(viewportCenter.x, 0f + viewportMargins.x, 1f - viewportMargins.x),
			Mathf.Clamp(viewportCenter.y, 0f + viewportMargins.y, 1f - viewportMargins.y),
			viewportCenter.z
		);
		Vector3 iconPos = m_mapCamera.camera.ViewportToWorldPoint(snapViewportPos);

		// Restore rotation
		markerTransform.rotation = rotationBackup;

		// Apply new position to icon's transform root
		m_iconRoot.position = iconPos;

		// Compute look direction from the actual icon position to the real reference position
		Vector3 dir = targetPos - iconPos;
		dir.z = 0.0f;

		// Arrow
		if((viewportTopLeft.x < 0f 
			|| viewportTopLeft.y < 0f 
			|| viewportBotRight.x > 1f 
			|| viewportBotRight.y > 1f)
			&& (dir.sqrMagnitude > m_arrowSqrDistanceThreshold)) {
			// We're outside limits, show arrow
			m_iconRoot.gameObject.SetActive(true);

			// Arrow orientation
			// Revert parent's flip (if any), otherwise maths wont work
			if(markerTransform.localScale.x < 0) {
				m_iconRoot.localScale = new Vector3(-1f, 1f, 1f);
			} else {
				m_iconRoot.localScale = new Vector3(1f, 1f, 1f);
			}

			// Revert also parent's rotation
			m_iconRoot.rotation = Quaternion.identity;

			// Compute rotation angle
			// Correct range
			float angle = Vector3.Angle(dir, Vector3.right);
			if(dir.y < 0) {
				angle = -angle;
			}

			// Apply rotation
			m_arrowPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		} else {
			// We're within limits, hide arrow
			m_iconRoot.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Debug gizmos.
	/// </summary>
	private void OnDrawGizmos() {
		// Draw bounds
		if(m_bounds == null) return;
		if(!isActiveAndEnabled) return;
		Gizmos.matrix = this.transform.localToWorldMatrix;
		Gizmos.color = Colors.orange;
		Gizmos.DrawWireCube(m_bounds.center, m_bounds.size);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Click over the marker happened.
	/// </summary>
	private void OnMouseUpAsButton() {
		// Center view on dragon!
		Messenger.Broadcast<float>(GameEvents.UI_MAP_CENTER_TO_DRAGON, m_scrollToDragonSpeed);
	}
}