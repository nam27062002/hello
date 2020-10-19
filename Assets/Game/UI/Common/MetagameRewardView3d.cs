// MetagameRewardView3d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/10/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
/// Widget to display the info of a metagame reward.
/// </summary>
public class MetagameRewardView3d : MetagameRewardView {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[Tooltip("Optional")] [SerializeField] private MenuDragonLoader m_dragonLoader = null;
	[Tooltip("Optional")] [SerializeField] private MenuPetLoader m_petLoader = null;
	[Tooltip("Optional")] [SerializeField] private MenuEggLoader m_eggLoader = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_hcPreview = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_scPreview = null;
	[Space]
	[Tooltip("Optional")] [SerializeField] private DragControl m_dragControl = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Make sure dragon loader doesn't have any sku assigned
		if(m_dragonLoader != null) {
			m_dragonLoader.dragonSku = "";
			m_dragonLoader.onDragonLoaded += OnDragonLoaded;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
		// Unsubscribe from external events
		if(m_petLoader != null) {
			m_petLoader.OnLoadingComplete.RemoveListener(OnPetLoadingComplete);
		}

		if(m_dragonLoader != null) {
			m_dragonLoader.onDragonLoaded -= OnDragonLoaded;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear all 3D previews.
	/// </summary>
	private void Clear() {
		if(m_dragonLoader != null) {
			m_dragonLoader.dragonSku = "";
			m_dragonLoader.UnloadDragon();
			m_dragonLoader.gameObject.SetActive(false);
		}

		if(m_petLoader != null) {
			m_petLoader.Unload();
			m_petLoader.gameObject.SetActive(false);
		}

		if(m_eggLoader != null) {
			m_eggLoader.Unload();
			m_eggLoader.gameObject.SetActive(false);
		}

		if(m_hcPreview != null) {
			m_hcPreview.SetActive(false);
		}

		if(m_scPreview != null) {
			m_scPreview.SetActive(false);
		}

		if(m_dragControl != null) {
			m_dragControl.target = null;
			m_dragControl.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Refresh the visuals using current data.
	/// </summary>
	public override void Refresh() {
		if(m_reward == null) return;

		// Clear any loaded stuff
		Clear();

		// Based on type
		bool success = false;
		switch(m_reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				// Get the pet preview
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_reward.sku);
				if(petDef != null && m_petLoader != null) {
					// Initialize pet loader with the target pet preview!
					m_petLoader.Load(petDef.sku);
					m_petLoader.OnLoadingComplete.AddListener(OnPetLoadingComplete);
					m_petLoader.gameObject.SetActive(true);

					// Init drag control
					if(m_dragControl != null) {
						m_dragControl.gameObject.SetActive(true);
						m_dragControl.target = m_petLoader.transform;
					}

					success = true;
				}
			} break;

			case Metagame.RewardSkin.TYPE_CODE: {
				DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, m_reward.sku);
				if(skinDef != null && m_dragonLoader != null) {
					// Init dragon preview
					m_dragonLoader.LoadDragon(skinDef.GetAsString("dragonSku"), skinDef.sku);
					m_dragonLoader.gameObject.SetActive(true);

					// Init drag control
					if(m_dragControl != null) {
						m_dragControl.gameObject.SetActive(true);
						m_dragControl.target = m_dragonLoader.transform;
					}

					success = true;
				}
			} break;

			case Metagame.RewardDragon.TYPE_CODE: {
				DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, m_reward.sku);
				if(dragonDef != null && m_dragonLoader != null) {
					// Init dragon preview
					m_dragonLoader.LoadDragon(dragonDef.sku, IDragonData.GetDefaultDisguise(dragonDef.sku).sku);
					m_dragonLoader.gameObject.SetActive(true);
					
					// Init drag control
					if(m_dragControl != null) {
						m_dragControl.gameObject.SetActive(true);
						m_dragControl.target = m_dragonLoader.transform;
					}

					success = true;
				}
			} break;

			case Metagame.RewardEgg.TYPE_CODE:
			case Metagame.RewardMultiEgg.TYPE_CODE: {
				// Get the egg definition
				DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_reward.sku);
				if(eggDef != null && m_eggLoader != null) {
					m_eggLoader.Load(eggDef.sku);
					m_eggLoader.gameObject.SetActive(true);
					success = true;	
				}
			} break;

			case Metagame.RewardSoftCurrency.TYPE_CODE: {
				if(m_scPreview != null) {
					m_scPreview.gameObject.SetActive(true);
					success = true;
				}
			} break;

			case Metagame.RewardHardCurrency.TYPE_CODE: {
				if(m_hcPreview != null) {
					m_hcPreview.gameObject.SetActive(true);
					success = true;
				}
			} break;
		}

		// Let parent do the rest
		base.Refresh();

		// If 3d preview was successful, hide 2d preview
		if(m_icon != null) {
			m_icon.gameObject.SetActive(!success);
		}

		if(m_iconLoader != null) {
			m_iconLoader.gameObject.SetActive(false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS                                                              //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dragon has been loaded.
	/// </summary>
	/// <param name="_loader">Loader that triggered the event.</param>
	private void OnDragonLoaded(MenuDragonLoader _loader) {
		// Particle systems require a special initialization
		if(_loader.dragonInstance != null) {
			ParticleScaler scaler = _loader.GetComponentInChildren<ParticleScaler>();
			if(scaler != null) {
				scaler.DoScale();
			}
		}
	}

	/// <summary>
	/// Pet finished loading.
	/// </summary>
	/// <param name="_loader"></param>
	private void OnPetLoadingComplete(MenuPetLoader _loader) {
		// Particle systems require a special initialization
		if(_loader.petInstance != null) {
			_loader.pscaler.DoScale();
			_loader.OnLoadingComplete.RemoveListener(OnPetLoadingComplete);
		}
	}
}