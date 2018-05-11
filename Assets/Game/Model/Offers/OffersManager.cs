// OffersManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global manager for offer packs.
/// </summary>
public class OffersManager : UbiBCN.SingletonMonoBehaviour<OffersManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float REFRESH_FREQUENCY = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Public
	private List<OfferPack> m_activeOffers = new List<OfferPack>();
	public static List<OfferPack> activeOffers {
		get { return instance.m_activeOffers; }
	}

	private OfferPack m_featuredOffer = new OfferPack();
	public static OfferPack featuredOffer {
		get { return instance.m_featuredOffer; }
	}

	// Internal
	private List<OfferPack> m_allOffers = new List<OfferPack>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to game events that might change offers list (segmentation)
		Messenger.AddListener<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, OnGameStateChanged1);
		Messenger.AddListener<string>(MessengerEvents.SCENE_UNLOADED, OnGameStateChanged2);
		Messenger.AddListener<DragonData>(MessengerEvents.DRAGON_ACQUIRED, OnGameStateChanged3);
		Messenger.AddListener<string>(MessengerEvents.SKIN_ACQUIRED, OnGameStateChanged2);
		Messenger.AddListener<string>(MessengerEvents.PET_ACQUIRED, OnGameStateChanged2);
		Messenger.AddListener<Egg>(MessengerEvents.EGG_OPENED, OnGameStateChanged4);
		Messenger.AddListener<OfferPack>(MessengerEvents.OFFER_APPLIED, OnGameStateChanged5);
	}

	/// <summary>
	/// First update loop.
	/// </summary>
	private void Start() {
		// Update the offers periodically
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, OnGameStateChanged1);
		Messenger.RemoveListener<string>(MessengerEvents.SCENE_UNLOADED, OnGameStateChanged2);
		Messenger.RemoveListener<DragonData>(MessengerEvents.DRAGON_ACQUIRED, OnGameStateChanged3);
		Messenger.RemoveListener<string>(MessengerEvents.SKIN_ACQUIRED, OnGameStateChanged2);
		Messenger.RemoveListener<string>(MessengerEvents.PET_ACQUIRED, OnGameStateChanged2);
		Messenger.RemoveListener<Egg>(MessengerEvents.EGG_OPENED, OnGameStateChanged4);
		Messenger.RemoveListener<OfferPack>(MessengerEvents.OFFER_APPLIED, OnGameStateChanged5);

		// Parent
		base.OnDestroy();
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions Manager must be ready before invoking this method.");

		// Aux vars
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();

		// Clear current offers cache
		instance.m_allOffers.Clear();
		instance.m_activeOffers.Clear();
		instance.m_featuredOffer = null;

		// Create data for each known offer pack definition
		List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		for(int i = 0; i < offerDefs.Count; ++i) {
			// Skip if offer is not enabled
			if(!offerDefs[i].GetAsBool("enabled", false)) continue;

			// Create new pack
			OfferPack newPack = new OfferPack();
			newPack.InitFromDefinition(offerDefs[i]);

			// Store new pack
			instance.m_allOffers.Add(newPack);
		}

		// Refresh active and featured offers
		instance.Refresh(true);

		// Notiy game
		Messenger.Broadcast(MessengerEvents.OFFERS_RELOADED);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Will refresh the list of offers to be displayed as well as the featured offer.
	/// </summary>
	/// <param name="_forceActiveRefresh">Force a refresh of the active offers list.</param>
	private void Refresh(bool _forceActiveRefresh) {
		// Aux vars
		bool dirty = false;
		List<OfferPack> toRemove = new List<OfferPack>();

		// Iterate all offer packs looking for state changes
		for(int i = 0; i < m_allOffers.Count; ++i) {
			// Has offer changed state?
			if(m_allOffers[i].UpdateState() || _forceActiveRefresh) {
				// Yes!
				dirty = true;

				// Which state? Refresh lists
				switch(m_allOffers[i].state) {
					case OfferPack.State.ACTIVE: {
						m_activeOffers.Add(m_allOffers[i]);
					} break;

					case OfferPack.State.EXPIRED: {
						toRemove.Add(m_allOffers[i]);
						m_activeOffers.Remove(m_allOffers[i]);
					} break;

					case OfferPack.State.PENDING_ACTIVATION: {
						m_activeOffers.Remove(m_allOffers[i]);
					} break;
				}

				// Update persistence with this pack's new state
				// [AOC] Packs Save() is smart, only stores packs when required
				UsersManager.currentUser.SaveOfferPack(m_allOffers[i]);
			}
		}

		// Remove expired offers (they won't be active anymore, no need to update them)
		for(int i = 0; i < toRemove.Count; ++i) {
			m_allOffers.Remove(toRemove[i]);
		}

		// Has any offer changed its state?
		if(dirty) {
			// Re-sort active offers
			m_activeOffers.Sort(OfferPackComparer);

			// New featured offer?
			m_featuredOffer = null;
			for(int i = 0; i < m_activeOffers.Count; ++i) {
				if(m_activeOffers[i].featured) {
					m_featuredOffer = m_activeOffers[i];
					break;
				}
			}

			// Save persistence
			PersistenceFacade.instance.Save_Request();

			// Notify game
			Messenger.Broadcast(MessengerEvents.OFFERS_CHANGED);
		}
	}

	/// <summary>
	/// Function used to sort offer packs.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_p1">First pack to compare.</param>
	/// <param name="_p2">Second pack to compare.</param>
	private static int OfferPackComparer(OfferPack _p1, OfferPack _p2) {
		// Featured packs come first
		if(_p1.featured && !_p2.featured) {
			return -1;
		} else if(!_p1.featured && _p2.featured) {
			return 1;
		}

		// Sort by order afterwards
		int order = _p1.order.CompareTo(_p2.order);
		if(order != 0) return order;

		// Then by discount
		int discount = _p1.def.GetAsFloat("discount").CompareTo(_p2.def.GetAsFloat("discount"));
		if(discount != 0) return -discount;	// Reverse: item with greater discount goes first!

		// Remaining time goes next
		int remainingTime = _p1.remainingTime.CompareTo(_p2.remainingTime);
		if(remainingTime != 0) return remainingTime;

		// Finally by reference price
		int price = _p1.def.GetAsFloat("refPrice").CompareTo(_p2.def.GetAsFloat("refPrice"));
		return -price;	// Reverse: item with greater price goes first! (usually a better pack)
	}

	//------------------------------------------------------------------------//
	// PUBLIC UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Very custom method to make sure all definitions related to offers contain
	/// all required default values.
	/// This is a requirement before applying customizations via CRM.
	/// If a parameter is missing in a definition, it will be added with the right
	/// default value for that parameter.
	/// </summary>
	public static void ValidateContent() {
		// Content must be loaded!
		Debug.Assert(ContentManager.ready, "Definitions Manager must be ready before invoking this method!");

		// Offer packs
		// Create a dummy offer pack with default values and use it to validate the definitions
		List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		OfferPack defaultPack = new OfferPack();
		defaultPack.Reset();
		for(int i = 0; i < offerDefs.Count; ++i) {
			defaultPack.ValidateDefinition(offerDefs[i]);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void PeriodicRefresh() {
		// Update offers
		Refresh(false);
	}

	/// <summary>
	/// Something generic has changed in the game that requires the offers segmentation
	/// to be checked.
	/// Different overloads to support different event parameters, but we don't actually care about them.
	/// </summary>
	private void OnGameStateChanged() {
		// [AOC] New implementation: Don't instantly refresh, periodic update will already do it.
		//Refresh();
	}
	private void OnGameStateChanged1(UserProfile.Currency _p1, long _p2, long _p3) {
		OnGameStateChanged(); 
	}
	private void OnGameStateChanged2(string _p1) { 
		OnGameStateChanged(); 
	}
	private void OnGameStateChanged3(DragonData _p1) { 
		OnGameStateChanged(); 
	}
	private void OnGameStateChanged4(Egg _p1) { 
		OnGameStateChanged(); 
	}
	private void OnGameStateChanged5(OfferPack _p1) { 
		OnGameStateChanged(); 
	}
}