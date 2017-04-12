// CurrencyFlow.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This class encapsulates the whole logic of purchasing anything in the game with
/// either SC or PC. Manages the check of required currencies, computes the required
/// PC to top up price and opens the PC shop if needed.
/// If the flow is completed, performs the currency transaction on the profile and
/// invokes the corresponding Callback to notify its result so item transaction can
/// be completed.
/// </summary>
public class ResourcesFlow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// States
	public enum State {
		INIT,

		SHOWING_MISSING_CURRENCY,
		SHOWING_MISSING_EXTRA_PC,

		ASKING_BIG_AMOUNT_CONFIRMATION,

		FINISHED_SUCCESS,
		FINISHED_CANCELED,
		FINISHED_ERROR,
		FINISHED_PC_SHOP
	}

	// Constant values
	public const long PC_CONFIRMATION_POPUP_THRESHOLD = 20;	// Show confirmation popup for PC purchases bigger than this threshold

	// Custom events
	public class ResourcesFlowEvent : UnityEvent<ResourcesFlow> { };
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Flow data
	private string m_name = "";	// Identifier, mostly for debug purposes
	public string name {
		get { return m_name; }
		set { m_name = value; }
	}

	// Transaction Data
	private DefinitionNode m_itemDef = null;	// Optional, item to be purchased
	public DefinitionNode itemDef {
		get { return m_itemDef; }
		set { m_itemDef = value; }
	}

	// Base prices
	private UserProfile.Currency m_currency = 0;
	public UserProfile.Currency currency {
		get { return m_currency; }
	}

	private long m_originalAmount = 0;
	public long originalAmount {
		get { return m_originalAmount; }
	}
	
	// Missing resources
	private long m_missingAmount = 0;
	public long missingAmount {
		get { return m_missingAmount; }
	}

	// Actual amount considering current resources
	private long m_finalAmount = 0;
	public long finalAmount {
		get { return m_finalAmount; }
	}

	// Total extra PC cost of buying missing resources
	private long m_extraPCCost = 0;
	public long extraPCCost {
		get { return m_extraPCCost; }
	}

	// Missing PC for the extra cost
	private long m_missingExtraPC = 0;
	public long missingExtraPC {
		get { return m_missingExtraPC; }
	}

	private DefinitionNode m_recommendedPCPackDef = null;
	public DefinitionNode recommendedPCPackDef {
		get { return m_recommendedPCPackDef; }
	}

	// Popups
	private List<PopupController> m_popups = new List<PopupController>();

	// Events
	public ResourcesFlowEvent OnSuccess = new ResourcesFlowEvent();
	public ResourcesFlowEvent OnCancel = new ResourcesFlowEvent();
	public ResourcesFlowEvent OnFinished = new ResourcesFlowEvent();	// The flow has finished, regardless of its result (success, cancel, error, etc.)

	// Internal logic
	private State m_state = State.INIT;
	public State state {
		get { return m_state; }
	}

	public bool successful {
		get { return m_state == State.FINISHED_SUCCESS; }
	}

	// Internal data
	private List<DefinitionNode> m_pcPackDefinitions = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	/// <param name="_name">Optional identifier to be given to the flow, mostly for debugging.</param>
	public ResourcesFlow(string _name = "") {
		m_name = _name;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	~ResourcesFlow() {
		
	}

	/// <summary>
	/// Start the flow.
	/// Prices should have been set.
	/// </summary>
	/// <param name="_targetAmount">How much are we trying to spend?</param>
	/// <param name="_currency">Which currency are we trying to spend?</param>
	/// <param name="_itemDef">Optional, which item are we trying to buy? (Only for visual purposes)</param>
	public void Begin(long _targetAmount, UserProfile.Currency _currency, DefinitionNode _itemDef) {
		// Only from Init state!
		if(m_state != State.INIT) return;

		// Initialize internal vars
		m_currency = _currency;
		m_itemDef = _itemDef;

		// Currency amounts
		m_originalAmount = _targetAmount;
		m_missingAmount = System.Math.Max(0, m_originalAmount - UsersManager.currentUser.GetCurrency(_currency));	// Non-negative!
		m_finalAmount = m_originalAmount - m_missingAmount;

		// Extra PC price of missing resources
		// Depends on currency!
		switch(_currency) {
			case UserProfile.Currency.SOFT: {
				m_extraPCCost = GameSettings.ComputePCForCoins(m_missingAmount);
			} break;

			default:{ 
				m_extraPCCost = 0;
			} break;
		}
		m_missingExtraPC = System.Math.Max(0, m_extraPCCost - UsersManager.currentUser.pc);	// Non-negative!

		// If a resource has failed, decide what to do
		if(m_missingAmount > 0) {
			// Depends on currency
			switch(_currency) {
				case UserProfile.Currency.SOFT: {
					// Show popup to buy missing resources with PC
					PopupController popup = PopupManager.LoadPopup(ResourcesFlowMissingSCPopup.PATH);
					ResourcesFlowMissingSCPopup coinsPopup = popup.GetComponent<ResourcesFlowMissingSCPopup>();
					coinsPopup.Init(m_missingAmount, m_extraPCCost);

					coinsPopup.OnAccept.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
					coinsPopup.OnAccept.AddListener(OnBuyMissingCoins);

					coinsPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
					coinsPopup.OnCancel.AddListener(Cancel);

					popup.Open();
					m_popups.Add(popup);
				} break;

				case UserProfile.Currency.HARD: {
					// [AOC] TODO!! Show confirmation popup BEFORE starting missing amount flow

					// Find recommended shop pack for the missing amount
					m_recommendedPCPackDef = FindRecommendedPCPack(m_missingAmount);

					// Show popup to buy missing PC
					PopupController popup = PopupManager.LoadPopup(ResourcesFlowMissingPCPopup.PATH);
					ResourcesFlowMissingPCPopup pcPopup = popup.GetComponent<ResourcesFlowMissingPCPopup>();
					pcPopup.Init(m_recommendedPCPackDef);

					pcPopup.OnRecommendedPackPurchased.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
					pcPopup.OnRecommendedPackPurchased.AddListener(OnPCPackPurchased);

					pcPopup.OnGoToShop.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
					pcPopup.OnGoToShop.AddListener(OnGoToPCShop);

					pcPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
					pcPopup.OnCancel.AddListener(Cancel);

					popup.Open();
					m_popups.Add(popup);
				} break;
			}

			// Change state
			m_state = State.SHOWING_MISSING_CURRENCY;
		}

		// Everything ok! Do the transaction
		else {
			// Everything ok!
			DoTransaction(true);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Close any popups/UI opened by this flow.
	/// </summary>
	private void Close() {
		// Close any open popup
		// Reverse order for better visual effect
		for(int i = m_popups.Count - 1; i >= 0; i--) {
			m_popups[i].Close(false);	// Don't destroy, let's reuse popups from flow to flow! (popup manager will take care of that)
		}
		m_popups.Clear();

		// Notify listeners
		OnFinished.Invoke(this);
	}

	/// <summary>
	/// Cancel the flow.
	/// </summary>
	private void Cancel() {
		// Change state
		m_state = State.FINISHED_CANCELED;

		// Notify!
		OnCancel.Invoke(this);

		// Close flow
		Close();
	}

	/// <summary>
	/// Perform the transaction! Should only be called once when all the checks
	/// have been passed (all missing resources purchased).
	/// It will actually subtract m_finalAmount from the target currency plus m_extraPCCost 
	/// from PC, so there is no need to manually do intermediate transactions 
	/// other than purchasing more PC for the extra cost.
	/// </summary>
	/// <param name="_askConfirmationForBigPCAmounts">If set to true, a confirmation popup will be triggered for big PC amounts and the transaction wont happen until the popup is confirmed.</param>
	private void DoTransaction(bool _askConfirmationForBigPCAmounts) {
		// Confirmation required?
		if(_askConfirmationForBigPCAmounts) {
			// Final PC cost?
			long finalPCAmount = 0;

			// a) Purchasing with PC (no extra PC cost when purchasing with PC)
			if(m_currency == UserProfile.Currency.HARD) {
				finalPCAmount = m_finalAmount;
			}

			// b) Not enough resources
			else {
				finalPCAmount = m_extraPCCost;
			}

			// Final PC amount over threshold?
			if(finalPCAmount > PC_CONFIRMATION_POPUP_THRESHOLD) {
				// Show confirmation popup
				PopupController popup = PopupManager.LoadPopup(ResourcesFlowBigAmountConfirmationPopup.PATH);
				ResourcesFlowBigAmountConfirmationPopup confirmationPopup = popup.GetComponent<ResourcesFlowBigAmountConfirmationPopup>();
				confirmationPopup.Init(finalPCAmount);

				confirmationPopup.OnAccept.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
				confirmationPopup.OnAccept.AddListener(() => { DoTransaction(false); });	// Finally do the transaction!

				confirmationPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
				confirmationPopup.OnCancel.AddListener(() => { Cancel(); });	// Cancel flow

				popup.Open();
				m_popups.Add(popup);

				// Change state
				m_state = State.ASKING_BIG_AMOUNT_CONFIRMATION;

				// Don't do anything else until confirmed by user
				return;
			}
		}

		// Transaction confirmed!
		// Just in case, doublecheck that the player has enough currencies
		if(m_finalAmount > UsersManager.currentUser.GetCurrency(m_currency)
		|| m_extraPCCost > UsersManager.currentUser.pc) {
			// Move to error state
			m_state = State.FINISHED_ERROR;

			// Show default message
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_RESOURCES_FLOW_UNKNOWN_ERROR"), new Vector2(0.5f, 0.33f), PopupManager.canvas.transform as RectTransform);	// Use popup's canvas

			// Close the flow
			Close();
			return;
		}

		// [AOC] TODO!! Tracking

		// Currency transaction
		if(m_finalAmount > 0) {
			UsersManager.currentUser.AddCurrency(-m_finalAmount, m_currency);
		}

		// Extra PC Cost Transaction
		if(m_extraPCCost > 0) {
			UsersManager.currentUser.AddPC(-m_extraPCCost);
		}

		// Change state
		m_state = State.FINISHED_SUCCESS;

		// Notify!
		OnSuccess.Invoke(this);

		// Close any open popups
		Close();
	}

	/// <summary>
	/// Find the closest PC pack covering a given amount.
	/// </summary>
	/// <returns>The definition of the recommended PC pack.</returns>
	/// <param name="_pcAmount">Target PC amount.</param>
	private DefinitionNode FindRecommendedPCPack(long _pcAmount) {
		// If not already done, initialize definitions list
		if(m_pcPackDefinitions == null) {
			// Get all PC pack definitions and sort them by PC amount given
			m_pcPackDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", "hc");	// Hard currency packs only
			DefinitionsManager.SharedInstance.SortByProperty(ref m_pcPackDefinitions, "amount", DefinitionsManager.SortType.NUMERIC);
		}

		// Definitions are sorted, should be easy to find the right pack!
		for(int i = 0; i < m_pcPackDefinitions.Count; i++) {
			// Is it the first pack covering our target amount?
			if(m_pcPackDefinitions[i].GetAsLong("amount") >= _pcAmount) {
				// Yes!! Return it
				return m_pcPackDefinitions[i];
			}
		}

		// No pack was found, return biggest available pack
		return m_pcPackDefinitions.Last();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Confirmation from the player to buy missing coins with PC.
	/// </summary>
	private void OnBuyMissingCoins() {
		// If the player doesn't have enough PC, let him buy some!
		if(m_missingExtraPC > 0) {
			// Find recommended shop pack for the missing amount
			m_recommendedPCPackDef = FindRecommendedPCPack(m_missingExtraPC);

			// Show popup to buy missing PC
			PopupController popup = PopupManager.LoadPopup(ResourcesFlowMissingPCPopup.PATH);
			ResourcesFlowMissingPCPopup pcPopup = popup.GetComponent<ResourcesFlowMissingPCPopup>();
			pcPopup.Init(m_recommendedPCPackDef);

			pcPopup.OnRecommendedPackPurchased.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
			pcPopup.OnRecommendedPackPurchased.AddListener(OnPCPackPurchased);

			pcPopup.OnGoToShop.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
			pcPopup.OnGoToShop.AddListener(OnGoToPCShop);

			pcPopup.OnCancel.RemoveAllListeners();	// We're recycling popups, so we don't want events to be added twice!
			pcPopup.OnCancel.AddListener(Cancel);

			popup.Open();
			m_popups.Add(popup);

			// Change state
			m_state = State.SHOWING_MISSING_EXTRA_PC;
		}

		// Otherwise buy the missing currency with PC
		else {
			// Transaction will do everything! ^_^
			DoTransaction(false);	// Ask confirmation? No, looks weird after having purchased the gems
		}
	}

	/// <summary>
	/// The recommended PC pack has been purchased.
	/// </summary>
	private void OnPCPackPurchased() {
		// We should have enough PC to complete the transaction now, do it!
		DoTransaction(false);	// Ask confirmation? No, looks weird after having purchased the pack
	}

	/// <summary>
	/// While buying PC, the player asks for more offers.
	/// </summary>
	private void OnGoToPCShop() {
		// Open PC shop
		PopupController popup = PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		PopupCurrencyShop shopPopup = popup.GetComponent<PopupCurrencyShop>();
		shopPopup.Init(PopupCurrencyShop.Mode.PC_ONLY);

		// Change state
		m_state = State.FINISHED_PC_SHOP;

		// Cancel flow?
		// [AOC] We should, it's quite difficult to track player's steps from now on (although we could listen to shop events)
		Cancel();
	}
}