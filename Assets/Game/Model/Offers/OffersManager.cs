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
	private const float REFRESH_FREQUENCY = 1f; // Seconds
	private const int MAX_ACTIVE_ROTATIONAL_OFFERS = 1;
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Public
	private List<OfferPack> m_activeOffers = new List<OfferPack>();		// All currently active offers (state ACTIVE), regardless of type
	public static List<OfferPack> activeOffers {
		get { return instance.m_activeOffers; }
	}

	private OfferPack m_featuredOffer = null;
	public static OfferPack featuredOffer {
		get { return instance.m_featuredOffer; }
	}

	// Internal
	private List<OfferPack> m_allEnabledOffers = new List<OfferPack>();	// All enabled offer packs, regardless of type
	private List<OfferPack> m_allOffers = new List<OfferPack>();        // All defined offer packs, regardless of state and type
	private List<OfferPack> m_allRotationalOffers = new List<OfferPack>();  // All rotational offers, regardless of state
	private List<OfferPack> m_activeRotationalOffers = new List<OfferPack>();	// Currently active rotational offers
	private Queue<string> m_rotationalHistory = new Queue<string>();	// Historic of rotational offers
	private List<OfferPack> m_offersToRemove = new List<OfferPack>();   // Temporal list of offers to be removed from active lists
    private float m_timer = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update loop.
	/// </summary>
	private void Start() {
        m_timer = 0;
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Refresh offers periodically for better performance
		if(m_timer <= 0) {
			m_timer = REFRESH_FREQUENCY;
			Refresh(false);
		}
		m_timer -= Time.deltaTime;
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
		instance.m_allEnabledOffers.Clear();
		instance.m_activeOffers.Clear();
		instance.m_offersToRemove.Clear();
		instance.m_allRotationalOffers.Clear();
		instance.m_activeRotationalOffers.Clear();
		instance.m_featuredOffer = null;

		// Get all known offer packs
		// Sort offers by their "order" field, so if two mutually exclusive offers (same uniqueId)
		// are triggered at the same time, we can control which one shows
		List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		DefinitionsManager.SharedInstance.SortByProperty(ref offerDefs, "order", DefinitionsManager.SortType.NUMERIC);

		// Create data for each known offer pack definition
		for(int i = 0; i < offerDefs.Count; ++i) {
			// Create and initialize new pack
			OfferPack newPack = OfferPack.CreateFromDefinition(offerDefs[i]);

            // Store new pack
            instance.m_allOffers.Add(newPack);

			// If enabled, store to the enabled collection
			if(offerDefs[i].GetAsBool("enabled", false)) {
				instance.m_allEnabledOffers.Add(newPack);
			}

			// If rotational, store to the rotational collection
			if(newPack.type == OfferPack.Type.ROTATIONAL) {
				instance.m_allRotationalOffers.Add(newPack);
			}
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
		OfferPack pack = null;
		bool dirty = false;
		m_offersToRemove.Clear();

		// Iterate active rotational packs to check for expiration
		bool newRotationalNeeded = false;
		for(int i = 0; i < m_activeRotationalOffers.Count; ++i) {
			// 
		}

		// Iterate all offer packs looking for state changes
		for(int i = 0; i < m_allEnabledOffers.Count; ++i) {
			// Aux vars
			pack = m_allEnabledOffers[i];

			// Process pack depending on its type
			switch(pack.type) {
				// Classic packs
				case OfferPack.Type.PROGRESSION:
				case OfferPack.Type.PUSHED: {
					// Has offer changed state?
					if(pack.UpdateState() || _forceActiveRefresh) {
						// Yes!
						dirty = true;

						// Update lists
						UpdateCollections(pack);

						// Update persistence with this pack's new state
						// [AOC] Packs Save() is smart, only stores packs when required
						UsersManager.currentUser.SaveOfferPack(pack);
					}
				} break;

				// Rotational packs
				case OfferPack.Type.ROTATIONAL: {
					//		__/\\\\\\\\\\\\\\\________/\\\\\________/\\\\\\\\\\\\___________/\\\\\___________/\\\_________/\\\____
					//		 _\///////\\\/////_______/\\\///\\\_____\/\\\////////\\\_______/\\\///\\\_______/\\\\\\\_____/\\\\\\\__
					//		  _______\/\\\__________/\\\/__\///\\\___\/\\\______\//\\\____/\\\/__\///\\\____/\\\\\\\\\___/\\\\\\\\\_
					//		   _______\/\\\_________/\\\______\//\\\__\/\\\_______\/\\\___/\\\______\//\\\__\//\\\\\\\___\//\\\\\\\__
					//		    _______\/\\\________\/\\\_______\/\\\__\/\\\_______\/\\\__\/\\\_______\/\\\___\//\\\\\_____\//\\\\\___
					//		     _______\/\\\________\//\\\______/\\\___\/\\\_______\/\\\__\//\\\______/\\\_____\//\\\_______\//\\\____
					//		      _______\/\\\_________\///\\\__/\\\_____\/\\\_______/\\\____\///\\\__/\\\________\///_________\///_____
					//		       _______\/\\\___________\///\\\\\/______\/\\\\\\\\\\\\/_______\///\\\\\/__________/\\\_________/\\\____
					//		        _______\///______________\/////________\////////////___________\/////___________\///_________\///_____
				} break;
			}
		}

		// Remove expired offers (they won't be active anymore, no need to update them)
		for(int i = 0; i < m_offersToRemove.Count; ++i) {
			m_allEnabledOffers.Remove(m_offersToRemove[i]);
		}
		m_offersToRemove.Clear();

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
	/// Refresh the offer collections adding/removing the given offer based
	/// on its type and state.
	/// </summary>
	/// <param name="_offer">Offer to be processed.</param>
	private void UpdateCollections(OfferPack _offer) {
		// Which state? Refresh lists
		switch(_offer.state) {
			case OfferPack.State.ACTIVE: {
				m_activeOffers.Add(_offer);
			} break;

			case OfferPack.State.EXPIRED: {
				m_offersToRemove.Add(_offer);
				m_activeOffers.Remove(_offer);
			} break;

			case OfferPack.State.PENDING_ACTIVATION: {
				m_activeOffers.Remove(_offer);
			} break;
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

    /// <summary>
    /// Get a offer pack defined in the definitions. It may not be enabled.
    /// </summary>
    /// <returns>The offer pack.</returns>
    /// <param name="_iapSku">Offer iap sku.</param>
    public static OfferPack GetOfferPack(string _iapSku) {
        int count = m_instance.m_allOffers.Count;
        for (int i = 0; i < count; i++) {
            OfferPack offerPack = m_instance.m_allOffers[i];
            if (offerPack.def.Get("iapSku") == _iapSku) {
                return offerPack;
            }
        }
        return null;
    }
    
	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}