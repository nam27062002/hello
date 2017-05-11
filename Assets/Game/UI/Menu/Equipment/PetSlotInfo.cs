// PetNameTag.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single controller for the pet's name label in the pet selection screen.
/// Will be linked to one of the pet's 3D view position.
/// </summary>
public class PetSlotInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_emptySlotAnim = null;
	[SerializeField] private ShowHideAnimator m_equippedSlotAnim = null;
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Image m_rarityIcon = null;

	// Internal logic
	private int m_slotIdx = 0;

	// Internal refs
	private Transform m_attachPoint = null;
	private DragonData m_dragonData = null;

	private PetsSceneController m_petsScene = null;
	private PetsSceneController petsScene {
		get {
			if(m_petsScene == null) {
				m_petsScene = InstanceManager.menuSceneController.GetScreenScene(MenuScreens.PETS).GetComponent<PetsSceneController>();
			}
			return m_petsScene;
		}
	}

	private ShowHideAnimator m_anim = null;
	public ShowHideAnimator anim {
		get {
			if(m_anim == null) m_anim = GetComponent<ShowHideAnimator>();
			return m_anim;
		}
	}

	// Internal objects
	private Coroutine m_rarityGlowCoroutine = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		anim.OnHidePreAnimation.AddListener(OnHidePreAnimation);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Keep anchored
		if(isActiveAndEnabled && m_attachPoint != null) {
			// Get camera and apply the inverse transformation
			if(InstanceManager.sceneController.mainCamera != null) {
				// From http://answers.unity3d.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
				// We can do it that easily because we've adjusted the containers to match the camera viewport coords
				Vector2 posScreen = InstanceManager.sceneController.mainCamera.WorldToViewportPoint(m_attachPoint.position);
				RectTransform rt = this.transform as RectTransform;
				rt.anchoredPosition = Vector2.zero;
				rt.anchorMin = posScreen;
				rt.anchorMax = posScreen;
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the slot info with a target dragon preview and data.
	/// </summary>
	/// <param name="_slotIdx">The pet slot assigned to this info object.</param>
	public void Init(int _slotIdx) {
		// Store slot index
		m_slotIdx = _slotIdx;

		// Get corresponding anchor
		m_attachPoint = petsScene.petAnchors[_slotIdx];
	}

	/// <summary>
	/// Refresh the slot's info with a specific dragon data.
	/// </summary>
	/// <param name="_dragonData">The dragon data to be used to refresh this slot's info.</param>
	public void Refresh(DragonData _dragonData, bool _animate) {
		// Store dragon data
		m_dragonData = _dragonData;

		// Show?
		bool show = m_slotIdx < _dragonData.pets.Count;	// Depends on the amount of slots for this dragon
		this.gameObject.SetActive(show);

		// Refresh info
		if(show) {
			// Equipped or empty?
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_dragonData.pets[m_slotIdx]);
			bool equipped = (petDef != null);
			m_equippedSlotAnim.Set(equipped, _animate);
			m_emptySlotAnim.Set(!equipped, _animate);

			// Pet info
			if(equipped) {
				// Name
				m_nameText.Localize(petDef.Get("tidName"));

				// Rarity icon
				string raritySku = petDef.Get("rarity");
				EggReward.Rarity rarity = EggReward.SkuToRarity(raritySku);
				m_rarityIcon.sprite = UIConstants.RARITY_ICONS[(int)rarity];
				m_rarityIcon.gameObject.SetActive(m_rarityIcon.sprite != null);	// Hide if no icon

				// Rarity glow
				// Delay it to be sure that the DragonEquip component has had time to instantiate the pet
				// Stop any pending coroutine first!
				if(m_rarityGlowCoroutine != null) {
					StopCoroutine(m_rarityGlowCoroutine);
					m_rarityGlowCoroutine = null;
				}
				m_rarityGlowCoroutine = StartCoroutine(LoadRarityGlowDelayed(raritySku));
			}
		}
	}

	/// <summary>
	/// Coroutine to laod the pet's rarity glow after a delay.
	/// </summary>
	/// <returns>Coroutine.</returns>
	/// <param name="_raritySku">Rarity to be displayed.</param>
	private IEnumerator LoadRarityGlowDelayed(string _raritySku) {
		// Wait until the pet preview has been loaded
		yield return new WaitUntil(() => petsScene.petLoaders[m_slotIdx].petInstance != null);

		// Show glow
		MenuPetPreview petPreview = petsScene.petLoaders[m_slotIdx].petInstance.GetComponent<MenuPetPreview>();
		if(petPreview != null) {
			petPreview.ToggleRarityGlow(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The empty slot object has been tapped.
	/// </summary>
	public void OnEmptySlotTap() {
		// Show some feedback
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_EMPTY_SLOT_INFO"), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);
	}

	/// <summary>
	/// The equipped object has been tapped.
	/// </summary>
	public void OnEquippedSlotTap() {
		// Ignore if data not valid
		if(m_dragonData == null) return;

		// Select equipped pet (tell the pets screen controller to do so)
		MenuSceneController menuController = InstanceManager.menuSceneController;
		PetsScreenController petsScreen = menuController.GetScreen(MenuScreens.PETS).GetComponent<PetsScreenController>();
		petsScreen.ScrollToPet(m_dragonData.pets[m_slotIdx]);
	}

	/// <summary>
	/// Unequip button has been pressed.
	/// </summary>
	public void OnUnequipButton() {
		// Ignore if data not valid
		if(m_dragonData == null) return;

		// Unequip pet! Refresh will be triggered by the screen controller once the unequip is confirmed
		UsersManager.currentUser.UnequipPet(m_dragonData.def.sku, m_slotIdx);
	}

	/// <summary>
	/// The slot is about to be hidden.
	/// </summary>
	/// <param name="_anim">The animator that triggered the event.</param>
	private void OnHidePreAnimation(ShowHideAnimator _anim) {
		// Toggle pet's rarity glow
		if(m_attachPoint != null) {
			if(petsScene.petLoaders[m_slotIdx].petInstance != null) {
				MenuPetPreview petPreview = petsScene.petLoaders[m_slotIdx].petInstance.GetComponent<MenuPetPreview>();
				if(petPreview != null) {
					petPreview.ToggleRarityGlow(false);
				}
			}
		}
	}
}