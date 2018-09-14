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
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChange);
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChange);
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

		// Don't show in some screens (mainly reward screens which will show the photo button for pets)
		switch(InstanceManager.menuSceneController.currentScreen) {
			case MenuScreen.OPEN_EGG:
			case MenuScreen.EVENT_REWARD:
			case MenuScreen.PENDING_REWARD:
			case MenuScreen.TOURNAMENT_REWARD: {
				animojiSupported = false;
			} break;
		}

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

	/// <summary>
	/// Screen change started.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Destination screen.</param>
	private void OnScreenChange(MenuScreen _from, MenuScreen _to) {
		Refresh();
	}
}