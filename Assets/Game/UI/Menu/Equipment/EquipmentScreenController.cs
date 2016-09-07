// EquipmentScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
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
/// 
/// </summary>
public class EquipmentScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Subscreen {
		DISGUISES,
		PETS
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Button m_disguisesButton = null;
	[SerializeField] private DisguisesScreenController m_disguisesScreen = null;

	[Space]
	[SerializeField] private Button m_petsButton = null;
	[SerializeField] private PetsScreenController m_petsScreen = null;

	// Internal
	private Subscreen m_lastActiveScreen = Subscreen.DISGUISES;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		ShowHideAnimator animator = GetComponent<ShowHideAnimator>();
		if(animator != null) {
			animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
			animator.OnHidePreAnimation.AddListener(OnHidePreAnimation);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize with either of the screens!
		switch(m_lastActiveScreen) {
			case Subscreen.DISGUISES:	ShowDisguises();	break;
			case Subscreen.PETS: 		ShowPets();			break;
		}
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
	private void OnDestroy() {
		// Unregister from external events
		ShowHideAnimator animator = GetComponent<ShowHideAnimator>();
		if(animator != null) {
			animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
			animator.OnHidePreAnimation.RemoveListener(OnHidePreAnimation);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the disguises screen and hides the pets.
	/// </summary>
	public void ShowDisguises() {
		// If we're in another screen, just change the lastActiveScreen so the disguises screen is displayed by default when opening the equipment screen
		if(isActiveAndEnabled) {
			m_disguisesScreen.Show();
			m_disguisesButton.interactable = false;
			m_petsScreen.Hide();
			m_petsButton.interactable = true;
		}
		m_lastActiveScreen = Subscreen.DISGUISES;
	}

	/// <summary>
	/// Shows the pets screen and hides the disguises.
	/// </summary>
	public void ShowPets() {
		// If we're in another screen, just change the lastActiveScreen so the pets screen is displayed by default when opening the equipment screen
		if(isActiveAndEnabled) {
			m_disguisesScreen.Hide();
			m_disguisesButton.interactable = true;
			m_petsScreen.Show();
			m_petsButton.interactable = false;
		}
		m_lastActiveScreen = Subscreen.PETS;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The screen is about to be hidden.
	/// </summary>
	/// <param name="_animator">The animator that is about to be hidden.</param>
	private void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Hdie both sub-screens
		m_disguisesScreen.Hide();
		m_petsScreen.Hide();
	}

	/// <summary>
	/// The screen is about to be displayed.
	/// </summary>
	/// <param name="_animator">The animator that is about to be shown.</param>
	private void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Restore last active screen and hide the rest
		switch(m_lastActiveScreen) {
			case Subscreen.DISGUISES:	ShowDisguises();	break;
			case Subscreen.PETS: 		ShowPets();			break;
		}
	}
}