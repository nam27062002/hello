// OfferItemPreviewSkin3d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Simple class to encapsulate the preview of an item.
/// </summary>
public class OfferItemPreviewSkin3d : IOfferItemPreview {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override OfferItemPrefabs.PrefabType type {
		get { return OfferItemPrefabs.PrefabType.PREVIEW_3D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure dragon loader doesn't have any sku assigned
		m_dragonLoader.dragonSku = "";
		m_dragonLoader.onDragonLoaded += OnDragonLoaded;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_dragonLoader.onDragonLoaded -= OnDragonLoaded;
	}

	//------------------------------------------------------------------------//
	// OfferItemPreview IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Item must be a skin!
		Debug.Assert(m_item.reward is Metagame.RewardSkin, "ITEM OF THE WRONG TYPE!", this);

		// Initialize dragon loader with the target dragon and skin!
		m_def = m_item.reward.def;
		if(m_def == null) {
			m_dragonLoader.LoadDragon("");
		} else {
			m_dragonLoader.LoadDragon(m_def.GetAsString("dragonSku"), m_def.sku);
		}
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public override string GetLocalizedDescription() {
		if(m_def != null) {
			return m_def.GetLocalized("tidName");
		}
		return LocalizationManager.SharedInstance.Localize("TID_DISGUISE");	// (shouldn't happen) use generic
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed.
	/// </summary>
	/// <param name="_trackingLocation">Where is this been triggered from?</param>
	override public void OnInfoButton(string _trackingLocation) {
		// Open info popup
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"),
			GameConstants.Vector2.center,
			GetComponentInParent<Canvas>().transform as RectTransform
		);
		/*PopupController popup = PopupManager.LoadPopup(PopupInfoEggDropChance.PATH);
		popup.GetComponent<PopupInfoEggDropChance>().Init(m_item.sku);
		popup.Open();*/
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dragon has been loaded.
	/// </summary>
	/// <param name="_loader">Loader that triggered the event.</param>
	private void OnDragonLoaded(MenuDragonLoader _loader) {
		// Particle systems require a special initialization
		InitParticles(m_dragonLoader.dragonInstance.gameObject);
	}
}