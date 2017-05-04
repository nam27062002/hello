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
	[Space]
	[SerializeField] private DragControlRotation m_dragController = null;

	// Internal

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

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
		
	}

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
		DragonData dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Initialize dragon info
		//if(m_dragonName != null) m_dragonName.Localize(dragonData.def.GetAsString("tidName"));
		if(m_dragonName != null) {
			m_dragonName.Localize("TID_RESULTS_DRAGON_UNLOCKED", dragonData.def.GetLocalized("tidName"));
		}
		if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
		if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));
		if(m_healthText != null) m_healthText.text = StringUtils.FormatNumber(dragonData.maxHealth, 0);
		if(m_energyText != null) m_energyText.text = StringUtils.FormatNumber(dragonData.baseEnergy, 0);

		// Initialize drag controller with current dragon preview
		MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.DRAGON_UNLOCK);
		MenuDragonPreview dragonPreview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(menuController.selectedDragon);
		if(m_dragController != null) m_dragController.target = dragonPreview.transform;

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

		// Hide HUD
		ToggleHUD(false);
	}

	/// <summary>
	/// Screen is about to close.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Trigger manually managed animators
		if(m_newPreysAnimator != null) {
			m_newPreysAnimator.Hide();
		}
	}
}