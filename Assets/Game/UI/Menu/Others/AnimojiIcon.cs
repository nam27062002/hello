// AnimojiIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller to display an icon only when animoji is supported.
/// </summary>
public class AnimojiIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private bool m_checkSelectedDragon = true;
	[SerializeField] private GameObject m_target = null;

	// Internal refs
	private ShowHideAnimator m_animator = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// If the target has an animator, get its reference
		m_animator = m_target.GetComponent<ShowHideAnimator>();

		// If this device doesn't support animojis at all, destroy target
		if(!AnimojiScreenController.IsDeviceSupported()) {
			GameObject.Destroy(m_target);
			m_target = null;
		}
		Refresh();

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show animoji notification?
	/// </summary>
	private void Refresh() {
		// Animoji must be available for current device and selected dragon
		// AnimojiScreen does all the work for us :)
		bool animojiSupported = AnimojiScreenController.IsSupported(InstanceManager.menuSceneController.selectedDragon);

		// Use animation if possible
		if(m_animator != null) {
			m_animator.Set(animojiSupported);
		} else {
			m_target.SetActive(animojiSupported);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed.
	/// </summary>
	/// <param name="_dragonSku">Sku of the newly selected dragon.</param>
	private void OnDragonSelected(string _dragonSku) {
		Refresh();
	}
}