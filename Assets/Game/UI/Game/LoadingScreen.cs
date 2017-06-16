// LevelLoadingSplash.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Splash screen to show while the level is loading.
/// </summary>
public class LoadingScreen : UbiBCN.SingletonMonoBehaviour<LoadingScreen> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_animator = null;
	[Space]
	[SerializeField] private Image m_dragonIcon = null;
	[SerializeField] private PowerIcon[] m_powerIcons = null;

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Toggle the loading screen on/off.
	/// </summary>
	/// <param name="_show">Whether to show or hide the screen.</param>
	/// <param name="_animate">Use fade animation?</param>
	public static void Toggle(bool _show, bool _animate = true) {
		// Just let the animator do it
		instance.m_animator.Set(_show, _animate);
	}

	/// <summary>
	/// Initialize the screen with current data: selected dragon, skin, pets, etc.
	/// </summary>
	public static void InitWithCurrentData() {
		// Aux vars
		DragonData currentDragon = DragonManager.GetDragonData(UsersManager.currentUser.currentDragon);
		DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, currentDragon.diguise);

		// Dragon image
		instance.m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + currentDragon.def.sku + "/" + skinDef.Get("icon"));

		// Powers: skin + pets
		List<DefinitionNode> powerDefs = new List<DefinitionNode>();

		// Skin
		if(skinDef == null) {
			powerDefs.Add(null);
		} else {
			powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup")));	// Can be null
		}

		// Pets
		for(int i = 0; i < currentDragon.pets.Count; i++) {
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, currentDragon.pets[i]);
			if(petDef == null) {
				powerDefs.Add(null);
			} else {
				powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup")));
			}
		}

		// Initialize power icons
		for(int i = 0; i < instance.m_powerIcons.Length; i++) {
			// Get icon ref
			PowerIcon powerIcon = instance.m_powerIcons[i];

			// Hide if there are not enough powers defined
			if(i >= powerDefs.Count) {
				powerIcon.gameObject.SetActive(false);
				continue;
			}

			// Hide if there is no power associated
			if(powerDefs[i] == null) {
				powerIcon.gameObject.SetActive(false);
				continue;
			}

			// Everything ok! Initialize
			powerIcon.gameObject.SetActive(true);
			powerIcon.InitFromDefinition(powerDefs[i], false, false);
		}
	}
}