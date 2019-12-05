// DragonUnlockScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the photo menu screen.
/// </summary>
public class DragonUnlockScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_dragonName = null;
	[SerializeField] private Localizer m_dragonDesc = null;
	[Space]
	[SerializeField] private Image m_dragonTierIcon = null;
	[SerializeField] private ShowHideAnimator m_newPreysAnimator = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[SerializeField] private TextMeshProUGUI m_speedText = null;
	[Space]
	[SerializeField] private DragControlRotation m_dragController = null;
	[SerializeField] private ShareButton m_shareButton = null;

	// Internal

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show/Hide menu HUD.
	/// </summary>
	/// <param name="_show">Show or hide?</param>
	public void ToggleHUD(bool _show) {
		InstanceManager.menuSceneController.hud.animator.Set(_show);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Screen is about to be open.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;
		IDragonData dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Initialize dragon info
		if(m_dragonName != null) {
			m_dragonName.Localize("TID_DRAGON_UNLOCK", dragonData.def.GetLocalized("tidName"));
		}
		if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
		if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));
		if(m_healthText != null) m_healthText.text = StringUtils.FormatNumber(dragonData.maxHealth, 0);
		if(m_energyText != null) m_energyText.text = StringUtils.FormatNumber(dragonData.baseEnergy, 0);
		if(m_speedText != null) m_speedText.text = StringUtils.FormatNumber(dragonData.maxSpeed * 10f, 0);	// x10 to show nicer numbers

		// Disable drag controller
		if(m_dragController != null) m_dragController.gameObject.SetActive(false);

		// If the unlocked dragon is of different tier as the dragon used to unlocked it, show 'new preys' banner
		if(m_newPreysAnimator != null) {
			DefinitionNode previousDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, dragonData.def.GetAsString("previousDragonSku"));
			if(previousDragonDef != null && previousDragonDef.Get("tier") != dragonData.tierDef.sku) {
				// Show!
				m_newPreysAnimator.RestartShow();	// Should have the proper delay
			} else {
				// Hide! (no animation)
				m_newPreysAnimator.ForceHide(false);
			}
		}

		// Only show photo button if the dragon is owned
		m_shareButton.gameObject.SetActive(dragonData.isOwned && ShareButton.CanBeDisplayed());

		// Hide HUD
		ToggleHUD(false);
	}

	/// <summary>
	/// The screen has just finished the open animation.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPostAnimation(ShowHideAnimator _animator) {
		// Initialize drag controller with current dragon preview
		if(m_dragController != null) {
			MenuSceneController menuController = InstanceManager.menuSceneController;
			MenuDragonPreview dragonPreview = menuController.dragonScroller.GetDragonPreview(menuController.selectedDragon);
			m_dragController.gameObject.SetActive(true);
			m_dragController.target = dragonPreview.transform;
		}
	}

	/// <summary>
	/// Screen is about to close.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Disable drag controller
		m_dragController.gameObject.SetActive(false);

		// Trigger manually managed animators
		if(m_newPreysAnimator != null) {
			m_newPreysAnimator.Hide();
		}
	}

	/// <summary>
	/// The share button has been pressed.
	/// </summary>
	public void OnShareButton() {
		// [AOC] New System
		/*
		// Get the share screen instance and initialize it with current data
		IDragonData dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);
		ShareScreenDragon shareScreen = ShareScreensManager.GetShareScreen("dragon_acquired") as ShareScreenDragon;
		shareScreen.Init(
			"dragon_acquired",
			InstanceManager.menuSceneController.mainCamera,
			dragonData,
			false,
			null
		);
		shareScreen.TakePicture(IShareScreen.CaptureMode.RENDER_TEXTURE);
		*/

		// [AOC] Not being able to position the dragon feels weird, keep the old flow
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.PHOTO);
	}
}