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
		IDragonData currentDragon = null;
		if (SceneController.mode == SceneController.Mode.TOURNAMENT) {
			currentDragon = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
		} else {
			currentDragon = DragonManager.currentDragon;
		}

		DefinitionNode skinDef = skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, currentDragon.disguise);
		List<string> pets = currentDragon.pets;

		// Dragon image
		instance.m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + currentDragon.def.sku + "/" + skinDef.Get("icon"));
        instance.m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, currentDragon.tierDef.Get("icon"));


		// Powers: skin + pets
		List<DefinitionNode> powerDefs = new List<DefinitionNode>();
        List<PowerIcon.Mode> powerMode = new List<PowerIcon.Mode>();
		// Skin
		if(skinDef == null) {
			powerDefs.Add(null);
		} else {
			powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup")));	// Can be null
		}
        powerMode.Add(PowerIcon.Mode.SKIN);

        // Special Dragon Powers
        if (SceneController.mode == SceneController.Mode.SPECIAL_DRAGONS) {
            DragonDataSpecial dataSpecial = (DragonDataSpecial)currentDragon;
            for (int i = 1; i <= dataSpecial.powerLevel; ++i) {
                powerDefs.Add(dataSpecial.specialPowerDefsByOrder[i - 1]);
                powerMode.Add(PowerIcon.Mode.SPECIAL_DRAGON);
            }
        }

		// Pets
		for(int i = 0; i < pets.Count; i++) {
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, pets[i]);
			if(petDef == null) {
				powerDefs.Add(null);
			} else {
				powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup")));
			}
            powerMode.Add(PowerIcon.Mode.PET);
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
                if (i > 0 || SceneController.mode == SceneController.Mode.SPECIAL_DRAGONS) {
					powerIcon.gameObject.SetActive(false);
				} else {
					powerIcon.InitFromDefinition(null, false, false);
				}
				continue;
			}

			// Everything ok! Initialize
			powerIcon.gameObject.SetActive(true);

            powerIcon.InitFromDefinition(powerDefs[i], false, false, powerMode[i]);
		}
	}
}