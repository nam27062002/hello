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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}