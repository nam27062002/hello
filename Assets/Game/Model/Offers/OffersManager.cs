// OffersManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//#define LOG
//#define LOG_PACKS

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global manager for offer packs.
/// </summary>
public class OffersManager : Singleton<OffersManager> {
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

    private HappyHourOffer m_happyHour = null;
    public HappyHourOffer happyHour {
        get { return m_happyHour; }
    }

    // Internal
    private List<OfferPack> m_allEnabledOffers = new List<OfferPack>();	// All enabled and non-expired offer packs, regardless of type
	private List<OfferPack> m_allOffers = new List<OfferPack>();        // All defined offer packs, regardless of state and type
	private List<OfferPack> m_offersToRemove = new List<OfferPack>();   // Temporal list of offers to be removed from active lists
    private float m_timer = 0;

	// Rotational offers
	private List<OfferPack> m_allEnabledRotationalOffers = new List<OfferPack>();  // All enabled and non-expired rotational offer packs
	private List<OfferPack> m_activeRotationalOffers = new List<OfferPack>();   // Currently active rotational offers

	// Free offers
	private List<OfferPack> m_allEnabledFreeOffers = new List<OfferPack>();  // All enabled and non-expired free offer packs

	private OfferPackFree m_activeFreeOffer = null;   // Currently active free offer
	public static OfferPackFree activeFreeOffer {
		get { return instance.m_activeFreeOffer; }
	}

	public static DateTime freeOfferCooldownEndTime {
		get { return UsersManager.currentUser.freeOfferCooldownEndTime; }
	}

	public static TimeSpan freeOfferRemainingCooldown {
		get { return freeOfferCooldownEndTime - GameServerManager.SharedInstance.GetEstimatedServerTime(); }
	}

	public static bool isFreeOfferOnCooldown {
		get { return freeOfferRemainingCooldown.TotalSeconds > 0; }
	} 

	private bool m_freeOfferNeedsSorting = false;	// While on cooldown, the free offer will always be placed last. Keep a flag to re-calculate order once the cooldown has finished.

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

    public bool enabled;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// First update loop.
    /// </summary>
    protected override void OnCreateInstance() {
        m_timer = 0;
        enabled = true;
    }

	/// <summary>
	/// Update loop.
	/// </summary>
	public void Update() {
		// Refresh offers periodically for better performance
		// Only if allowed
		if(enabled && m_autoRefreshEnabled) {
			if(m_timer <= 0) {
				m_timer = settings != null ? settings.refreshFrequency : 1f;	// Crashlytics was reporting a Null reference, protect it just in case
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

		instance.m_allEnabledFreeOffers.Clear();
		instance.m_activeFreeOffer = null;

		// Get all known offer packs
		// Sort offers by their "order" field, so if two mutually exclusive offers (same uniqueId)
		// are triggered at the same time, we can control which one shows
		List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		DefinitionsManager.SharedInstance.SortByProperty(ref offerDefs, "order", DefinitionsManager.SortType.NUMERIC);

        bool needsCleaning = HDCustomizerManager.instance.hasBeenApplied;

        List<string> validCustomIds = new List<string>();
		// Create data for each known offer pack definition
		for(int i = 0; i < offerDefs.Count; ++i) {
			// Create and initialize new pack
			OfferPack newPack = OfferPack.CreateFromDefinition(offerDefs[i]);

            // Store new pack
            instance.m_allOffers.Add(newPack);

			// If enabled, store to the enabled collection
			if(offerDefs[i].GetAsBool("enabled", false)) {
                //lets check if player has all the bundles required to view this offer
                if (instance.IsOfferAvailable(newPack)) {
					Log("ADDING OFFER PACK {0}", Color.green, newPack.def.sku);
                    instance.m_allEnabledOffers.Add(newPack);

					// Additional treatment based on offer type
					switch(newPack.type) {
						case OfferPack.Type.ROTATIONAL: {
							instance.m_allEnabledRotationalOffers.Add(newPack);
						} break;

						case OfferPack.Type.FREE: {
							instance.m_allEnabledFreeOffers.Add(newPack);
						} break;
					}
                } else {
					Log("OFFER PACK {0} CAN'T BE ADDED!", Color.red, newPack.def.sku);
				}
			}

			// If pushed, check whether it needs to be cleaned
			if(newPack.type == OfferPack.Type.PUSHED) {
				// Only if there is a customization on experiment
				if(needsCleaning) {
					validCustomIds.Add(OffersManager.GenerateTrackingOfferName(offerDefs[i]));
				}
			}
		}

        // Create a new happy hour offer from the definitions
        if (instance.m_happyHour == null)
        {
            instance.m_happyHour = HappyHourOffer.CreateFromDefinition();
        }

        // Refresh active and featured offers
        instance.Refresh(true);

        // Only clean if we have a new customization/s
        if ( needsCleaning ){
            UsersManager.currentUser.CleanOldPushedOffers(validCustomIds);
        }

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
		Log("REFRESH {0}", Colors.magenta, _forceActiveRefresh);

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
				Log("PACK UPDATED: {0} | {1}", Colors.magenta, pack.def.sku, pack.state);

				// Update lists
				UpdateCollections(pack);

				// Update persistence with this pack's new state
				// [AOC] Packs Save() is smart, only stores packs when required
				UsersManager.currentUser.SaveOfferPack(pack);
			}
		}

		// Remove expired offers (they won't be active anymore, no need to update them)
		for(int i = 0; i < m_offersToRemove.Count; ++i) {
			Log("---> REMOVING ", Color.red, m_offersToRemove[i].def.sku);
			m_allEnabledOffers.Remove(m_offersToRemove[i]);
			if(m_offersToRemove[i].type == OfferPack.Type.ROTATIONAL) {
				m_allEnabledRotationalOffers.Remove(m_offersToRemove[i] as OfferPackRotational);
			}
		}
		m_offersToRemove.Clear();

		// Do we need to activate a new rotational offer?
		dirty |= RefreshRotationals();

		// Do we need to activate a new free offer?
		dirty |= RefreshFree();

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
		Queue<string> history = null;

		// Crashlytics was reporting a Null reference, protect it just in case
		int minRotationalActiveOffers = settings != null ? settings.rotationalActiveOffers : 1;

		// Store reference to persistence
		List<SimpleJSON.JSONClass> rotationalOffersPersistence = UsersManager.currentUser.newOfferPersistanceData[OfferPack.Type.ROTATIONAL];

		// Do we need to activate a new rotational pack?
		while( m_activeRotationalOffers.Count < minRotationalActiveOffers && loopCount < maxLoops) {
			// Some logging
			Log("Rotational offers required: {0} active", Colors.orange, m_activeRotationalOffers.Count);
#if LOG
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			int totalPurchases = trackingPersistence == null ? 0 : trackingPersistence.TotalPurchases;
			Log("Total Purchases {0}", Colors.silver, totalPurchases);
#endif

			// Make sure rotational history has the proper size
			UpdateRotationalHistory(null);

			// Create a local copy of the history to be able to manipulate it
			// Do it ONLY if we need new rotational packs - avoid unnecessary memory allocation
			if(history == null) {
				history = new Queue<string>();
			} else {
				history.Clear();
			}
			int max = rotationalOffersPersistence.Count;
			for(int i = 0; i < max; i++) {
				history.Enqueue(rotationalOffersPersistence[i]["sku"]);
			}

			// Select a new pack!
			loopCount++;
			OfferPackRotational newPack = PickRandomPack(
				m_allEnabledRotationalOffers,
				history,
				history.Count - m_activeRotationalOffers.Count	// Don't try with active packs!
			) as OfferPackRotational;

			// If no pack was found, break the loop
			if(newPack == null) break;

			// Activate new pack and add it to collections
			newPack.Activate();
			Log("ACTIVATING pack {0}", Color.green, newPack.def.sku);
			UpdateCollections(newPack);
			UpdateRotationalHistory(newPack);

			// Update persistence with this pack's new state
			// [AOC] Packs Save() is smart, only stores packs when required
			UsersManager.currentUser.SaveOfferPack(newPack);

			// Manager is dirty
			dirty = true;
		}

		// Done!
		return dirty;
	}

	/// <summary>
	/// Checks wehther a new free pack needs to be activated and does it. 
	/// </summary>
	/// <returns>Wheteher a new free pack has been activated or not.</returns>
	private bool RefreshFree() {
		// Nothing to do if a free offer is already active
		if(m_activeFreeOffer != null) return false;

		// Some logging
		Log("Free offer required", Colors.orange);

		// Store reference to persistence
		List<SimpleJSON.JSONClass> freeOffersPersistence = UsersManager.currentUser.newOfferPersistanceData[OfferPack.Type.FREE];

		// Make sure free history has the proper size
		UpdateFreeHistory(null);

		// Create a local copy of the history to be able to manipulate it
		// Do it ONLY if we need new rotational packs - avoid unnecessary memory allocation
		Queue<string> history = new Queue<string>();
		int max = freeOffersPersistence.Count;
		for(int i = 0; i < max; i++) {
			history.Enqueue(freeOffersPersistence[i]["sku"]);
		}

		// Select a new pack!
		OfferPackFree newPack = PickRandomPack(
			m_allEnabledFreeOffers,
			history,
			history.Count
		) as OfferPackFree;

		// If no pack was found, nothing else to do
		if(newPack == null) return false;

		// Activate new pack and add it to collections
		newPack.Activate();
		Log("ACTIVATING pack {0}", Color.green, newPack.def.sku);
		UpdateCollections(newPack);
		UpdateFreeHistory(newPack);

		// Update persistence with this pack's new state
		// [AOC] Packs Save() is smart, only stores packs when required
		UsersManager.currentUser.SaveOfferPack(newPack);

		// Done!
		return true;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	/// <param name="_newPack"></param>
    private bool IsOfferAvailable(OfferPack _newPack) {
        List<OfferPackItem> items = _newPack.items;

        bool returnValue = true;
        for (int i = 0; i < items.Count && returnValue; ++i) {
            OfferPackItem item = items[i];

            DefinitionNode def = null;            			
            if (item.type.Equals(Metagame.RewardPet.TYPE_CODE)) {
				returnValue = HDAddressablesManager.Instance.AreResourcesForPetAvailable(item.sku);
            } else if (item.type.Equals(Metagame.RewardSkin.TYPE_CODE)) {
                def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, item.sku);
				if(def != null) {
					returnValue = HDAddressablesManager.Instance.AreResourcesForDragonAvailable(def.Get("dragonSku"));
				}
            }            
        }

        return returnValue;
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
				switch(_offer.type) {
					case OfferPack.Type.ROTATIONAL: {
						m_activeRotationalOffers.Add(_offer as OfferPackRotational);
					} break;

					case OfferPack.Type.FREE: {
						m_activeFreeOffer = _offer as OfferPackFree;
					} break;
				}
			} break;

			case OfferPack.State.EXPIRED: {
				m_offersToRemove.Add(_offer);
				m_activeOffers.Remove(_offer);
				switch(_offer.type) {
					case OfferPack.Type.ROTATIONAL: {
						m_activeRotationalOffers.Remove(_offer as OfferPackRotational);
					} break;

					case OfferPack.Type.FREE: {
						if(m_activeFreeOffer == _offer) m_activeFreeOffer = null;
					} break;
				}
			} break;

			case OfferPack.State.PENDING_ACTIVATION: {
				m_activeOffers.Remove(_offer);
				switch(_offer.type) {
					case OfferPack.Type.ROTATIONAL: {
						m_activeRotationalOffers.Remove(_offer as OfferPackRotational);
					} break;

					case OfferPack.Type.FREE: {
						if(m_activeFreeOffer == _offer) m_activeFreeOffer = null;
					} break;
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
        List<SimpleJSON.JSONClass> history = UsersManager.currentUser.newOfferPersistanceData[OfferPack.Type.ROTATIONAL];

		// If a pack needs to be added, do it now
		if(_offer != null) {
            history.Add( _offer.Save() );
		}

		// Remove as many items as needed until the history size is right
		int maxSize = settings.rotationalHistorySize + m_activeRotationalOffers.Count; // History contains active offers
		Log("Checking history size: {0} vs {1} ({2} + {3})", history.Count, maxSize, settings.rotationalHistorySize, m_activeRotationalOffers.Count);
		while(history.Count > maxSize) {
			Log("    History too big: Dequeing");
            history.RemoveRange(maxSize, history.Count - maxSize);
		}

		// Debug
		Log("Rotational history updated with pack {0}", (_offer == null ? "NULL" : _offer.def.sku));
		LogHistory(OfferPack.Type.ROTATIONAL, ref history);
	}

	/// <summary>
	/// Add a pack to the top of the historic of free packs to prevent repetition. 
	/// </summary>
	/// <param name="_offer">Pack to be added.</param>
	private void UpdateFreeHistory(OfferPackFree _offer) {
		// Aux vars
		List<SimpleJSON.JSONClass> history = UsersManager.currentUser.newOfferPersistanceData[OfferPack.Type.FREE];

		// If a pack needs to be added, do it now
		if(_offer != null) {
			history.Add(_offer.Save());
		}

		// Remove as many items as needed until the history size is right
		int activeFreeOfferCount = m_activeFreeOffer != null ? 1 : 0;	// History contains active offers
		int maxSize = settings.rotationalHistorySize + activeFreeOfferCount;
		Log("Checking history size: {0} vs {1} ({2} + {3})", history.Count, maxSize, settings.freeHistorySize, activeFreeOfferCount);
		while(history.Count > maxSize) {
			Log("    History too big: Dequeing");
			history.RemoveRange(maxSize, history.Count - maxSize);
		}

		// Debug
		Log("Free history updated with pack {0}", (_offer == null ? "NULL" : _offer.def.sku));
		LogHistory(OfferPack.Type.FREE, ref history);
	}

	/// <summary>
	/// Pick a random, valid pack from the given pool.
	/// </summary>
	/// <param name="_initialPool">Pool of candidate packs.</param>
	/// <param name="_history">History of recently used packs, including any active pack (to avoid repetition).</param>
	/// <param name="_maxTries">Max number of attempts if no valid candidate is found. With each new attempt, the last entry in the history will be removed.</param>
	/// <returns>The selected pack. <c>null</c> if no pack could be selected.</returns>
	private OfferPack PickRandomPack(List<OfferPack> _initialPool, Queue<string> _history, int _maxTries) {
		// Aux vars
		OfferPack pack = null;
		int poolCount = _initialPool.Count;

		// Create pool from non-expired offers
		List<OfferPack> pool = new List<OfferPack>();
		for(int i = 0; i < poolCount; ++i) {
			// Aux
			pack = _initialPool[i];
			Log("    Checking {0}", pack.def.sku);

			// Check if it can be activated
			if(!pack.CanBeActivated()) continue;

			// Skip if pack has recently been used
			Log("        Checking history...");
			if(_history.Contains(pack.def.sku)) continue;

			// Everything ok, add to the candidates pool!
			Log("    Candidate is Good! {0}", Colors.lime, pack.def.sku);
			pool.Add(pack);
		}

		// If at least one selectable pack was found, pick a random one from the pool
		if(pool.Count > 0) {
			// Pick a random pack from the pool!
			pack = pool.GetRandomValue();
#if LOG
			string poolStr = "";
			foreach(OfferPack p in pool) poolStr += "    " + p.def.sku + "\n";
			Log("Picking random pack from a pool of {0}...\n{1}", pool.Count, poolStr);
#endif
		} else {
			// If no selectable pack was found, try reusing packs in the history
			Log("Random pool is empty, try loosen up the history ({0})", _history.Count);

			// Be more flexible with repeating recently used packs
			while(pack == null && _history.Count > 0 && _maxTries > 0) {
				// One less try
				--_maxTries;

				// Pick last pack on the history
				string packSku = _history.Dequeue();

				// Is the pack in the pool?
				for(int i = 0; i < poolCount; ++i) {
					if(_initialPool[i].def.sku == packSku) {
						pack = _initialPool[i];
						break;
					}
				}

				// Can the pack be activated?
				if(pack != null) {
					Log("    Checking {0}", packSku);
					if(!pack.CanBeActivated()) pack = null;
				} else {
					Log("    Couldn't find pack {0} (from history). Trying next pack.", packSku);
				}
			}

			// Report failure
			if(pack == null) {
				Log("Can't remove any more pack from the history, no chances of selecting a new pack :(\nBreaking loop", Color.red);
			}
		}

		// Return selected pack
		return pack;
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
	/// <param name="_packSku">Offer pack sku.</param>
	public static OfferPack GetOfferPack(string _packSku) {
		int count = instance.m_allOffers.Count;
		for(int i = 0; i < count; ++i) {
			OfferPack pack = instance.m_allOffers[i];
			if(pack.def.sku == _packSku) {
				return pack;
			}
		}
		return null;
	}

	/// <summary>
	/// Get a offer pack defined in the definitions by its IAP. It may not be enabled.
	/// </summary>
	/// <returns>The offer pack.</returns>
	/// <param name="_iapSku">Offer iap sku.</param>
	public static OfferPack GetOfferPackByIAP(string _iapSku) {
        int count = instance.m_allOffers.Count;
        for (int i = 0; i < count; i++) {
            OfferPack offerPack = instance.m_allOffers[i];
            if (offerPack.def.Get("iapSku") == _iapSku) {
                return offerPack;
            }
        }
        return null;
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="def"></param>
	/// <returns></returns>
    public static string GenerateTrackingOfferName(DefinitionNode def)
    {
        string offerName = def.sku;
        string experimentName = HDCustomizerManager.instance.GetExperimentNameForDef(def);
        if ( !string.IsNullOrEmpty( experimentName ) )
        {
            offerName += "." + experimentName;
        }
        else if ( def.customizationCode > 0 )
        {
            offerName += "." + def.customizationCode;
        }
        return offerName;
    }

	/// <summary>
	/// Restart the free offer cooldown timer.
	/// </summary>
	public static void RestartFreeOfferCooldown() {
		DefinitionNode offerSettings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "offerSettings");
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		UsersManager.currentUser.freeOfferCooldownEndTime = serverTime.AddMinutes(offerSettings.GetAsDouble("freeCooldownMinutes"));
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
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string _msg, params object[] _replacements) {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		ControlPanel.Log(string.Format(_msg, _replacements), ControlPanel.ELogChannel.Offers);
#endif
	}

    /// <summary>
    /// Log into the console (if enabled).
    /// </summary>
    /// <param name="_msg">Message to be logged. Can have replacements like string.Format method would have.</param>
    /// <param name="_color">Message color.</param>
    /// <param name="_replacements">Replacements, to be used as string.Format method.</param>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string _msg, Color _color, params object[] _replacements) {
#if LOG
		Log(_color.Tag(_msg), _replacements);
#endif
	}

    /// <summary>
    /// Log into the console (if enabled).
    /// Checks LOG_PACKS flag.
    /// </summary>
    /// <param name="_msg">Message to be logged. Can have replacements like string.Format method would have.</param>
    /// <param name="_color">Message color.</param>
    /// <param name="_replacements">Replacements, to be used as string.Format method.</param>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogPack(string _msg, Color _color, params object[] _replacements) {
#if LOG_PACKS
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		ControlPanel.Log(string.Format(_color.Tag(_msg), _replacements), ControlPanel.ELogChannel.Offers);
#endif
	}

    /// <summary>
    /// Do a report on current collections state.
    /// </summary>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void LogCollections() {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		string str = "Collections:\n";

		AppendCollection(ref str, m_allOffers, "All");
		AppendCollection(ref str, m_allEnabledOffers, "All Enabled");
		AppendCollection(ref str, m_activeOffers, "Active");

		str += "\n";
		AppendCollection(ref str, m_allEnabledRotationalOffers, "All Rotational Enabled");
		AppendCollection(ref str, m_activeRotationalOffers, "Active Rotational");

		str += "\n";
		AppendCollection(ref str, m_offersToRemove, "To Remove");

		Log(str);
#endif
	}

#if LOG
	/// <summary>
	/// Appends the given collection to the target log string.
	/// </summary>
	/// <param name="_str">String.</param>
	/// <param name="_collection">Collection.</param>
	/// <param name="_collectionName">Name of the collection.</param>
	private void AppendCollection<T>(ref string _str, List<T> _collection, string _collectionName) where T : OfferPack{
		_str += "\t" + _collectionName + ": " + _collection.Count + "\n";
		foreach(OfferPack pack in _collection) {
			_str += "\t\t" + pack.def.sku + "\n";
		}
	}
#endif

    /// <summary>
    /// Do a report on current rotational history state.
    /// </summary>
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void LogHistory(OfferPack.Type _type, ref List<SimpleJSON.JSONClass> _history) {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;

		string str = _type.ToString() + " History (" + _history.Count + "):";
		foreach(SimpleJSON.JSONClass s in _history) {
			str += "    " + s["sku"] + "\n";
		}
		Log(str);

		foreach(SimpleJSON.JSONClass s in _history) {
			Debug.Log("    " + s.ToString());
		}
#endif
	}

	/// <summary>
	/// For debug purposes only!
	/// Skips the cooldown timer of the free offer.
	/// </summary>
	public static void DEBUG_SkipFreeOfferCooldown() {
		UsersManager.currentUser.freeOfferCooldownEndTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
	}
}