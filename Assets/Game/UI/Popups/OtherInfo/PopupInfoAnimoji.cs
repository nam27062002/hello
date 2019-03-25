// PopupInfoAnimoji.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Animoji info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoAnimoji : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/OtherInfo/PF_PopupInfoAnimoji";
	public const string ANIMOJI_TUTORIAL_KEY = "ANIMOJI_TUTORIAL";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed

	// Internal
	private bool m_deepLinkTriggered = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Reset local vars
		m_deepLinkTriggered = false;

		// Hide all menu UI
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(false);
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
		// Restore menu UI
		InstanceManager.menuSceneController.GetUICanvasGO().SetActive(true);

		// Mark tutorial as completed
		Prefs.SetBoolPlayer(ANIMOJI_TUTORIAL_KEY, true);
	}

	/// <summary>
	/// "Show Me" button has been pressed.
	/// </summary>
	public void OnShowMeButton() {
		// Prevent spamming
		if(m_deepLinkTriggered) return;
		m_deepLinkTriggered = true;

		// Close ourselves
		GetComponent<PopupController>().Close(true);

		// If current dragon doesn't support animojis, select the biggest owned dragon supporting them
		if(!AnimojiScreenController.IsDragonSupported(InstanceManager.menuSceneController.selectedDragon)) {
			// Get owned dragons and sort them by order (reversed, bigger to smaller)
			List<IDragonData> ownedDragons = DragonManager.GetDragonsByLockState(IDragonData.LockState.OWNED);
			ownedDragons.Sort(
				(IDragonData _d1, IDragonData _d2) => {
					return _d1.GetOrder().CompareTo(_d2.GetOrder()) * -1;	// [AOC] Reverse them!
				}
			);

			// Pick the biggest one!
			for(int i = 0; i < ownedDragons.Count; ++i) {
				// Does this dragon support Animojis?
				if(AnimojiScreenController.IsDragonSupported(ownedDragons[i].def.sku)) {
					// Yes! Make it the current one and break the loop!
					InstanceManager.menuSceneController.SetSelectedDragon(ownedDragons[i].def.sku);
					break;
				}
			}
		}

		// Clear popup's queue before changing screen
		PopupManager.ClearQueue();

		// Launch the animoji flow
		// Give some time for the dragon selection and popup close animations to finish
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.ANIMOJI);
			}, 0.5f
		);
	}
}