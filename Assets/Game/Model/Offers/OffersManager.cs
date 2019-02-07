// OffersManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//#define LOG

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Linq;
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
	private List<OfferPack> m_activeOffers = new List<OfferPack>();		// All currently active offers (state ACTIVE), regardless of type
	public static List<OfferPack> activeOffers {
		get { return instance.m_activeOffers; }
	}

	private OfferPack m_featuredOffer = null;
	public static OfferPack featuredOffer {
		get { return instance.m_featuredOffer; }
	}

	private bool m_autoRefreshEnabled = true;
	public static bool autoRefreshEnabled {
		get { return instance.m_autoRefreshEnabled; }
		set { instance.m_autoRefreshEnabled = value; }
	}

	// Internal
	private List<OfferPack> m_allEnabledOffers = new List<OfferPack>();	// All enabled and non-expired offer packs, regardless of type
	private List<OfferPack> m_allOffers = new List<OfferPack>();        // All defined offer packs, regardless of state and type
	private List<OfferPackRotational> m_allEnabledRotationalOffers = new List<OfferPackRotational>();  // All enabled and non-expired rotational offer packs
	private List<OfferPackRotational> m_activeRotationalOffers = new List<OfferPackRotational>();	// Currently active rotational offers
	private List<OfferPack> m_offersToRemove = new List<OfferPack>();   // Temporal list of offers to be removed from active lists
    private float m_timer = 0;

	// Settings
	private OffersManagerSettings m_settings = null;
	public static OffersManagerSettings settings {
		get { 
			if(instance.m_settings == null) {
				instance.m_settings = Resources.Load<OffersManagerSettings>(OffersManagerSettings.PATH);
			}
			return instance.m_settings;
		}
	}

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
		// Only if allowed
		if(m_autoRefreshEnabled) {
			if(m_timer <= 0) {
				m_timer = settings.refreshFrequency;
				Refresh(false);
			}
			m_timer -= Time.deltaTime;
		}
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
		instance.m_allEnabledRotationalOffers.Clear();
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
				instance.m_allEnabledRotationalOffers.Add(newPack as OfferPackRotational);
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

		// Iterate all offer packs looking for state changes
		for(int i = 0; i < m_allEnabledOffers.Count; ++i) {
			// Aux vars
			pack = m_allEnabledOffers[i];

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
		}

		// Remove expired offers (they won't be active anymore, no need to update them)
		for(int i = 0; i < m_offersToRemove.Count; ++i) {
			m_allEnabledOffers.Remove(m_offersToRemove[i]);
			if(m_offersToRemove[i].type == OfferPack.Type.ROTATIONAL) {
				m_allEnabledRotationalOffers.Remove(m_offersToRemove[i] as OfferPackRotational);
			}
		}
		m_offersToRemove.Clear();

		// Do we need to activate a new rotational offer?
		RefreshRotationals();

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
	/// Checks wehther a new rotational pack needs to be activated and does it. 
	/// </summary>
	/// <returns>Wheteher a new rotational pack has been activated or not.</returns>
	private bool RefreshRotationals() {
		// Aux vars
		int loopCount = 0;
		int maxLoops = 50;  // Just in case, prevent infinite loop
		bool dirty = false;
		Queue<string> history = UsersManager.currentUser.offerPacksRotationalHistory;

		// Do we need to activate a new rotational pack?
		while(m_activeRotationalOffers.Count < settings.rotationalActiveOffers && loopCount < maxLoops) {
			Log(Colors.orange.Tag("Rotational offers required: {0} active"), m_activeRotationalOffers.Count);
			UpdateRotationalHistory(null);	// Make sure rotational history has the proper size
			LogRotationalHistory();
#if LOG
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			int totalPurchases = trackingPersistence == null ? 0 : trackingPersistence.TotalPurchases;
			Log(Colors.silver.Tag("Total Purchases {0}"), totalPurchases);
#endif

			// Select a new pack!
			OfferPackRotational rotationalPack = null;
			loopCount++;

			// Create pool from non-expired rotational offers
			List<OfferPackRotational> pool = new List<OfferPackRotational>();
			for(int i = 0; i < m_allEnabledRotationalOffers.Count; ++i) {
				// Aux
				rotationalPack = m_allEnabledRotationalOffers[i];
				Log("    Checking {0}", rotationalPack.def.sku);

				// Skip active and expired packs
				Log("        Checking state... {0}", rotationalPack.state);
				if(rotationalPack.state != OfferPack.State.PENDING_ACTIVATION) continue;

				// Skip if pack has recently been used
				Log("        Checking history...");
				if(history.Contains(rotationalPack.def.sku)) continue;

				// Skip if segmentation conditions are not met for this pack
				Log("        Checking segmentation...");
				if(!rotationalPack.CheckSegmentation()) continue;

				// Skip if activation conditions are not met for this pack
				Log("        Checking activation...");
				if(!rotationalPack.CheckActivation()) continue;

				// Also skip if for some reason the pack has expired!
				// [AOC] TODO!! Should it be removed?
				Log("        Checking expiration...");
				if(rotationalPack.CheckExpiration(false)) continue;

				// Everything ok, add to the candidates pool!
				Log(Colors.lime.Tag("    Candidate is Good! {0}"), rotationalPack.def.sku);
				pool.Add(rotationalPack);
			}

			// If no selectable pack was found, stop looping
			if(pool.Count == 0) {
				Log("Random pool is empty, try loosen up the history ({0})", history.Count);

				// Be more flexible with repeating recently used packs
				if(history.Count > m_activeRotationalOffers.Count) {    // Don't dequeue active packs
					history.Dequeue();
					continue;   // Try again!
				} else {
					Log("Can't remove any pack from the history, no chances of selecting a new pack :(\nBreaking loop");
					break;  // No chances of getting any pack :(
				}
			}

			// Pick a random pack from the pool!
			rotationalPack = pool.GetRandomValue();
#if LOG
			string poolStr = "";
			foreach(OfferPack p in pool) poolStr += "    " + p.def.sku + "\n";
			Log("Picking random pack from a pool of {0}...\n{1}", pool.Count, poolStr);
#endif

			// Activate new pack and add it to collections
			rotationalPack.Activate();
			Log("Activating pack {0}", rotationalPack.def.sku);
			UpdateCollections(rotationalPack);
			UpdateRotationalHistory(rotationalPack);

			// Update persistence with this pack's new state
			// [AOC] Packs Save() is smart, only stores packs when required
			UsersManager.currentUser.SaveOfferPack(rotationalPack);

			// Manager is dirty
			dirty = true;
		}

		// Done!
		return dirty;
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
				if(_offer.type == OfferPack.Type.ROTATIONAL) {
					m_activeRotationalOffers.Add(_offer as OfferPackRotational);
				}
			} break;

			case OfferPack.State.EXPIRED: {
				m_offersToRemove.Add(_offer);
				m_activeOffers.Remove(_offer);
				if(_offer.type == OfferPack.Type.ROTATIONAL) {
					m_activeRotationalOffers.Remove(_offer as OfferPackRotational);
				}
			} break;

			case OfferPack.State.PENDING_ACTIVATION: {
				m_activeOffers.Remove(_offer);
				if(_offer.type == OfferPack.Type.ROTATIONAL) {
					m_activeRotationalOffers.Remove(_offer as OfferPackRotational);
				}
			} break;
		}

		// Debug
		Log("Collections updated with pack {0}", _offer.def.sku);
		LogCollections();
	}

	/// <summary>
	/// Add a pack to the top of the historic of rotational packs to prevent repetition. 
	/// </summary>
	/// <param name="_offer">Pack to be added.</param>
	private void UpdateRotationalHistory(OfferPackRotational _offer) {
		// Aux vars
		Queue<string> history = UsersManager.currentUser.offerPacksRotationalHistory;

		// If a pack needs to be added, do it now
		if(_offer != null) {
			history.Enqueue(_offer.def.sku);
		}

		// Remove as many items as needed until the history size is right
		int maxSize = settings.rotationalHistorySize + m_activeRotationalOffers.Count; // History contains active offers
		Log("Checking history size: {0} vs {1} ({2} + {3})", history.Count, maxSize, settings.rotationalHistorySize, m_activeRotationalOffers.Count);
		while(history.Count > maxSize) {
			Log("    History too big: Dequeing");
			history.Dequeue();
		}

		// Debug
		Log("Rotational history updated with pack {0}", (_offer == null ? "NULL" : _offer.def.sku));
		LogRotationalHistory();
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

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Log into the console (if enabled).
	/// </summary>
	/// <param name="_msg">Message to be logged. Can have replacements like string.Format method would have.</param>
	/// <param name="_replacements">Replacements, to be used as string.Format method.</param>
	private static void Log(string _msg, params object[] _replacements) {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		ControlPanel.Log(string.Format(_msg, _replacements), ControlPanel.ELogChannel.Offers);
#endif
	}

	/// <summary>
	/// Do a report on current collections state.
	/// </summary>
	private void LogCollections() {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		Log(
			"Collections:\n" +
			"    All: " + m_activeOffers.Count + "\n" +
			"    All Enabled: " + m_activeOffers.Count + "\n" +
			"    Active: " + m_activeOffers.Count + "\n" +
			"\n" +
			"    All Rotational Enabled: " + m_activeOffers.Count + "\n" +
			"    Active Rotational: " + m_activeOffers.Count + "\n" +
			"\n" +
			"    To Remove: " + m_offersToRemove.Count
		);
#endif
	}

	/// <summary>
	/// Do a report on current rotational history state.
	/// </summary>
	private void LogRotationalHistory() {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		Queue<string> history = UsersManager.currentUser.offerPacksRotationalHistory;
		string str = "Rotational History (" + history.Count + "):\n";
		foreach(string s in history) {
			str += "    " + s + "\n";
		}
		Log(str);
#endif
	}
}