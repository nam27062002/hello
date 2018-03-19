// OfferItemUI.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2018.
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
/// Widget to display the info of an offer pack reward.
/// </summary>
public class OfferItemUI : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Transform m_previewContainer = null;
	[SerializeField] private TextMeshProUGUI m_text = null;
	[Space]
	[SerializeField] private bool m_allow3dPreview = false;	// [AOC] In some cases, we want to display a 3d preview when appliable (pets/eggs)

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	private OfferPackItem m_item = null;
	public OfferPackItem item {
		get { return m_item; }
		set { InitFromItem(value); }
	}

	private GameObject m_preview = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.LANGUAGE_CHANGED, OnLanguageChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.LANGUAGE_CHANGED, OnLanguageChanged);
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

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific offer item.
	/// </summary>
	public void InitFromItem(OfferPackItem _item, bool _reloadPreview = true) {
		// Force reloading preview if item is different than the current one
		if(m_item != _item) _reloadPreview = true;

		// Store new item
		m_item = _item;

		// If given item is null, disable game object and don't do anything else
		if(m_item == null) {
			this.gameObject.SetActive(false);
			if(_reloadPreview) ClearPreview();
			return;
		}

		// Activate game object
		this.gameObject.SetActive(true);

		// If a preview was already created, destroy it
		if(_reloadPreview) ClearPreview();

		// Set reward preview and text
		// Based on type
		Metagame.Reward reward = item.reward;
		if(reward is Metagame.RewardPet) {
			// Get the pet preview
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, reward.sku);
			if(petDef != null) {
				if(_reloadPreview) {
					// 3d preview?
					if(m_allow3dPreview) {
						// [AOC] TODO!!
					} else {
						// [AOC] TODO!!
						//Resources.Load<Sprite>(UIConstants.PET_ICONS_PATH + petDef.Get("icon"));
					}
				}
				m_text.text = petDef.GetLocalized("tidName");
			} else {
				// (shouldn't happen)
				m_text.text = LocalizationManager.SharedInstance.Localize("TID_PET");
			}
		} else if(reward is Metagame.RewardEgg) {
			// Get the egg definition
			DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, reward.sku);
			if(eggDef != null) {
				if(_reloadPreview) {
					// 3d preview?
					if(m_allow3dPreview) {
						// [AOC] TODO!!
					} else {
						// [AOC] TODO!!
						//Resources.Load<Sprite>(UIConstants.EGG_ICONS_PATH + eggDef.Get("icon"));
					}
				}
				m_text.text = eggDef.GetLocalized("tidName");
			} else {
				// (shouldn't happen) Use generic
				m_text.text = LocalizationManager.SharedInstance.Localize("TID_EGG");
			}
		} else if(reward is Metagame.RewardCurrency) {
			if(_reloadPreview) {
				// Get the icon linked to this currency
				//UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(reward.currency));	// [AOC] TODO!!
			}
			m_text.text = StringUtils.FormatNumber(reward.amount, 0);	// [AOC] TODO!! Better text: 30 coins, 1000 gems, etc.
		} else {
			m_text.text = "Unknown reward type";
		}
	}

	/// <summary>
	/// Destroy current preview, if any.
	/// </summary>
	private void ClearPreview() {
		if(m_preview != null) {
			Destroy(m_preview);
			m_preview = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, refresh texts.
	/// </summary>
	private void OnLanguageChanged() {
		// Reapply current reward
		InitFromItem(m_item, false);
	}
}