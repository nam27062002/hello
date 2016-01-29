// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Show the currently selected dragon in the menu screen.
/// TODO!! Nice animation
/// </summary>
public class MenuDragonScroller3D : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Camera m_camera = null;
	[SerializeField] private List<MenuDragonPreview> m_dragons = new List<MenuDragonPreview>();

	// Exposed setup
	[SerializeField] private float m_radius = 5f;
	[SerializeField] [Range(0f, 360f)] private float m_angleBetweenDragons = 10f;
	[SerializeField] [Range(-180f, 180f)] private float m_dragonPreviewAngle = 0f;

	// Internal
	private MenuDragonPreview m_currentDragon = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		DebugUtils.Assert(m_camera != null, "Required field!");
	}

	/// <summary>
	/// First update
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnSelectedDragonChanged);
		
		// Do a first refresh
		OnSelectedDragonChanged(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnSelectedDragonChanged);
	}

	//------------------------------------------------------------------//
	// TOOLS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Sets dragons position and rotation according to given parameters.
	/// </summary>
	public void RearrangeDragons() {
		// Go dragon by dragon
		for(int i = 0; i < m_dragons.Count; i++) {
			// Set position, distributing around the radius but respecting Y (different dragons have different heights)
			float angle = i * m_angleBetweenDragons;	// As simple as that, first dragon is at 0
			Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
			Vector3 targetPos = q * (Vector3.forward * m_radius);
			targetPos.y = m_dragons[i].transform.position.y;
			m_dragons[i].transform.position = targetPos;

			// Look towards center - but horizontally
			Vector3 targetLookAt = this.transform.position;
			targetLookAt.y = m_dragons[i].transform.position.y;
			m_dragons[i].transform.LookAt(targetLookAt, Vector3.up);

			// Apply rotation offset, subtract 90 degrees because dragon is facing towards its +X direction
			m_dragons[i].transform.Rotate(Vector3.up, m_dragonPreviewAngle - 90f, Space.Self);
		}

		// If we have a selected dragon, re-focus camera to it
		LookAtSelectedDragon();
	}

	/// <summary>
	/// Point camera towards the selected dragon (if any).
	/// </summary>
	private void LookAtSelectedDragon() {
		if(m_currentDragon != null) {
			// Keep camera looking at the same height all the time
			Vector3 targetPos = m_currentDragon.transform.position;
			LookAtPoint lookAt = m_camera.GetComponent<LookAtPoint>();
			if(lookAt != null) targetPos.y = lookAt.lookAtPoint.y;
			m_camera.transform.DOLookAt(targetPos, 0.5f).SetEase(Ease.InOutQuad);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed
	/// </summary>
	/// <param name="_sku">The sku of the selected dragon</param>
	public void OnSelectedDragonChanged(string _sku) {
		// Get dragon data from the dragon manager
		DragonData newDragonData = DragonManager.GetDragonData(_sku);

		// Get target dragon preview
		string targetSku = newDragonData.def.sku;
		MenuDragonPreview newDragon = null;
		for(int i = 0; i < m_dragons.Count; i++) {
			if(m_dragons[i].sku == targetSku) {
				newDragon = m_dragons[i];
			}
		}

		// Store new selected dragon
		m_currentDragon = newDragon;

		// Focus camera to the selected dragon
		LookAtSelectedDragon();
	}

	/// <summary>
	/// Draw gizmos.
	/// </summary>
	private void OnDrawGizmos() {
		Gizmos.matrix = transform.localToWorldMatrix;

		// Outer radius
		Gizmos.color = Colors.WithAlpha(Colors.red, 0.5f);
		Gizmos.DrawWireSphere(Vector3.zero, m_radius);

		// Center
		Gizmos.color = Colors.red;
		Gizmos.DrawSphere(Vector3.zero, 1f);

		// Z-plane
		float numRadius = 16;
		for(int i = 0; i < numRadius; i++) {
			// First radius of a different color
			if(i == 0) { 
				Gizmos.color = Colors.WithAlpha(Colors.green, 0.5f);
			} else {
				Gizmos.color = Colors.WithAlpha(Colors.orange, 0.5f);
			}

			float angle = (float)i/(float)numRadius * 360f;
			Quaternion q = Quaternion.AngleAxis(angle, Vector3.up);
			Gizmos.DrawLine(Vector3.zero, q * (Vector3.forward * m_radius));
		}

		// Camera position axis - draw Y-axis instead if camera not set
		Gizmos.color = Colors.WithAlpha(Colors.green, 0.5f);
		if(m_camera != null) {
			Gizmos.DrawLine(Vector3.zero, transform.worldToLocalMatrix * m_camera.transform.position);
		} else {
			Gizmos.DrawLine(Vector3.zero, Vector3.up * m_radius);
		}
	}
}

