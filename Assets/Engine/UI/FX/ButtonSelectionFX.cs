// ButtonSelectionFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/07/2016.
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
/// Auxiliar component to show some visuals when button is not interactable.
/// Usually you want to use it with buttons not doing any visual effect on their "disabled" state.
/// Useful for tabs/button groups, since Unity doesn't have a "selected" state for the buttons.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSelectionFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_fxObj = null;

	// Internal references
	private Button m_button = null;

	// Internal logic
	private bool m_interactable = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get the button's reference
		m_button = GetComponent<Button>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize FX
		OnInteractableChanged(m_button.interactable);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate() {
		// [AOC] Couldn't figure out a better way to do it than checking frame by frame :(
		if(m_button.interactable != m_interactable) {
			OnInteractableChanged(m_button.interactable);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	private void OnInteractableChanged(bool _interactable) {
		// Just enable/disable object for now
		if(m_fxObj != null) m_fxObj.SetActive(!_interactable);		// [AOC] Selected FX should be displayed when button is disabled

		// Store new state
		m_interactable = _interactable;
	}
}