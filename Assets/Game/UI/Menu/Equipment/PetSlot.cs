// PetSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Group a pet slot info with a power button in the pets screen.
/// </summary>
public class PetSlot : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External references
	[Comment("Optional, if not defined we'll automatically look for them in the nested hierarchy")]
	[SerializeField] private PetSlotInfo m_slotInfo = null;
	public PetSlotInfo slotInfo { 
		get { return m_slotInfo; }
	}

	[SerializeField] private PowerIcon m_powerIcon = null;
	public PowerIcon powerIcon {
		get { return m_powerIcon; }
	}

	[SerializeField] private MenuPetLoader m_petLoader = null;
	public MenuPetLoader petLoader {
		get { return m_petLoader; }
	}

	[Space]
	[Comment("Optional")]
	[SerializeField] private ShowHideAnimator m_equippedAnim = null;
	[SerializeField] private ShowHideAnimator m_emptyAnim = null;

	// Internal logic
	private int m_slotIdx = 0;
	public int slotIdx { 
		get { return m_slotIdx; }
	}

	private DragonData m_dragonData = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// If the required references are not initialized via inspector, look for them in the nested hierarchy
		if(m_slotInfo == null) m_slotInfo = this.GetComponentInChildren<PetSlotInfo>();
		if(m_powerIcon == null) m_powerIcon = this.GetComponentInChildren<PowerIcon>();
		if(m_petLoader == null) m_petLoader = this.GetComponentInChildren<MenuPetLoader>();

		// To make sure pet particles are properly scaled, but keeping in mind performance,
		// have a ParticleScaler constantly checking particle scales during the pet slot
		// show/hide animations, but disable it when idle
		petLoader.pscaler.m_whenScale = ParticleScaler.WhenScale.ALWAYS;
		DisableParticleScaler(m_slotInfo.equippedSlotAnim);
		m_slotInfo.equippedSlotAnim.OnShowPreAnimation.AddListener(EnableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnShowPostAnimation.AddListener(DisableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnHidePreAnimation.AddListener(EnableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnHidePostAnimation.AddListener(DisableParticleScaler);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unregister listeners
		m_slotInfo.equippedSlotAnim.OnShowPreAnimation.RemoveListener(EnableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnShowPostAnimation.RemoveListener(DisableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnHidePreAnimation.RemoveListener(EnableParticleScaler);
		m_slotInfo.equippedSlotAnim.OnHidePostAnimation.RemoveListener(DisableParticleScaler);
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

		// Initialize slot info and power icon
		m_slotInfo.Init(_slotIdx);
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

		// Get pet info
		DefinitionNode petDef = null;
		if(show) petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_dragonData.pets[m_slotIdx]);
		bool equipped = (petDef != null);

		// Refresh slot info
		m_slotInfo.Refresh(_dragonData, _animate);

		// Refresh power info
		if(show) {
			// Show
			m_powerIcon.gameObject.SetActive(true);
			m_powerIcon.anim.ForceShow(false);

			// Equipped?
			if(equipped) {
				// Get power definition
				DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
				m_powerIcon.InitFromDefinition(powerDef, false, _animate);
			} else {
				m_powerIcon.InitFromDefinition(null, false, _animate);
			}
		} else {
			// Instant hide
			m_powerIcon.anim.ForceHide(false);
		}

		// Toggle equipped/empty animators
		if(show) {
			// Equipped or empty?
			if(m_equippedAnim != null) m_equippedAnim.ForceSet(equipped, _animate);
			if(m_emptyAnim != null) m_emptyAnim.ForceSet(!equipped, _animate);
		}

		// Pet preview
		if(show) {
			// Equip or unequip?
			if(equipped) {
				// Don't reload if pet is already loaded
				if(petLoader.petSku != petDef.sku || petLoader.petInstance == null) {
					// The loader will do everything!
					petLoader.Load(petDef.sku);

					// Change render queue so the pet is renderd behind the UI!
					Renderer[] targetRenderers = petLoader.GetComponentsInChildren<Renderer>();
					for(int i = 0; i < targetRenderers.Length; ++i) {
						for(int j = 0; j < targetRenderers[i].materials.Length; ++j) {
							targetRenderers[i].materials[j].renderQueue = 3000;
						}
					}
				}
			} else {
				if(petLoader.petInstance != null) {
					// Animate?
					if(_animate) {
						// Toggle the OUT anim
						MenuPetPreview pet = petLoader.petInstance.GetComponent<MenuPetPreview>();
						pet.SetAnim(MenuPetPreview.Anim.OUT);

						// Program a delayed destruction of the pet preview (to give some time to see the anim)
						UbiBCN.CoroutineManager.DelayedCall(() => petLoader.Unload(), 0.3f, true);	// [AOC] MAGIC NUMBERS!! More or less synced with the animation
					} else {
						petLoader.Unload();
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Enable pet preview particle scaler.
	/// </summary>
	private void EnableParticleScaler(ShowHideAnimator _anim) {
		m_petLoader.pscaler.enabled = true;
	}

	/// <summary>
	/// Disable pet preview particle scaler.
	/// </summary>
	private void DisableParticleScaler(ShowHideAnimator _anim) {
		m_petLoader.pscaler.enabled = false;
	}
}