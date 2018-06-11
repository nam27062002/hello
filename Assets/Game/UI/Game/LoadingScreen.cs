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
	[SerializeField] private Canvas m_loadingCanvas = null;
	[Space]
	[SerializeField] private Image m_dragonIcon = null;
	[SerializeField] private PowerIcon[] m_powerIcons = null;

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//

	public void Awake(){
		m_animator.OnHidePostAnimation.AddListener(OnHidePostAnimation);
	}

	public void OnHidePostAnimation(ShowHideAnimator _animator) {
		m_loadingCanvas.gameObject.SetActive(false);
		m_loadingCanvas.worldCamera.gameObject.SetActive(false);
	}

	/// <summary>
	/// Toggle the loading screen on/off.
	/// </summary>
	/// <param name="_show">Whether to show or hide the screen.</param>
	/// <param name="_animate">Use fade animation?</param>
	public static void Toggle(bool _show, bool _animate = true) {
		if ( _show ){
			instance.m_loadingCanvas.gameObject.SetActive(true);
			instance.m_loadingCanvas.worldCamera.gameObject.SetActive(true);
		}
		// Just let the animator do it
		instance.m_animator.Set(_show, _animate);
	}

	/// <summary>
	/// Initialize the screen with current data: selected dragon, skin, pets, etc.
	/// </summary>
	public static void InitWithCurrentData() {
		// Aux vars
		DragonData currentDragon = null;
		DefinitionNode skinDef = null;
		List<string> pets = null;

		if (SceneController.s_mode == SceneController.Mode.TOURNAMENT) {
			HDTournamentManager tournament = HDLiveEventsManager.instance.m_tournament;
			currentDragon = DragonManager.GetDragonData(tournament.GetToUseDragon());
			skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, tournament.GetToUseSkin());
			pets = tournament.GetToUsePets();
		} else {
			currentDragon = DragonManager.GetDragonData(UsersManager.currentUser.currentDragon);
			skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, currentDragon.diguise);
			pets = currentDragon.pets;
		}


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
		for(int i = 0; i < pets.Count; i++) {
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, pets[i]);
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

			PowerIcon.Mode mode = PowerIcon.Mode.SKIN;
			if (i > 0) mode = PowerIcon.Mode.PET;

			powerIcon.InitFromDefinition(powerDefs[i], false, false, mode);
		}
	}
}