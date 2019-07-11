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
public class MetagameRewardView : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Tooltip("Optional")] [SerializeField] protected Image m_icon = null;
    [Tooltip("Optional")] [SerializeField] protected UISpriteAddressablesLoader m_iconLoader = null;
    [Tooltip("Optional")] [SerializeField] protected TextMeshProUGUI m_rewardText = null;
	[Space]
	[SerializeField] protected bool m_showNameForEggsAndPets = true;    // [AOC] In some cases, the egg/pets names are an inconvenience and shouldn't be displayed
	[SerializeField] protected bool m_showNameForCurrencies = false;	// [AOC] Usually not needed, but in some cases looks better
	[Tooltip("Optional")] [SerializeField] protected GameObject m_nameContainer = null;
	[Space]
	[Tooltip("Optional")] [SerializeField] protected PowerIcon m_powerIcon = null;    // Will only be displayed for some types

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
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}
    
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
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

		// Based on type
		string rewardText = string.Empty;
		Sprite iconSprite = null;
		DefinitionNode powerDef = null;
		switch(m_reward.type) {
			case Metagame.RewardPet.TYPE_CODE: {
				// Get the pet preview
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_reward.sku);
				if(petDef != null) {
                    if (m_iconLoader != null)   m_iconLoader.LoadAsync(petDef.Get("icon"));
                    else                        iconSprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(petDef.Get("icon"));

					rewardText = petDef.GetLocalized("tidName");
					powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
				} else {
					// (shouldn't happen)
					rewardText = LocalizationManager.SharedInstance.Localize("TID_PET");
				}

				// [AOC] Don't show name for some specific cases
				if(!m_showNameForEggsAndPets) rewardText = string.Empty;
			} break;

			case Metagame.RewardSkin.TYPE_CODE: {
				DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, m_reward.sku);
				if(skinDef != null) {
                    if (m_iconLoader != null)   m_iconLoader.LoadAsync(skinDef.Get("icon"));
                    else                        iconSprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(skinDef.Get("icon"));
					rewardText = skinDef.GetLocalized("tidName");
					powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup"));
				} else {
					// (shouldn't happen)
					rewardText = LocalizationManager.SharedInstance.Localize("TID_DISGUISE");
				}
			} break;

			case Metagame.RewardDragon.TYPE_CODE: {
				DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, m_reward.sku);
				if(dragonDef != null) {
                    string defaultIcon = IDragonData.GetDefaultDisguise(dragonDef.sku).Get("icon");
                    if (m_iconLoader != null) m_iconLoader.LoadAsync(defaultIcon);
                    else iconSprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(defaultIcon);
					rewardText = dragonDef.GetLocalized("tidName");
					powerDef = null;
				} else {
					// (shouldn't happen)
					rewardText = LocalizationManager.SharedInstance.Localize("Dragon");
				}
			} break;

			case Metagame.RewardEgg.TYPE_CODE:
			case Metagame.RewardMultiEgg.TYPE_CODE: {
				// Get the egg definition
				string tidName = "TID_EGG";
				DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_reward.sku);
				if(eggDef != null) {
					m_iconLoader.LoadAsync(string.Empty);
					iconSprite = Resources.Load<Sprite>(UIConstants.EGG_ICONS_PATH + eggDef.Get("icon"));
					tidName = eggDef.Get("tidName");
				}

				// Use plural tid instead if needed
				if(m_reward.amount > 1) {
					tidName = tidName + "_PLURAL";
				}

				// Join with the amount given
				rewardText = LocalizationManager.SharedInstance.Localize(
					"TID_REWARD_AMOUNT",
					StringUtils.FormatNumber(m_reward.amount),
					LocalizationManager.SharedInstance.Localize(tidName)
				);

				// [AOC] Don't show name for some specific cases
				if(!m_showNameForEggsAndPets) rewardText = string.Empty;
			} break;

			case Metagame.RewardSoftCurrency.TYPE_CODE:
			case Metagame.RewardHardCurrency.TYPE_CODE:
			case Metagame.RewardGoldenFragments.TYPE_CODE: {
				// Get the icon linked to this currency
				m_iconLoader.LoadAsync(string.Empty);
				iconSprite = UIConstants.GetIconSprite(UIConstants.GetCurrencyIcon(m_reward.currency));

				// Show currency name?
				string amountText = StringUtils.FormatNumber(m_reward.amount, 0);
				if(m_showNameForCurrencies) {
					rewardText = LocalizationManager.SharedInstance.Localize(
						"TID_REWARD_AMOUNT",
						amountText,
						LocalizationManager.SharedInstance.Localize(m_reward.GetTID(m_reward.amount > 1))
					);
				} else {
					rewardText = amountText;
				}
			} break;
		
			default: {
				rewardText = "Unknown reward type";
			} break;
		}

		// Apply
		// Icon
		if(m_icon != null && iconSprite != null) {
			m_icon.sprite = iconSprite;
            m_icon.enabled = true;
		}

		// Reward
		if(m_rewardText != null) {
			m_rewardText.text = rewardText;	
		}

		// Name
		if(m_nameContainer != null) {
			m_nameContainer.SetActive(!string.IsNullOrEmpty(rewardText));   // If empty, hide the whole object
		}

		// Power
		if(m_powerIcon != null) {
			// Show?
			m_powerIcon.gameObject.SetActive(powerDef != null);

			// Initialize
			m_powerIcon.InitFromDefinition(powerDef, false);
		}
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