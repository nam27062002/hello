﻿// MenuDragonLockIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for the lock icon in the dragon selection screen.
/// </summary>
public class MenuDragonLockIcon : MonoBehaviour, IPointerClickHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private LockViewController m_view = null;
	public LockViewController view { 
		get {
			if(m_view == null) {
				m_view = GetComponentInChildren<LockViewController>();
			}
			return m_view; 
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Start() {
		// Init view
		m_view = GetComponentInChildren<LockViewController>();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The input has detected a click over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnPointerClick(PointerEventData _event) {
		string sku = InstanceManager.menuSceneController.selectedDragon;
		DragonData data = DragonManager.GetDragonData(sku);

		if (data.GetLockState() == DragonData.LockState.SHADOW) {
			DragonData needDragonData = DragonManager.GetDragonData(data.revealFromDragons[0]);
			string[] replacements = new string[1];
			replacements[0] = LocalizationManager.SharedInstance.Localize(needDragonData.def.GetAsString("tidName"));
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_SELECT_DRAGON_UNKNOWN_MESSAGE", replacements), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		} 

		// Trigger bounce animation
		view.LaunchBounceAnim();

		// Propagate event to parent hierarchy (we don't want to capture the event)
		// From https://coeurdecode.com/2015/10/20/bubbling-events-in-unity/ <3
		// Dirty hack to simulate event propagation. The downside is that the lock icon must then be a children of the dragon scroller.
		ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _event, ExecuteEvents.pointerClickHandler);
	}
}