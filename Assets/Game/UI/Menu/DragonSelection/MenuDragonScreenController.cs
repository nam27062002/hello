// MenuDragonSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/05/2016.
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
/// Main controller of the dragon selection screen.
/// </summary>
public class MenuDragonScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Shortcuts to important elements
	// Dragon selector
	private MenuDragonSelector m_dragonSelector = null;
	public MenuDragonSelector dragonSelector {
		get {
			if(m_dragonSelector == null) {
				m_dragonSelector = this.FindComponentRecursive<MenuDragonSelector>();
			}
			return m_dragonSelector;
		}
	}

	// Dragon scroller
	private MenuDragonScroller3D m_dragonScroller3D = null;
	public MenuDragonScroller3D dragonScroller3D {
		get {
			if(m_dragonScroller3D == null) {
				m_dragonScroller3D = InstanceManager.GetSceneController<MenuSceneController>().GetScreenScene(MenuScreens.DRAGON_SELECTION).FindComponentRecursive<MenuDragonScroller3D>();
			}
			return m_dragonScroller3D;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events.
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events.
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEvent _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.GetSceneController<MenuSceneController>().screensController) return;

		// If leaving this screen, remove "new" flag from eggs
		if(_event.fromScreenIdx == (int)MenuScreens.DRAGON_SELECTION) {
			for(int i = 0; i < EggManager.INVENTORY_SIZE; i++) {
				if(EggManager.inventory[i] != null) {
					EggManager.inventory[i].isNew = false;
				}
			}
		}
	}
}