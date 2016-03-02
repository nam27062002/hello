// IncubatorEggController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single egg on the incubator menu.
/// </summary>
public class IncubatorEggController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data
	private Egg m_eggData = null;
	public Egg eggData {
		get { return m_eggData; }
		set { m_eggData = value; }
	}

	// External references
	private Camera m_camera = null;
	private IncubatorEggAnchor m_incubatorAnchor = null;

	// Backup some values while dragging
	private Vector3 m_originalPos = Vector3.zero;
	private Transform m_originalParent = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Start() {
		// Search the anchor point of the incubator
		m_incubatorAnchor = GameObject.FindObjectOfType<IncubatorEggAnchor>();
		Debug.Assert(m_incubatorAnchor != null, "Eggs shouldn't be instantiated outside the incubator scene!");

		// Get 3D canera
		m_camera = GameObject.Find("Camera3D").GetComponent<Camera>();

		// Subscribe to external events
		Messenger.AddListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether the egg at its current position should be snapped to the anchor.
	/// </summary>
	/// <returns><c>true</c> if the egg can be snapped to the anchor; otherwise, <c>false</c>.</returns>
	private bool CanSnap() {
		// Impossible if incubator is already busy
		if(!EggManager.isIncubatorAvailable) return false;

		// Is it close enough to the anchor?
		float dist = m_incubatorAnchor.transform.position.Distance(transform.position);
		return (dist <= m_incubatorAnchor.snapDistance);
	}

	/// <summary>
	/// Check whether this egg can be dragged or not.
	/// </summary>
	/// <returns><c>true</c> if this egg can be dragged; otherwise, <c>false</c>.</returns>
	private bool CanDrag() {
		// Can only be dragged while in inventory
		return eggData.state == Egg.State.STORED;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Input has started on this object.
	/// </summary>
	public void OnMouseDown() {
		// Skip if dragging is not possible
		if(!CanDrag()) return;

		// Store some values
		m_originalPos = transform.position;
		m_originalParent = transform.parent;

		// Move egg to the world (layer and parent)
		gameObject.SetLayerRecursively("Default");
		transform.SetParent(m_incubatorAnchor.transform.parent, false);
	}

	/// <summary>
	/// A drag movement started on this object is moving.
	/// </summary>
	public void OnMouseDrag() {
		// Skip if dragging is not possible
		if(!CanDrag()) return;

		// Object follows the cursor at anchor's depth
		float anchorDistToCamera = m_camera.transform.position.Distance(m_incubatorAnchor.transform.position);
		transform.position = m_camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, anchorDistToCamera));

		// Snap to anchor if possible
		if(CanSnap()) {
			transform.position = m_incubatorAnchor.transform.position;
		}
	}

	/// <summary>
	/// Input started on this object has been released.
	/// </summary>
	public void OnMouseUp() {
		// Skip if dragging is not possible
		if(!CanDrag()) return;

		// Start incubating?
		bool incubating = false;
		if(CanSnap()) {
			// Dropped onto the incubator! Start incubating
			// Double check in case the manager doesn't allow us to do it
			incubating = EggManager.PutEggToIncubator(eggData);

			// If successful, save persistence
			if(incubating) PersistenceManager.Save();
		}

		// Either snap to incubator anchor or go back to original position
		if(incubating) {
			transform.position = m_incubatorAnchor.transform.position;
		} else {
			// Move back to original position, parent and layer
			gameObject.SetLayerRecursively("3dOverUI");
			transform.SetParent(m_originalParent, false);
			transform.position = m_originalPos;
		}
	}

	/// <summary>
	/// An egg has been collected!
	/// </summary>
	/// <param name="_egg">The egg that has been collected.</param>
	private void OnEggCollected(Egg _egg) {
		// If it matches this egg, destroy ourselves
		if(_egg == this.eggData) {
			GameObject.Destroy(this.gameObject);
		}
	}
}

