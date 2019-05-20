// PopupMap.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using InControl;


//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// In-game map popup.
/// </summary>
public class PopupInGameMap : PopupPauseBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupInGameMap";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private MapScroller m_mapScroller = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

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
        if (m_popup.isReady) {
            InputDevice device = InputManager.ActiveDevice;
            if (device != null && device.Action4.WasReleased) {
                m_popup.Close(false);
            }
        }
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		base.OnDestroy();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show the map. To be connected in the open animation.
	/// </summary>
	public void ShowMapAnimationEvent() {
		m_mapScroller.OnOpenPostAnimation();
	}

	/// <summary>
	/// Hide the map. To be connected in the close animation.
	/// </summary>
	public void HideMapAnimationEvent() {
		m_mapScroller.OnClosePreAnimation();
	}
}