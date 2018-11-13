// MapMarker.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Generic behaviour for all map markers - sprites in the game scene that should 
/// be rendered in the map.
/// </summary>
public class MapMarker : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Type {
		CHEST,
		EGG,
		LETTER,
		DECO,
		DRAGON
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Type m_type = Type.DECO;
	public Type type {
		get { return m_type; }
	}

	[Space]
	[SerializeField] private bool m_rotateWithObject = true;
	[SerializeField] private bool m_zoomCompensation = true;

	// Whether to show the marker or not (i.e. set to false when egg has been collected)
	private bool m_showMarker = true;
	public bool showMarker {
		get { return m_showMarker; }
		set {
			m_showMarker = value;
			UpdateMarker();
		}
	}

	// Store some original properties of the marker
	protected Vector3 m_originalScale = Vector3.one;
	protected float m_zoomScaleFactor = 1f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Initialize internal vars
		m_originalScale = GetMarkerTransform().localScale;

		// Subscribe to external events
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
		Broadcaster.AddListener(BroadcastEventType.PROFILE_MAP_UNLOCKED, this);
		Messenger.AddListener<float>(MessengerEvents.UI_MAP_ZOOM_CHANGED, OnMapZoomChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
		Broadcaster.RemoveListener(BroadcastEventType.PROFILE_MAP_UNLOCKED, this);
		Messenger.RemoveListener<float>(MessengerEvents.UI_MAP_ZOOM_CHANGED, OnMapZoomChanged);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.PROFILE_MAP_UNLOCKED:
            {
                OnMapUnlocked();
            }break;
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh marker to match the object's position, rotation, scaling, etc.
	/// </summary>
	protected virtual void UpdateMarker() {
		// Check visibility based on marker type and map upgrade level
		switch(m_type) {
			// Unlockable marker types
			case Type.CHEST:
			case Type.EGG:
			case Type.LETTER: {
				// [AOC] If the map timer runs out during the game, we let the player enjoy the unlocked map for the whole run
				//       That's why we check the GameSceneController rather than the user profile
				this.gameObject.SetActive(showMarker && InstanceManager.gameSceneControllerBase.mapUnlocked);
			} break;

			// Rest of marker types
			default: {
				this.gameObject.SetActive(showMarker);
			} break;
		}

		// Nothing else to do if not visible
		if(!this.gameObject.activeSelf) return;

		// Update marker's transformation
		UpdatePosition();
		UpdateScale();
		UpdateRotation();
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATES													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the root marker transform, in case it's not this object's transform.
	/// </summary>
	/// <returns>The marker's transform.</returns>
	protected virtual Transform GetMarkerTransform() {
		return this.transform;
	}

	/// <summary>
	/// Gets the transform used as reference for the marker.
	/// Typically the actual object they're representing on the map, to copy position, scale, rotation, etc.
	/// </summary>
	/// <returns>The marker's reference transform.</returns>
	protected virtual Transform GetReferenceTransform() {
		return GetMarkerTransform().parent;
	}

	/// <summary>
	/// Set the position of the marker relative to its reference transform.
	/// </summary>
	protected virtual void UpdatePosition() {
		Transform t = GetMarkerTransform();
		t.localPosition = Vector3.zero;
		t.position = t.position + new Vector3(0f, 0f, UIConstants.MAP_MARKERS_DEPTH);
	}

	/// <summary>
	/// Set the scale of the marker based on its reference transform.
	/// </summary>
	protected virtual void UpdateScale() {
		// Aux vars
		Transform markerTransform = GetMarkerTransform();
		Transform refTransform = GetReferenceTransform();

		// Compensate parent's scale factor (i.e. if parent is a dragon, which scales with level, or if parent has a non-linear scale)
		// Also apply minimap's zoom correction so markers keep a constant size
		Vector3 parentScale = refTransform.lossyScale;	// Global scale of the parent
		markerTransform.localScale = new Vector3(
			m_originalScale.x / parentScale.x / m_zoomScaleFactor, // If smaller zoom (closer), make marker bigger and vice-versa
			m_originalScale.y / parentScale.y / m_zoomScaleFactor, 
			1f	// We don't care about Z scaling
		);
	}

	/// <summary>
	/// Apply the XY plane rotation to the marker based on reference transform.
	/// Will also flip the target transform if looking to the left.
	/// </summary>
	protected virtual void UpdateRotation() {
		// Rotate?
		if(m_rotateWithObject) {
			// Yes!
			ApplyRotation(GetMarkerTransform(), GetReferenceTransform());
		} else {
			// Compensate parent's rotation
			GetMarkerTransform().rotation = Quaternion.identity;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the XY plane rotation from the reference transform to the target transform.
	/// Will also flip the target if looking to the left.
	/// </summary>
	/// <param name="_target">Target transform.</param>
	/// <param name="_reference">Reference transform.</param>
	protected void ApplyRotation(Transform _target, Transform _reference) {
		// Black maths magic from HSX
		// Find out parent's direction and nullify Z component
		Vector3 dir = _reference.forward;
		dir.z = 0.0f;

		// Flip based on direction
		Vector3 scale = _target.localScale;
		scale.x = dir.x >= 0? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
		_target.localScale = scale;

		// Make x absolute, since we're flipping the sprite
		Vector3 absDir = dir;
		absDir.x = Mathf.Abs(absDir.x);

		// Compute rotation angle
		float angle = Vector3.Angle(absDir, Vector3.right);
		if((dir.x >= 0 && dir.y < 0) || (dir.x < 0 && dir.y >= 0)) {
			angle = -angle;
		}

		// Apply rotation
		_target.LookAt(_target.position + Vector3.forward);
		_target.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A popup has been opened
	/// </summary>
	/// <param name="_popup">The popup that has just been opened.</param>
	private void OnPopupOpened(PopupController _popup) {
		// If it's the map popup, refresh marker
		if(_popup.GetComponent<PopupInGameMap>() != null) {
			UpdateMarker();
		}
	}

	/// <summary>
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that has just been closed.</param>
	private void OnPopupClosed(PopupController _popup) {
		// If it's the map popup, disable marker
		if(_popup.GetComponent<PopupInGameMap>() != null) {
			this.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Minimap has been upgraded.
	/// </summary>
	private void OnMapUnlocked() {
		// Update marker will do the job
		// Add some delay to give time for feedback to show off
		UbiBCN.CoroutineManager.DelayedCall(UpdateMarker, 0.25f, true);
	}

	/// <summary>
	/// The zoom has changed in the minimap.
	/// </summary>
	/// <param name="_zoomFactor">Percentage relative to initial zoom level (0.5x, 1x, 2x, etc, the smaller the closer.</param>
	private void OnMapZoomChanged(float _zoomFactor) {
		// Update zoom correction factor (if setup to do so)
		if(m_zoomCompensation) {
			m_zoomScaleFactor = _zoomFactor;
		} else {
			m_zoomScaleFactor = 1f;
		}

		// Refresh marker
		UpdateMarker();
	}
}