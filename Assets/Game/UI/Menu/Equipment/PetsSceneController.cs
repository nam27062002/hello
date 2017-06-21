// PetsSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the pets scene
/// </summary>
public class PetsSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Transform[] m_petAnchors = null;
	public Transform[] petAnchors { 
		get { return m_petAnchors; }
	}

	private MenuPetLoader[] m_petLoaders = null;
	public MenuPetLoader[] petLoaders { 
		get { 
			if(m_petLoaders == null) {
				m_petLoaders = new MenuPetLoader[m_petAnchors.Length];
				for(int i = 0; i < m_petAnchors.Length; i++) {
					m_petLoaders[i] = m_petAnchors[i].GetComponentInChildren<MenuPetLoader>();
				}
			}
			return m_petLoaders; 
		}
	}

	[Space]
	[SerializeField] private float m_anchorSpacing = 2f;	// World units
//	[SerializeField] private Range m_petAnimDuration = new Range(0.15f, 0.35f);	// Seconds
//	[SerializeField] private Range m_petAnimDistance = new Range(5f, 15f);	// World Units
	[SerializeField] private Vector3 m_petAnimFrom = new Vector3(0f, -4f, 0f);	// World Units
	[SerializeField] private float m_petAnimDuration = 0.5f;	// Seconds
	[SerializeField] private float m_petAnimDelay = 0.1f;	// Seconds

	// Internal
	private DragonData m_dragonData = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Messenger.AddListener<string, int , string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
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
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Messenger.RemoveListener<string, int , string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// We're entering the scene.
	/// </summary>
	/// <param name="_targetDragon">Dragon data to be used to initialize the scene.</param>
	public void OnShowPreAnimation() {
		// Store target dragon
		m_dragonData = DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon);	// Selected dragon

		// Adjust anchor positions based on dragon's slots amount so they look centered to 0
		int numSlots = m_dragonData.pets.Count;
		float startX = (float)(numSlots - 1)/2f * -m_anchorSpacing;	// [AOC] Black Magic! ^_^
		for(int i = 0; i < m_petAnchors.Length; i++) {
			// Valid slot?
			if(i < numSlots) {
				m_petAnchors[i].gameObject.SetActive(true);
				m_petAnchors[i].SetLocalPosX(startX + m_anchorSpacing * i);
			} else {
				m_petAnchors[i].gameObject.SetActive(false);
			}
		}

		// Load pet previews and launch intro animation
		for(int i = 0; i < numSlots; i++) {
			// Load
			LoadPetPreview(m_dragonData.pets[i], i);

			// Animate! Make them "come from afar"
			//Vector3 animFrom = Random.onUnitSphere * m_petAnimDistance.GetRandom();
			//float animDuration = m_petAnimDuration.GetRandom();
			Vector3 animFrom = m_petAnimFrom;
			float animDuration = m_petAnimDuration;

			MenuPetLoader petLoader = petLoaders[i];
			if(petLoader.petInstance != null) {
				petLoader.petInstance.transform.DOLocalMove(animFrom, animDuration)
					.From()
					.SetEase(Ease.OutCubic)
					.SetDelay(i * m_petAnimDelay);
			}
		}
	}

	/// <summary>
	/// We're exiting the scene.
	/// </summary>
	public void OnHidePreAnimation() {
		// Make pets fly away!
		for(int i = 0; i < petLoaders.Length; i++) {
			// Only if we actually have a pet in that slot
			MenuPetLoader petLoader = petLoaders[i];
			if(petLoader.petInstance != null) {
				// Make them "go away"!
				//Vector3 animFrom = Random.onUnitSphere * m_petAnimDistance.GetRandom();
				//float animDuration = m_petAnimDuration.GetRandom();
				Vector3 animFrom = m_petAnimFrom;
				float animDuration = m_petAnimDuration;

				petLoader.petInstance.transform.DOLocalMove(
					animFrom,
					animDuration
				)
					.SetDelay(i * m_petAnimDelay)
					.SetEase(Ease.InCubic)
					.OnComplete(() => {
						petLoader.Unload();
					});
			}
		}

		// Clear data
		m_dragonData = null;
	}

	/// <summary>
	/// Load the pet preview at the given slot.
	/// </summary>
	/// <param name="_petSku">Pet sku.</param>
	/// <param name="_slotIdx">Slot index.</param>
	private void LoadPetPreview(string _petSku, int _slotIdx) {
		// Equip or unequip?
		if(string.IsNullOrEmpty(_petSku)) {
			if(petLoaders[_slotIdx].petInstance != null) {
				// Toggle the OUT anim
				MenuPetPreview pet = petLoaders[_slotIdx].petInstance.GetComponent<MenuPetPreview>();
				pet.SetAnim(MenuPetPreview.Anim.OUT);

				// Program a delayed destruction of the pet preview (to give some time to see the anim)
				UbiBCN.CoroutineManager.DelayedCall(() => petLoaders[_slotIdx].Unload(), 0.3f, true);	// [AOC] MAGIC NUMBERS!! More or less synced with the animation
			}
		} else {
			// The loader will do everything!
			petLoaders[_slotIdx].Load(_petSku);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pets loadout has changed in the menu.
	/// </summary>
	/// <param name="_dragonSku">The dragon whose assigned pets have changed.</param>
	/// <param name="_slotIdx">Slot that has been changed.</param>
	/// <param name="_newPetSku">New pet assigned to the slot. Empty string for unequip.</param>
	public void OnPetChanged(string _dragonSku, int _slotIdx, string _newPetSku) {
		// Ignore if screen not active
		if(m_dragonData == null) return;

		// Is it meant for this dragon?
		if(_dragonSku == m_dragonData.def.sku) {
			// Reload pet at target slot
			LoadPetPreview(_newPetSku, _slotIdx);
		}
	}
}