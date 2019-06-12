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
	[SerializeField] private UISpriteAddressablesLoader m_dragonIconLoader = null;
    [SerializeField] private Image m_tierIcon = null;
    [Space]
    [SerializeField] private PowerIcon[] m_powerIcons = null;

	public static bool isVisible {
		get { 
			if(instance == null) return false;
			if(instance.m_animator != null) {
				return instance.m_animator.visible; 
			}
			return instance.gameObject.activeInHierarchy;
		}
	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//

	public void Awake(){
		m_animator.OnHidePostAnimation.AddListener(OnHidePostAnimation);
	//	m_animator.OnShowPostAnimation.AddListener();
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
			InitWithCurrentData();
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
		IDragonData currentDragon = null;
		if (SceneController.mode == SceneController.Mode.TOURNAMENT) {
			currentDragon = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
		} else {
			currentDragon = DragonManager.currentDragon;
		}

		DefinitionNode skinDef = skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, currentDragon.disguise);
		List<string> pets = currentDragon.pets;

        // Dragon image
        instance.m_dragonIconLoader.LoadAsync(skinDef.Get("icon"));
        instance.m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, currentDragon.tierDef.Get("icon"));

		// Powers: skin + pets
		// [AOC] PowerIcon does all the job for us!
		PowerIcon.InitPowerIconsWithDragonData(ref instance.m_powerIcons, currentDragon);
	}
}