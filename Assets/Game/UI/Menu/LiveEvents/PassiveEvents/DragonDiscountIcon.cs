// DragonDiscountIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the dragon discount icon in the menu HUD.
/// </summary>
public class DragonDiscountIcon : IPassiveEventIcon {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private UISpriteAddressablesLoader m_dragonIconLoader = null;
	[SerializeField] private Image m_tierIcon = null;
	[SerializeField] private TextMeshProUGUI m_discountText = null;
	[Space]
	[SerializeField] private GameObject m_arrow = null;
	[Space]
	[SerializeField] private Graphic[] m_toTint = null;

	// Internal properties for comfort
	private ModEconomyDragonPrice _mod {
		get { 
			if(m_passiveEventManager != null) {
				if(m_passiveEventManager.m_passiveEventData != null) {
					return m_passiveEventManager.m_passiveEventDefinition.mainMod as ModEconomyDragonPrice; 
				}
			}
			return null;
		}
	}

	private IDragonData _targetDragonData {
		get {
			ModEconomyDragonPrice mod = _mod;
			if(mod != null) {
				return DragonManager.GetDragonData(mod.dragonSku);
			}
			return null;
		}
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	protected override void Start() {
		// Call parent
		base.Start();

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.AddListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	protected override void OnDestroy() {
		// Call parent
		base.OnDestroy();

		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.RemoveListener<int, HDLiveDataManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewEventDefinition);
    }

	//------------------------------------------------------------------------//
	// IPassiveEventIcon IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the manager for this specific passive event type.
	/// </summary>
	/// <returns>The event manager corresponding to this event type.</returns>
	protected override HDPassiveEventManager GetEventManager() {
		return HDLiveDataManager.dragonDiscounts;
	}

	/// <summary>
	/// Update visuals when new data has been received.
	/// </summary>
	protected override void RefreshDataInternal() {
		// Aux vars
		ModEconomyDragonPrice mod = _mod;
		IDragonData targetDragonData = _targetDragonData;

		// Dragon icon
		if(m_dragonIconLoader != null) {
			bool show = false;
			if(targetDragonData != null) {
				show = true;
                string iconName = IDragonData.GetDefaultDisguise(targetDragonData.sku).Get("icon");
				m_dragonIconLoader.LoadAsync(iconName);
            }
		}

		// Tier icon
		if(m_tierIcon != null) {
			bool show = false;
			if(targetDragonData != null) {
				show = true;
				m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, targetDragonData.tierDef.GetAsString("icon"));
			}
			m_tierIcon.gameObject.SetActive(show);
		}

		// Color tint
		if(m_toTint != null && targetDragonData != null) {
			for(int i = 0; i < m_toTint.Length; ++i) {
				if(m_toTint[i] != null) {
					m_toTint[i].color = UIConstants.GetDragonTierColor(targetDragonData.tier);
				}
			}
		}

		// Discount text
		if(m_discountText != null) {
			bool show = false;
			if(targetDragonData != null) {
				show = true;
				m_discountText.text = LocalizationManager.SharedInstance.Localize(
					"TID_OFFER_DISCOUNT_PERCENTAGE",
					StringUtils.FormatNumber(Mathf.Abs(targetDragonData.GetPriceModifier(UserProfile.Currency.HARD)), 0)
				);
			}
			m_discountText.gameObject.SetActive(show);
		}
	}

	/// <summary>
	/// Do custom visibility checks based on passive event type.
	/// </summary>
	/// <returns>Whether the icon can be displayed or not.</returns>
	protected override bool RefreshVisibilityInternal() {
		// Only show in the menu
		if(InstanceManager.menuSceneController == null) return false;

		// Only show in the some specific screens
		MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
		if(currentScreen != MenuScreen.DRAGON_SELECTION) return false;

		// Don't show if mod not valid
		ModEconomyDragonPrice mod = _mod;
		if(mod == null) return false;

		// Don't show if target dragon not valid
		IDragonData targetDragonData = _targetDragonData;
		if(targetDragonData == null) return false;

		// Don't show if selected dragon is the target dragon
		if(InstanceManager.menuSceneController.selectedDragon == targetDragonData.sku) {
			return false;
		}

		// Don't show arrow if the target dragon is previous to the current selected dragon
		if(m_arrow != null) {
			bool showArrow = InstanceManager.menuSceneController.selectedDragonData.GetOrder() < targetDragonData.GetOrder();
			m_arrow.SetActive(showArrow);
		}

		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Selected dragon has changed.
	/// </summary>
	/// <param name="_dragonSku">Sku of the newly selected dragon.</param>
	private void OnDragonSelected(string _dragonSku) {
		RefreshVisibility();
	}

    private void OnNewEventDefinition(int _eventID, HDLiveDataManager.ComunicationErrorCodes _errorCode) {
        RefreshVisibility();
    }

    /// <summary>
    /// The scroll to target button has been pressed.
    /// </summary>
    public void OnScrollToTargetDragon() {
		// Just do it
		IDragonData targetDragonData = _targetDragonData;
		if(targetDragonData != null) {
			InstanceManager.menuSceneController.SetSelectedDragon(targetDragonData.sku);
		}
	}
}