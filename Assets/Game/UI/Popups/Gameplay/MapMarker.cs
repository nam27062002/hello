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
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Generic behaviour for all map markers - sprites in the game scene that should 
/// be rendered in the map.
/// </summary>
public class MapMarker : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Type {
		CHEST,
		EGG,
		LETTER,
		DECO
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Type m_type = Type.DECO;
	[Space]
	[SerializeField] private bool m_rotateWithObject = true;

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
	private Vector3 m_originalScale = Vector3.one;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Initialize internal vars
		m_originalScale = transform.localScale;

		// Subscribe to external events
		Messenger.AddListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.AddListener(GameEvents.PROFILE_MAP_UNLOCKED, OnMapUnlocked);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<PopupController>(EngineEvents.POPUP_OPENED, OnPopupOpened);
		Messenger.RemoveListener(GameEvents.PROFILE_MAP_UNLOCKED, OnMapUnlocked);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh marker to match the object's position, rotation, scaling, etc.
	/// </summary>
	private void UpdateMarker() {
		// Check visibility based on marker type and level
		switch(m_type) {
			case Type.DECO: {
				this.gameObject.SetActive(showMarker);
			} break;

			case Type.CHEST:
			case Type.EGG:
			case Type.LETTER: {
				// [AOC] If the map timer runs out during the game, we let the player enjoy the unlocked map for the whole run
				//       That's why we check the GameSceneController rather than the user profile
				this.gameObject.SetActive(showMarker && InstanceManager.gameSceneControllerBase.mapUnlocked);
			} break;
		}

		// Nothing else to do if not visible
		if(!this.gameObject.activeSelf) return;

		// Reset position
		transform.localPosition = Vector3.zero;

		// Compensate parent's scale factor (i.e. if parent is a dragon, which scales with level, or if parent has a non-linear scale)
		//Vector3 parentScale = parentTransform.lossyScale;
		//transform.localScale = new Vector3(m_originalScale.x / parentScale.x, m_originalScale.y / parentScale.y, m_originalScale.z / parentScale.z);
		transform.localScale = new Vector3(m_originalScale.x, m_originalScale.y, 1f);	// [AOC] Don't like it, plust Z scale should always be 1

		// Apply parent's rotation - only in the XY plane
		if(m_rotateWithObject) {
			// Black maths magic from HSX
			// Find out parent's direction and nullify Z component
			Vector3 dir = transform.parent.forward;
			dir.z = 0.0f;

			// Flip based on direction
			Vector3 scale = transform.localScale;
			scale.x = dir.x >= 0? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
			transform.localScale = scale;

			// Make x absolute, since we're flipping the sprite
			Vector3 absDir = dir;
			absDir.x = Mathf.Abs(absDir.x);

			// Compute rotation angle
			float angle = Vector3.Angle(absDir, Vector3.right);
			if((dir.x >= 0 && dir.y < 0) || (dir.x < 0 && dir.y >= 0)) {
				angle = -angle;
			}

			// Apply rotation
			transform.LookAt(transform.position + Vector3.forward);
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		} else {
			// Compensate parent's rotation
			transform.rotation = Quaternion.identity;
		}
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
	/// Minimap has been upgraded.
	/// </summary>
	private void OnMapUnlocked() {
		// Update marker will do the job
		// Add some delay to give time for feedback to show off
		DOVirtual.DelayedCall(0.25f, UpdateMarker, true);
	}
}