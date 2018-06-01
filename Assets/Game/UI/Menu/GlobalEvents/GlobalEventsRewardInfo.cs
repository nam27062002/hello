// GlobalEventsScreenRewardInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2017.
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
/// Widget to display the info of a global event reward.
/// </summary>
public class GlobalEventsRewardInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Image m_icon = null;
	[SerializeField] private TextMeshProUGUI m_rewardText = null;
	[SerializeField] private TextMeshProUGUI m_targetText = null;
	[Space]
	[SerializeField] private bool m_showNameForEggsAndPets = true;	// [AOC] In some cases, the egg/pets names are an inconvenience and shouldn't be displayed
	[SerializeField] private GameObject m_nameContainer = null;

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	private HDQuestDefinition.HDLiveEventReward m_rewardSlot = null;
	public HDQuestDefinition.HDLiveEventReward rewardSlot {
		get { return m_rewardSlot; }
		set { InitFromReward(value); }
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
	/// Refresh the widget with the data of a specific reward.
	/// </summary>
	public void InitFromReward(HDQuestDefinition.HDLiveEventReward _rewardSlot) {
		// Store new reward
		m_rewardSlot = _rewardSlot;

		// If given reward is null, disable game object and don't do anything else
		if(m_rewardSlot == null) {
			this.gameObject.SetActive(false);
			return;
		}

		// Activate game object
		this.gameObject.SetActive(true);


		// Set reward icon and text
		// Based on type
		string rewardText = string.Empty;
		Sprite iconSprite = null;
		Metagame.Reward reward = _rewardSlot.reward;
		if (reward is Metagame.RewardPet) {
			// Get the pet preview
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, reward.sku);
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
		} else if (reward is Metagame.RewardEgg) {
			// Get the egg definition
			DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, reward.sku);
			if(eggDef != null) {
				iconSprite = Resources.Load<Sprite>(UIConstants.EGG_ICONS_PATH + eggDef.Get("icon"));
				rewardText = eggDef.GetLocalized("tidName");
			} else {
				// (shouldn't happen) Use generic
				iconSprite = null;
				rewardText = LocalizationManager.SharedInstance.Localize("TID_EGG");
			}

			// [AOC] Don't show name for some specific cases
			if(!m_showNameForEggsAndPets) rewardText = string.Empty;
		} else if (reward is Metagame.RewardCurrency) {
			// Get the icon linked to this currency
			iconSprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(reward.currency));
			rewardText = StringUtils.FormatNumber(reward.amount, 0);
		} else {
			iconSprite = null;
			rewardText = "Unknown reward type";
		}

		// Apply
		if(m_icon != null) m_icon.sprite = iconSprite;
		if(m_rewardText != null) m_rewardText.text = rewardText;
		if(m_nameContainer) m_nameContainer.SetActive(!string.IsNullOrEmpty(rewardText));	// If empty, hide the whole object

		// Set target text
		if(m_targetText != null) {
			// Abbreviated for big amounts
			// m_targetText.text = StringUtils.FormatBigNumber(m_rewardSlot.targetAmount);
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
		InitFromReward(m_rewardSlot);
	}
}