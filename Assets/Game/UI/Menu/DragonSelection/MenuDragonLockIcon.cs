// MenuDragonLockIcon.cs
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
		// Show feedback message
		IDragonData selectedDragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		string textToDisplay = string.Empty;
		switch(selectedDragonData.GetLockState()) {
			case IDragonData.LockState.SHADOW: {
				textToDisplay = LocalizationManager.SharedInstance.Localize("TID_SELECT_DRAGON_UNKNOWN_MESSAGE");
			} break;

			case IDragonData.LockState.LOCKED: {
				DefinitionNode previousDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, selectedDragonData.def.GetAsString("previousDragonSku"));
				if(previousDragonDef != null) {
					textToDisplay = LocalizationManager.SharedInstance.Localize("TID_SELECT_DRAGON_KNOWN_MESSAGE", previousDragonDef.GetLocalized("tidName"));
				}
			} break;
		}

		if(!string.IsNullOrEmpty(textToDisplay)) {
			UIFeedbackText txt = UIFeedbackText.CreateAndLaunch(textToDisplay, new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			txt.transform.SetLocalPosZ(txt.transform.localPosition.z - 150f);	// Bring it forward so it doesn't conflict with the 3D lock
			txt.duration = 3f;	// Text is quite long, make it last a bit longer
		}

		// Trigger bounce animation
		view.LaunchBounceAnim();

		// Propagate event to parent hierarchy (we don't want to capture the event)
		// From https://coeurdecode.com/2015/10/20/bubbling-events-in-unity/ <3
		// Dirty hack to simulate event propagation. The downside is that the lock icon must then be a children of the dragon scroller.
		ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _event, ExecuteEvents.pointerClickHandler);
	}
}