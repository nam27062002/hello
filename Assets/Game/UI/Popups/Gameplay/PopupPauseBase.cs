// PopupPauseBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for all in-game popups pausing the game.
/// </summary>
[RequireComponent(typeof(PopupController))]
public abstract class PopupPauseBase : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	protected PopupController m_popup = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Get popup reference
		m_popup = GetComponent<PopupController>();

		// Subscribe to popup events
		m_popup.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
		m_popup.OnClosePostAnimation.AddListener(OnClosePostAnimation);

		// This popup won't be destroyed during the whole game, but we want to destroy it upon game ending
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnd);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from popup events
		m_popup.OnOpenPreAnimation.RemoveListener(OnOpenPreAnimation);
		m_popup.OnClosePostAnimation.RemoveListener(OnClosePostAnimation);

		// Unsubscribe to external events
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnd);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public virtual void OnOpenPreAnimation() {
		// Pause the game
		if(InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.PauseGame(true);
		}
	}

	/// <summary>
	/// Close animation has finished.
	/// </summary>
	public virtual void OnClosePostAnimation() {
		// Resume game
		if(InstanceManager.gameSceneController != null) {
			InstanceManager.gameSceneController.PauseGame(false);
		}
	}

	/// <summary>
	/// The game has eneded.
	/// </summary>
	private void OnGameEnd() {
		// Destroy this popup
		GameObject.Destroy(this.gameObject);
	}
}