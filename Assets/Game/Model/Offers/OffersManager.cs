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
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<UserProfile.Currency, long, long>(MessengerEvents.PROFILE_CURRENCY_CHANGED, OnGameStateChanged1);
		Messenger.RemoveListener<string>(MessengerEvents.SCENE_UNLOADED, OnGameStateChanged2);
		Messenger.RemoveListener<DragonData>(MessengerEvents.DRAGON_ACQUIRED, OnGameStateChanged3);
		Messenger.RemoveListener<string>(MessengerEvents.SKIN_ACQUIRED, OnGameStateChanged2);
		Messenger.RemoveListener<string>(MessengerEvents.PET_ACQUIRED, OnGameStateChanged2);
		Messenger.RemoveListener<Egg>(MessengerEvents.EGG_OPENED, OnGameStateChanged4);
		Messenger.RemoveListener<OfferPack>(MessengerEvents.OFFER_APPLIED, OnGameStateChanged5);
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

			// [AOC] Small optimization: remove featured offers that end in the past
			if(newPack.featured && newPack.endDate < serverTime) {
				continue;
			}

			// Store new pack
			instance.m_allOffers.Add(newPack);
		}

		// Sort the offers list using custom criteria
		instance.m_allOffers.Sort(OfferPackComparer);

		// Refresh active and featured offers
		instance.Refresh();

		// Notiy game
		Messenger.Broadcast(MessengerEvents.OFFERS_RELOADED);
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Will refresh the list of offers to be displayed as well as the featured offer.
	/// </summary>
	public void Refresh() {
		// Iterate all offers checking which ones can be displayed
		// Offers are already sorted
		m_activeOffers.Clear();
		m_featuredOffer = null;
		for(int i = 0; i < m_allOffers.Count; ++i) {
			// Must this offer be displayed?
			if(m_allOffers[i].CanBeDisplayed()) {
				// Yes! Store it as active
				m_activeOffers.Add(m_allOffers[i]);

				// Is this offer featured?
				if(m_featuredOffer == null && m_allOffers[i].featured) {
					m_featuredOffer = m_allOffers[i];
				}
			}
		}

		// Notify game
		Messenger.Broadcast(MessengerEvents.OFFERS_CHANGED);
	}

	/// <summary>
	/// Refreshes the featured offer:
	/// If a featured offer exists, check its expiring time and look for a new one if it has ended.
	/// If a featured offer doesn't exist, look for a new one.
	/// </summary>
	/// <returns>The new featured offer, if any (or the current one if it haasn't changed).</returns>
	public OfferPack RefreshFeaturedOffer() {
		// Aux vars
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();

		// If a fetured offer exists, check its expiration date
		if(m_featuredOffer != null) {
			// Has it expired?
			if(serverTime > m_featuredOffer.endDate) {
				// Offer has expired, remove it from active offers and clear reference
				m_activeOffers.Remove(m_featuredOffer);
				m_featuredOffer = null;
			}
		}

		// If no featured offer, look for a new one
		if(m_featuredOffer == null) {
			// Actually refresh all active offers
			Refresh();
		}

		return m_featuredOffer;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
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
	/// Something generic has changed in the game that requires the offers segmentation
	/// to be checked.
	/// Different overloads to support different event parameters, but we don't actually care about them.
	/// </summary>
	private void OnGameStateChanged() {
		Refresh();
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