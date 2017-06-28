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

	// Internal
	private GlobalEvent.Reward m_reward = null;
	public GlobalEvent.Reward reward {
		get { return m_reward; }
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
		Messenger.AddListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(EngineEvents.LANGUAGE_CHANGED, OnLanguageChanged);
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
	public void InitFromReward(GlobalEvent.Reward _reward) {
		// Store new reward
		m_reward = _reward;

		// If given reward is null, disable game object and don't do anything else
		if(m_reward == null) {
			this.gameObject.SetActive(false);
			return;
		}

		// Activate game object
		this.gameObject.SetActive(true);

		// Set reward icon
		if(m_icon != null) {
			// Based on type
			string type = m_reward.def.Get("type");
			switch(type) {
				case "pet": {
					// Get the pet preview
				} break;

				case "egg": {
					// Get the egg icon :s
				} break;

				case "sc":
				case "hc":
				case "goldenFragments": {
					// Get the icon linked to this currency
					UserProfile.Currency currency = UserProfile.SkuToCurrency(type);
					m_icon.sprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(currency));
				} break;

				default: {
					m_icon.sprite = null;
				} break;
			}

			Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_reward.def.Get("icon"));
		}

		// Set reward text
		if(m_rewardText != null) {
			// Based on type
			switch(m_reward.def.Get("type")) {
				// Pet: pet name (we assume there's never gonna be more than one pet rewarded)
				case "pet": {
					DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_reward.def.Get("gameSku"));
					if(petDef != null) {
						m_rewardText.text = petDef.GetLocalized("tidName");
					} else {
						// (shouldn't happen) Use generic
						m_rewardText.text = LocalizationManager.SharedInstance.Localize("TID_PET");
					}
				} break;

				// Egg: egg name (we assume there's never gonna be more than one egg rewarded)
				case "egg": {
					DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_reward.def.Get("gameSku"));
					if(eggDef != null) {
						m_rewardText.text = eggDef.GetLocalized("tidName");
					} else {
						// (shouldn't happen) Use generic
						m_rewardText.text = LocalizationManager.SharedInstance.Localize("TID_EGG");
					}
				} break;

				// Default (typically currencies): amount
				default: {
					m_rewardText.text = StringUtils.FormatNumber(m_reward.amount, 0);
				} break;
			}
		}

		// Set target text
		if(m_targetText != null) {
			// Abbreviated for big amounts
			m_targetText.text = StringUtils.FormatBigNumber(m_reward.targetAmount);
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
		InitFromReward(m_reward);
	}
}