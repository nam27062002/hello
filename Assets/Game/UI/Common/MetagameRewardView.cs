// MetagameRewardView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/06/2018.
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
/// Widget to display the info of a metagame reward.
/// </summary>
public class MetagameRewardView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] protected Image m_icon = null;
	[SerializeField] protected TextMeshProUGUI m_rewardText = null;
	[Space]
	[SerializeField] protected bool m_showNameForEggsAndPets = true;	// [AOC] In some cases, the egg/pets names are an inconvenience and shouldn't be displayed
	[SerializeField] protected GameObject m_nameContainer = null;

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	protected Metagame.Reward m_reward = null;
	public Metagame.Reward reward {
		get { return m_reward; }
		set { InitFromReward(value); }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
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

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific reward.
	/// </summary>
	public void InitFromReward(Metagame.Reward _reward) {
		// Store new reward
		m_reward = _reward;

		// If given reward is null, disable game object
		this.gameObject.SetActive(m_reward != null);

		// Refresh visuals
		Refresh();
	}

	/// <summary>
	/// Refresh the visuals using current data.
	/// </summary>
	public virtual void Refresh() {
		if(m_reward == null) return;

		// Set reward icon and text
		// Based on type
		string rewardText = string.Empty;
		Sprite iconSprite = null;
		switch(m_reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				// Get the pet preview
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_reward.sku);
				if(petDef != null) {
					iconSprite = Resources.Load<Sprite>(UIConstants.PET_ICONS_PATH + petDef.Get("icon"));
					rewardText = petDef.GetLocalized("tidName");
				} else {
					// (shouldn't happen)
					iconSprite = null;
					rewardText = LocalizationManager.SharedInstance.Localize("TID_PET");
				}

				// [AOC] Don't show name for some specific cases
				if(!m_showNameForEggsAndPets) rewardText = string.Empty;
			} break;

			case Metagame.RewardEgg.TYPE_CODE: {
				// Get the egg definition
				string tid = "TID_EGG";
				DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_reward.sku);
				if(eggDef != null) {
					iconSprite = Resources.Load<Sprite>(UIConstants.EGG_ICONS_PATH + eggDef.Get("icon"));
					tid = eggDef.Get("tidName");
				} else {
					// (shouldn't happen) Use generic
					iconSprite = null;
				}

				// Use plural tid instead if needed
				if(m_reward.amount > 1) {
					tid = tid + "_PLURAL";
				}
				rewardText = LocalizationManager.SharedInstance.Localize(tid);

				// [AOC] Don't show name for some specific cases
				if(!m_showNameForEggsAndPets) rewardText = string.Empty;
			} break;

			case Metagame.RewardSoftCurrency.TYPE_CODE:
			case Metagame.RewardHardCurrency.TYPE_CODE:
			case Metagame.RewardGoldenFragments.TYPE_CODE: {
				// Get the icon linked to this currency
				iconSprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(m_reward.currency));
				rewardText = StringUtils.FormatNumber(m_reward.amount, 0);
			} break;
		
			default: {
				iconSprite = null;
				rewardText = "Unknown reward type";
			} break;
		}

		// Apply
		if(m_icon != null) m_icon.sprite = iconSprite;
		if(m_rewardText != null) m_rewardText.text = rewardText;
		if(m_nameContainer) m_nameContainer.SetActive(!string.IsNullOrEmpty(rewardText));	// If empty, hide the whole object
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, refresh texts.
	/// </summary>
	private void OnLanguageChanged() {
		// Refresh visuals
		Refresh();
	}
}