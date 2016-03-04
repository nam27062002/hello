// IncubatorEggBehaviour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single egg on the incubator menu.
/// </summary>
[RequireComponent(typeof(EggController))]
public class IncubatorEggBehaviour : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// External references
	private IncubatorEggAnchor m_incubatorAnchor = null;
	private IncubatorWarningMessage m_warningMessage = null;

	// Backup some values while dragging
	private Vector3 m_originalPos = Vector3.zero;
	private Transform m_originalParent = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// If we are not at the menu scene, disable this component
		MenuSceneController sceneController = InstanceManager.GetSceneController<MenuSceneController>();
		if(sceneController == null) {
			this.enabled = false;
			return;
		}

		// Get incubator screen and 3D scene
		NavigationScreen incubatorScreen = sceneController.screensController.GetScreen((int)MenuScreens.INCUBATOR);
		MenuScreenScene incubatorScene = sceneController.screensController.GetScene((int)MenuScreens.INCUBATOR);

		// Search the anchor point of the incubator
		m_incubatorAnchor = incubatorScene.FindComponentRecursive<IncubatorEggAnchor>();

		// Search the UI warning message as well
		m_warningMessage = incubatorScreen.FindComponentRecursive<IncubatorWarningMessage>();
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether the egg at its current position should be snapped to the anchor.
	/// </summary>
	/// <param name="_triggerWarning">Whether to show warning message if snapping is not possible.</param>
	/// <returns><c>true</c> if the egg can be snapped to the anchor; otherwise, <c>false</c>.</returns>
	private bool CanSnap(bool _triggerWarning) {
		// Is it close enough to the anchor?
		float dist = m_incubatorAnchor.transform.position.Distance(transform.position);
		bool withinDistance = (dist <= m_incubatorAnchor.snapDistance);

		// Impossible if incubator is already busy
		if(!EggManager.isIncubatorAvailable) {
			// Show message if we're in snapping range
			if(_triggerWarning && withinDistance) m_warningMessage.Show(true);
			return false;
		}

		// Hide message if we're outside snapping range
		if(!withinDistance && _triggerWarning) {
			m_warningMessage.Show(false);
		}

		return withinDistance;
	}

	/// <summary>
	/// Move the egg around following cursor's position (touch in mobile devices).
	/// </summary>
	private void FollowCursor() {
		// Object follows the cursor at anchor's depth
		float anchorDistToCamera = Camera.main.transform.position.Distance(m_incubatorAnchor.transform.position);
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, anchorDistToCamera));
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Input has started on this object.
	/// </summary>
	public void OnMouseDown() {
		// Ignore if component is disabled
		if(!enabled) return;

		// Store some values
		m_originalPos = transform.position;
		m_originalParent = transform.parent;

		// Move egg to the world (layer and parent)
		gameObject.SetLayerRecursively("Default");
		transform.SetParent(m_incubatorAnchor.transform, false);

		// Move to initial position
		FollowCursor();
	}

	/// <summary>
	/// A drag movement started on this object is moving.
	/// </summary>
	public void OnMouseDrag() {
		// Ignore if component is disabled
		if(!enabled) return;

		// Drag around the screen!
		FollowCursor();

		// Snap to anchor if possible
		if(CanSnap(true)) {
			transform.position = m_incubatorAnchor.transform.position;
		}
	}

	/// <summary>
	/// Input started on this object has been released.
	/// </summary>
	public void OnMouseUp() {
		// Ignore if component is disabled
		if(!enabled) return;

		// Hide warning message in any case
		m_warningMessage.Show(false);

		// Start incubating?
		bool incubating = false;
		if(CanSnap(false)) {
			// Dropped onto the incubator! Start incubating
			// Double check in case the manager doesn't allow us to do it
			incubating = EggManager.PutEggToIncubator(GetComponent<EggController>().eggData);

			// If successful, save persistence
			if(incubating) PersistenceManager.Save();
		}

		// Either snap to incubator anchor or go back to original position
		if(incubating) {
			m_incubatorAnchor.AttachEgg(GetComponent<EggController>());
		} else {
			// Move back to original position, parent and layer
			gameObject.SetLayerRecursively("3dOverUI");
			transform.SetParent(m_originalParent, false);
			transform.position = m_originalPos;
		}
	}
}

