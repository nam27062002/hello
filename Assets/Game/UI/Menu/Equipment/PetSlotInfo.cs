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
	[SerializeField] private GameObject m_emptySlotObj = null;
	[SerializeField] private GameObject m_equippedSlotObj = null;
	[Space]
	[SerializeField] private Localizer m_nameText = null;

	// Internal
	private int m_slotIdx = 0;
	private AttachPoint m_attachPoint = null;
	private DragonData m_dragonData = null;

	private ShowHideAnimator m_anim = null;
	public ShowHideAnimator anim {
		get {
			if(m_anim == null) m_anim = GetComponent<ShowHideAnimator>();
			return m_anim;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
				Vector2 posScreen = InstanceManager.sceneController.mainCamera.WorldToViewportPoint(m_attachPoint.transform.position);
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
	/// <param name="_dragonPreview">The 3D preview to link this info to.</param>
	public void Init(int _slotIdx, MenuDragonPreview _dragonPreview) {
		// Store slot index
		m_slotIdx = _slotIdx;

		// Get corresponding anchor
		Equipable.AttachPoint targetPoint = (Equipable.AttachPoint)((int)Equipable.AttachPoint.Pet_1 + m_slotIdx);
		m_attachPoint = _dragonPreview.GetComponent<DragonEquip>().GetAttachPoint(targetPoint);
	}

	/// <summary>
	/// Refresh the slot's info with a specific dragon data.
	/// </summary>
	/// <param name="_dragonData">The dragon data to be used to refresh this slot's info.</param>
	public void Refresh(DragonData _dragonData) {
		// Store dragon data
		m_dragonData = _dragonData;

		// Show?
		bool show = m_slotIdx < _dragonData.pets.Count;	// Depends on the amount of slots for this dragon
		this.gameObject.SetActive(show);

		// Refresh info
		if(show) {
			// Equipped or empty?
			// [AOC] TODO!! Make it nicer
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_dragonData.pets[m_slotIdx]);
			bool equipped = (petDef != null);
			m_equippedSlotObj.SetActive(equipped);
			m_emptySlotObj.SetActive(!equipped);

			// Pet info
			if(equipped) {
				// Name
				m_nameText.Localize(petDef.Get("tidName"));

				// Rarity
				m_nameText.text.color = UIConstants.GetRarityColor(petDef.Get("rarity"));
			}
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
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("Pick a pet from the list below!"), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);	// [AOC] HARDCODED!!
	}

	/// <summary>
	/// The equipped object has been tapped.
	/// </summary>
	public void OnEquippedSlotTap() {
		// Ignore if data not valid
		if(m_dragonData == null) return;

		// Select equipped pet (tell the pets screen controller to do so)
		MenuSceneController menuController = InstanceManager.GetSceneController<MenuSceneController>();
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
}