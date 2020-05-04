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
	private const string FREE_FTUX_PACK_SKU = "AdOfferFtux";

    // Debug
#if LOG_PACKS
	private const string LOG_PACK_SKU = "rotationalHigh";
#endif

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

	private HappyHourManager m_happyHourManager = new HappyHourManager();
	public static HappyHourManager happyHourManager {
		get { return instance.m_happyHourManager; }
	}

    private OfferPack m_removeAdsOffer = null;
    public OfferPack removeAdsOffer
    {
        get { return removeAdsOffer; }
    }

    private OfferPack m_activeRemoveAdsOffer = null;

    // List of all the active categories
    private List<ShopCategory> m_categories = new List<ShopCategory>();
    public List<ShopCategory> activeCategories
    {
        get { return m_categories; }
    }

    // Internal
	private Dictionary<string, OfferPack> m_allOffersBySku = new Dictionary<string, OfferPack>(); // All defined offer packs, regardless of state and type
    private List<OfferPack> m_allEnabledOffers = new List<OfferPack>();	// All enabled and non-expired offer packs, regardless of type
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
				instance.m_settings = new OffersManagerSettings();
				instance.m_settings.InitFromDefinitions();
			}
			return instance.m_settings;
		}
	}

	// Cache
	private string m_clusterId;

	private bool m_initialized = false;
	private bool m_enabled = true;
	public bool enabled {
		get { return m_enabled; }
		set { m_enabled = value; }
	}

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// First update loop.
    /// </summary>
    protected override void OnCreateInstance() {
        m_timer = 0;
        enabled = true;
		m_initialized = false;
    }

	/// <summary>
	/// Update loop.
	/// </summary>
	public void Update() {
		// Refresh offers periodically for better performance
		// Only if allowed
		if(m_initialized && enabled && m_autoRefreshEnabled) {
			if(m_timer <= 0) {
				m_timer = settings != null ? settings.refreshFrequency : 1f;	// Crashlytics was reporting a Null reference, protect it just in case
				Refresh(false);

                // [JOM] Update the cooldowns in Remove Ads controller 
                // Technically not an offer, but didnt find any better place for this
				UsersManager.currentUser.removeAds.Update();

				// Refresh happy hour manager as well!
				m_happyHourManager.Update();
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

		// Reload settings
		settings.InitFromDefinitions();

        // Clear current offers cache
        instance.m_allOffersBySku.Clear();
		instance.m_allEnabledOffers.Clear();
		instance.m_activeOffers.Clear();
		instance.m_offersToRemove.Clear();

		instance.m_allEnabledRotationalOffers.Clear();
		instance.m_activeRotationalOffers.Clear();

        instance.m_featuredOffer = null;

		instance.m_allEnabledFreeOffers.Clear();
		instance.m_activeFreeOffer = null;

        instance.m_categories.Clear();

		// Cache player cluster
		instance.m_clusterId = ClusteringManager.Instance.GetClusterId();

		// Get all the shop categories
		List<DefinitionNode> categoriesDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SHOP_CATEGORIES);
        DefinitionsManager.SharedInstance.SortByProperty(ref categoriesDefs, "order", DefinitionsManager.SortType.NUMERIC);

        // Iterate all the categories definitions and initialize them
        for (int i=0; i<categoriesDefs.Count; i++)
        {
            ShopCategory newCategory = ShopCategory.CreateFromDefinition(categoriesDefs[i]);

            instance.m_categories.Add(newCategory);
        }

        // Gather all the shop packs definitions for the currency packs
        // Add them to activeOffers, as they will be always active
        List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", OfferPack.TypeToString(OfferPack.Type.SC));
        defs.AddRange(DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", OfferPack.TypeToString(OfferPack.Type.HC)));
        foreach(DefinitionNode currencyDef in defs) {
			// Skip if not enabled by content
			if(!currencyDef.GetAsBool("enabled")) continue;

            OfferPack newPack = OfferPack.CreateFromDefinition(currencyDef);
            if(newPack != null && newPack.isActive) {
                instance.m_activeOffers.Add(newPack);
            }
        }

        // Get all known offer packs
        // Sort offers by their "order" field, so if two mutually exclusive offers (same uniqueId)
        // are triggered at the same time, we can control which one shows
        List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		DefinitionsManager.SharedInstance.SortByProperty(ref offerDefs, "order", DefinitionsManager.SortType.NUMERIC);

		// Use the loop to clean old offers from persistence so we don't end up having a huge user profile
        bool cleaningPushed = HDCustomizerManager.instance.hasBeenApplied;
		List<string> validPushedCustomIds = new List<string>();
		Dictionary<OfferPack.Type, List<string>> toClean = new Dictionary<OfferPack.Type, List<string>> {
			{ OfferPack.Type.ROTATIONAL, new List<string>() },
			{ OfferPack.Type.FREE, new List<string>() }
		};

		// Create data for each known offer pack definition
		for(int i = 0; i < offerDefs.Count; ++i) {

			// Check clustering
			string offerCluster = offerDefs[i].GetAsString("cluster");

            // Is this offer targeted for a specific cluster?
			if (!String.IsNullOrEmpty(offerCluster) && offerCluster != "-")
            {
                if ( instance.m_clusterId != offerCluster)
                {
					// The player is in a different cluster, so discard it
					continue ;
                }

			}

			// Create and initialize new pack
			OfferPack newPack = OfferPack.CreateFromDefinition(offerDefs[i]);

			// Load persisted data
			UsersManager.currentUser.LoadOfferPack(newPack);

			// Store new pack
			instance.m_allOffersBySku.Add(newPack.def.sku, newPack);

			// If pack is expired, discard it
			// [AOC] Let's minimize risks by keeping it in the all offers list
			if(newPack.state == OfferPack.State.EXPIRED) {
				Log("InitFromDefinitions: OFFER PACK {0} EXPIRED! Won't be added", Color.red, newPack.def.sku);
			}
			// If enabled, store to the enabled collection
			else if(offerDefs[i].GetAsBool("enabled", false)) {
                //lets check if player has all the bundles required to view this offer
                if (instance.IsOfferAvailable(newPack)) {
					Log("InitFromDefinitions: ADDING OFFER PACK {0} ({1})", DEBUG_GetColorByState(newPack), newPack.def.sku, newPack.state);
					instance.m_allEnabledOffers.Add(newPack);

					// Additional treatment based on offer type
					switch(newPack.type) {
						case OfferPack.Type.ROTATIONAL: {
							instance.m_allEnabledRotationalOffers.Add(newPack);
						} break;

						case OfferPack.Type.FREE: {
							instance.m_allEnabledFreeOffers.Add(newPack);

							// If active, store it as the current free offer
							// [AOC] Shouldn't be needed since the Refresh(true) below should do the trick
							if(newPack.state == OfferPack.State.ACTIVE) {
								// If another free offer was active first, something went wrong! Change this one to PENDING_ACTIVATION.
								// [AOC] Shouldn't happen, but we have seen some cases, so protect it just in case
								if(instance.m_activeFreeOffer != null) {
									instance.m_activeFreeOffer.ForceStateChange(OfferPack.State.PENDING_ACTIVATION, false);
								} else {
									instance.m_activeFreeOffer = newPack as OfferPackFree;
								}
							}
						} break;

                        case OfferPack.Type.REMOVE_ADS:
                        {
                            instance.m_removeAdsOffer = newPack;
                        }
                        break;

                    }
                } else {
					Log("InitFromDefinitions: OFFER PACK {0} CAN'T BE ADDED!", Color.red, newPack.def.sku);
				}
			}

			// Check whether it needs to be cleaned
			// Pushed offers work a bit different here
			if(newPack.type == OfferPack.Type.PUSHED) {
				// Only if there is a customization on experiment
				if(cleaningPushed) {
					validPushedCustomIds.Add(OffersManager.GenerateTrackingOfferName(offerDefs[i]));
				}
			} else if(toClean.ContainsKey(newPack.type)) {
				// Clean it if the pack is pending activation and has no purchases
				if(newPack.state == OfferPack.State.PENDING_ACTIVATION && newPack.purchaseCount == 0) {
					toClean[newPack.type].Add(newPack.def.sku);
				}
			}
		}

        // Clean offers persistence if needed
        if ( cleaningPushed ){
            UsersManager.currentUser.CleanOldPushedOffers(validPushedCustomIds);
        }
		foreach(KeyValuePair<OfferPack.Type, List<string>> kvp in toClean) {
			if(kvp.Value.Count > 0) {
				UsersManager.currentUser.CleanOldOffers(kvp.Key, kvp.Value);
			}
		}

		// Make sure to check whether free offer is on cooldown or not and put in the right place
		instance.m_freeOfferNeedsSorting = true;

		// Reload Happy Hour
		instance.m_happyHourManager.InitFromDefinitions();
		instance.m_happyHourManager.Load();

		// Refresh active and featured offers
		Log("InitFromDefinitions: INITIAL REFRESH -------------------------", Colors.magenta);
		instance.Refresh(true);

		// Done!
		instance.m_initialized = true;

		// Notify game
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
		Log("-------------------------------------------------------------------", Colors.magenta);
		Log("OFFERS MANAGER REFRESH {0}", Colors.magenta, _forceActiveRefresh);
		Log("-------------------------------------------------------------------", Colors.magenta);

		// Aux vars
		OfferPack pack = null;
		bool dirty = false;
		m_offersToRemove.Clear();

        List<OfferPack> offersChanged = new List<OfferPack>();

		// Iterate all offer packs looking for state changes
		for(int i = 0; i < m_allEnabledOffers.Count; ++i) {
			// Aux vars
			pack = m_allEnabledOffers[i];

			// Has offer changed state?
			if(pack.UpdateState() || _forceActiveRefresh) {
				// Yes!
				dirty = true;
				Log("Refresh: PACK UPDATED: {0} | {1}", DEBUG_GetColorByState(pack), pack.def.sku, pack.state);

				// Update lists
				UpdateCollections(pack);

				// Update persistence with this pack's new state
				// [AOC] Packs Save() is smart, only stores packs when required
				UsersManager.currentUser.SaveOfferPack(pack);

                offersChanged.Add(pack);
			}
		}

		// Remove expired offers (they won't be active anymore, no need to update them)
		for(int i = 0; i < m_offersToRemove.Count; ++i) {
			Log("Refresh: ---> REMOVING {0}", Color.red, m_offersToRemove[i].def.sku);
			m_allEnabledOffers.Remove(m_offersToRemove[i]);
			if(m_offersToRemove[i].type == OfferPack.Type.ROTATIONAL) {
				m_allEnabledRotationalOffers.Remove(m_offersToRemove[i] as OfferPackRotational);
			}

		}

        offersChanged.AddRange(m_offersToRemove);

		m_offersToRemove.Clear();

		// Do we need to activate a new rotational offer?
		dirty |= RefreshRotationals();

		// Do we need to activate a new free offer?
		dirty |= RefreshFree();

        // Do we need to activate the remove Ads offer?
        dirty |= RefreshRemoveAds();

        // Has any offer changed its state?
        if (dirty) {
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
			Messenger.Broadcast(MessengerEvents.OFFERS_CHANGED, offersChanged);
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

		// Crashlytics was reporting a Null reference, protect it just in case
		int minRotationalActiveOffers = settings != null ? settings.rotationalActiveOffers : 1;

		// Store reference to persistence
		Queue<string> rotationalsHistory = UsersManager.currentUser.offersHistory[OfferPack.Type.ROTATIONAL];

		// Do we need to activate a new rotational pack?
		while( m_activeRotationalOffers.Count < minRotationalActiveOffers && loopCount < maxLoops) {
			// Some logging
			Log("RefreshRotationals: Rotational offers required: {0} active", Colors.orange, m_activeRotationalOffers.Count);

			// Remove excess packs from the history
			UpdateRotationalHistory(null);

			// Select a new pack!
			loopCount++;
			OfferPackRotational newPack = PickRandomPack(
				m_allEnabledRotationalOffers,
				rotationalsHistory,
				rotationalsHistory.Count - m_activeRotationalOffers.Count	// Currently active packs are also in the history - don't remove them!!
			) as OfferPackRotational;

			// If no pack was found, break the loop
			if(newPack == null) break;

			// Activate new pack and add it to collections
			newPack.Activate();
			Log("RefreshRotationals: ACTIVATING pack {0}", Color.green, newPack.def.sku);
			UpdateCollections(newPack);
			UpdateRotationalHistory(newPack);

			// Update persistence with this pack's new state
			// [AOC] UserProfile.SaveOfferPack() is smart, only stores packs when required
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
		// If a free offer is already active, check whether the free offer needs re-sorting
		if(m_activeFreeOffer != null) {
			// If sorting is pending and cooldown has ended, re-sort!
			if(m_freeOfferNeedsSorting && !isFreeOfferOnCooldown) {
				m_freeOfferNeedsSorting = false;
				return true;
			}

			// Nothing to do if a free offer is already active and no re-sorting is needed
			return false;
		}

		// Some logging
		Log("RefreshFree: Free offer required", Colors.orange);

		// Remove excess packs from the history
		UpdateFreeHistory(null);
		Queue<string> freeHistory = UsersManager.currentUser.offersHistory[OfferPack.Type.FREE];

		// Select a new pack!
		// Special case for FTUX
		OfferPackFree newPack = null;
		if(freeHistory.Count == 0) {
			// Pick a specific pack
			newPack = GetOfferPack(FREE_FTUX_PACK_SKU) as OfferPackFree;

			// Make sure it can be activated!
			if(newPack != null) {
				if(!newPack.CanBeActivated()) newPack = null;
			}
		}

		// If it's not the FTUX or for some reason the FTUX pack doesn't exist, pick a random one
		if(newPack == null) {
			// Pick a random pack
			newPack = PickRandomPack(
				m_allEnabledFreeOffers,
				freeHistory,
				freeHistory.Count - 1   // Currently active pack is also in the history - don't remove it!!
			) as OfferPackFree;
		}

		// If no pack was found, nothing else to do
		if(newPack == null) return false;

		// Activate new pack and add it to collections
		Log("RefreshFree: ACTIVATING pack {0}", Color.green, newPack.def.sku);
		newPack.Activate();
		m_activeFreeOffer = newPack;
		UpdateCollections(newPack);
		UpdateFreeHistory(newPack);

		// Update persistence with this pack's new state
		// [AOC] UserProfile.SaveOfferPack() is smart, only stores packs when required
		Log("RefreshFree: ATTEMPTING TO SAVE FREE OFFER {0} -> {1}", Colors.blue, newPack.def.sku, newPack.ShouldBePersisted());
		UsersManager.currentUser.SaveOfferPack(newPack);

		// Done!
		return true;
	}

    /// <summary>
    /// Activates the remove ads offer
    /// </summary>
    private bool RefreshRemoveAds()
    {
        if (m_removeAdsOffer != null)
        {
            // Check if the offer is already accquired
            bool stateChanged = m_removeAdsOffer.UpdateState();

            // If active, add the offer to activeOffers collection
            if (m_activeRemoveAdsOffer == null)
            {
                UpdateCollections(m_removeAdsOffer);
                m_activeRemoveAdsOffer = m_removeAdsOffer;
            }

            // If state has changed, update the panel
            return stateChanged;
        }

        return false;
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
		Log("UpdateCollections: Collections updated with pack {0}", _offer.def.sku);
		LogCollections();
	}

	/// <summary>
	/// Add a pack to the top of the historic of rotational packs to prevent repetition. 
	/// </summary>
	/// <param name="_offer">Pack to be added.</param>
	private void UpdateRotationalHistory(OfferPackRotational _offer) {
        // Aux vars
        Queue<string> history = UsersManager.currentUser.offersHistory[OfferPack.Type.ROTATIONAL];

		// If a pack needs to be added, do it now
		if(_offer != null) {
            history.Enqueue(_offer.def.sku);
		}

		// Remove as many items as needed until the history size is right
		int maxSize = settings.rotationalHistorySize + m_activeRotationalOffers.Count; // History contains active offers
		CleanupHistory(ref history, maxSize);
		Log("UpdateRotationalHistory: Checking history size: {0} vs {1} ({2} + {3})", history.Count, maxSize, settings.rotationalHistorySize, m_activeRotationalOffers.Count);
		
		// Debug
		Log("UpdateRotationalHistory: Rotational history updated with pack {0}", (_offer == null ? "NULL" : _offer.def.sku));
		LogHistory(OfferPack.Type.ROTATIONAL, ref history);
	}

	/// <summary>
	/// Add a pack to the top of the historic of free packs to prevent repetition. 
	/// </summary>
	/// <param name="_offer">Pack to be added.</param>
	private void UpdateFreeHistory(OfferPackFree _offer) {
		// Aux vars
		Queue<string> history = UsersManager.currentUser.offersHistory[OfferPack.Type.FREE];

		// If a pack needs to be added, do it now
		if(_offer != null) {
			history.Enqueue(_offer.def.sku);
		}

		// Remove as many items as needed until the history size is right
		int activeFreeOfferCount = m_activeFreeOffer != null ? 1 : 0;	// History contains active offers
		int maxSize = settings.freeHistorySize + activeFreeOfferCount;
		CleanupHistory(ref history, maxSize);
		
		// Debug
		Log("UpdateFreeHistory: Free history updated with pack {0}", (_offer == null ? "NULL" : _offer.def.sku));
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

		// Some logging
#if LOG
		string logStr = "";
		foreach(OfferPack p in _initialPool) logStr += "    " + p.def.sku + "\n";
		Log("<b>PickRandomPack: Creating pool from non-expired offers...</b>\nInitial Pool: {0}", logStr);
#endif

		// Create pool from non-expired offers
		List<OfferPack> pool = new List<OfferPack>();
		for(int i = 0; i < poolCount; ++i) {
			// Aux
			pack = _initialPool[i];
			if(pack == null) continue;  // Just in case
			Log("  PickRandomPack: {0} Checking...", pack.def.sku);

			// Check if it can be activated
			if(!pack.CanBeActivated()) {
				Log("  PickRandomPack: {0} Activation checks failed! Try next pack", Colors.coral, pack.def.sku);
				continue;
			}

			// Skip if pack has recently been used
			if(_history.Contains(pack.def.sku)) {
				Log("  PickRandomPack: {0} Already in the history. Try next pack", Colors.coral, pack.def.sku);
				continue;
			}

			// Everything ok, add to the candidates pool!
			Log("  PickRandomPack: {0} Candidate is Good!", Colors.paleGreen, pack.def.sku);
			pool.Add(pack);
		}

		// If at least one selectable pack was found, pick a random one from the pool
		if(pool.Count > 0) {
			// Pick a random pack from the pool!
			pack = pool.GetRandomValue();
#if LOG
			logStr = "";
			foreach(OfferPack p in pool) logStr += "    " + p.def.sku + "\n";
			Log("<b>PickRandomPack: Picking random pack from a pool of {0}...</b>\n{1}", pool.Count, logStr);
#endif
		} else {
			// If no selectable pack was found, try reusing packs in the history
#if LOG
			logStr = "";
			foreach(string str in _history) logStr += "    " + str + "\n";
			Log("PickRandomPack: Random pool is empty, try loosen up the history\nhistorySize: {0}, maxTries: {1}\n{2}", _history.Count, _maxTries, logStr);
#endif

			// Be more flexible with repeating recently used packs
			pack = null;
			int maxDequeueTries = _history.Count;	// Prevent infinite loop in case all packs are active
			while(pack == null && _history.Count > 0 && _maxTries > 0 && maxDequeueTries > 0) {
				// Some logging
				Log("  PickRandomPack: {0} remaining tries", _maxTries);

				// Pick last pack on the history
				string packSku = _history.Peek();
				Log("  PickRandomPack: {0} Picked from history, check whether it's a valid pack", packSku);
				if(!DequeueIfNotActive(ref _history)) {
					// Pack is still active and has been moved to the end of the queue
					// Check next pack in the queue
					// Doesn't count as a normal try!
					--maxDequeueTries;
					if(maxDequeueTries <= 0) {
						Log("PickRandomPack: FAIL! Can't remove any pack from the history, all packs max tries reached! ({0})", Colors.coral, _maxTries);
						break;	// No need to keep looping
					}
					continue;
				}

				// One less try
				--_maxTries;

				// Is the pack in the pool?
				for(int i = 0; i < poolCount; ++i) {
					if(_initialPool[i].def.sku == packSku) {
						pack = _initialPool[i];
						break;
					}
				}

				// Can the pack be activated?
				if(pack != null) {
					Log("  PickRandomPack {0}: Checking activation...", Colors.paleYellow, packSku);
					if(!pack.CanBeActivated()) {
						Log("  PickRandomPack {0}: Can't be activated, try with next pack in the history.", Colors.coral, packSku);
						pack = null;
					} else {
						Log("  PickRandomPack {0}: ALL CHECKS PASSED!", Colors.paleGreen, packSku);
					}
				} else {
					Log("  PickRandomPack {0}: Pack (from history) is not in the initial pool. Trying next pack.", Colors.coral, packSku);
				}
			}

			// Report failure
			if(pack == null) {
				if(_maxTries <= 0) {
					Log("PickRandomPack: FAIL! Can't remove any more packs from the history, max tries reached! ({0})", Colors.coral, _maxTries);
				} else if(_history.Count <= 0) {
					Log("PickRandomPack: FAIL! Can't remove any more packs from the history, history empty", Colors.coral);
				} else {
					Log("PickRandomPack: FAIL! Unknown error", Colors.coral);
				}
				Log("PickRandomPack: FAIL! No chances of selecting a new pack :(\nBreaking loop", Color.red);
			}
		}

		// Return selected pack
		return pack;
	}

	/// <summary>
	/// Clean up a history
	/// </summary>
	/// <param name="_history"></param>
	/// <param name="_maxSize"></param>
	private static void CleanupHistory(ref Queue<string> _history, int _maxSize) {
#if LOG
		if(_history.Count > _maxSize) {
			Log("    CleanupHistory: History too big ({0} > {1})! Dequeing packs...", Colors.red, _history.Count, _maxSize);
		} else {
			Log("    CleanupHistory: History size is OK! ({0} <= {1}) - Doing nothing.", Colors.lime, _history.Count, _maxSize);
		}
#endif

		int tryCount = _history.Count;   // Prevent infinite loop in case none of the offers can be dequeued!
		while(_history.Count > _maxSize) {
			// Dequeue the first offer in the history
			string dequeuedSku = _history.Dequeue();
			OfferPack pack = GetOfferPack(dequeuedSku);

			// If we still have remaining tries, check whehter the offer is active
			if(tryCount > 0) {
				if(pack != null && pack.isActive) { // [AOC] Pack could be null if it had a sku that is no longer in the content
					// Offer is still active, enqueue it again!
					_history.Enqueue(dequeuedSku);
					Log("    CleanupHistory: Dequeued pack ({0}) was still active! Enqueued again at the end of the history.", Colors.magenta, dequeuedSku);
				} else {
					Log("    CleanupHistory: Pack ({0}) dequeued!", Colors.magenta, dequeuedSku);
				}
				--tryCount;
			} else {
				// Force dequeueing regardless of pack state
				// If pack is active, disable it properly
				if(pack != null && pack.isActive) {
					pack.ForceStateChange(OfferPack.State.PENDING_ACTIVATION, false);	// Allow it to be activated again if all conditions are met
					Log("    CleanupHistory: Pack ({0}) was still active, but dequeue forced!", Colors.magenta, dequeuedSku);
				}
			}
		}
	}

	/// <summary>
	/// Dequeue one item from the given queue, provided the Offer Pack it represents is not active.
	/// If it's active, it will move it to the end of the queue.
	/// </summary>
	/// <param name="_q">Queue to be treated. Typically a offer packs skus history.</param>
	/// <returns>Whether the first item in the queue could be dequeued (true) or not (false, in which case it has been moved to the end of the queue).</returns>
	private static bool DequeueIfNotActive(ref Queue<string> _q) {
		// Make sure we don't dequeue any active offer!
		string dequeuedSku = _q.Dequeue();
		OfferPack pack = GetOfferPack(dequeuedSku);
		if(pack != null && pack.isActive) {	// [AOC] Pack could be null if it had a sku that is no longer in the content
			// Enqueue it again
			_q.Enqueue(dequeuedSku);
			Log("    DequeueIfNotActive: Dequeued pack ({0}) was still active! Enqueue it again.", Colors.magenta, dequeuedSku);
			return false;
		}
		return true;
	}

	/// <summary>
	/// Function used to sort offer packs.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_p1">First pack to compare.</param>
	/// <param name="_p2">Second pack to compare.</param>
	private static int OfferPackComparer(OfferPack _p1, OfferPack _p2) {
		// Sort by order first
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
		OfferPack pack = null;
		instance.m_allOffersBySku.TryGetValue(_packSku, out pack);
		return pack;
	}

	/// <summary>
	/// Get a offer pack defined in the definitions by its IAP. It may not be enabled.
	/// </summary>
	/// <returns>The offer pack.</returns>
	/// <param name="_iapSku">Offer iap sku.</param>
	public static OfferPack GetOfferPackByIAP(string _iapSku) {
		// No pretty way to do it
		// We could cache another dictionary of all packs by iap, but not sure it would be a better solution
		foreach(KeyValuePair<string, OfferPack> kvp in instance.m_allOffersBySku) {
			if(kvp.Value.def.Get("iapSku") == _iapSku) {
				return kvp.Value;
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
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		UsersManager.currentUser.freeOfferCooldownEndTime = serverTime.AddMinutes(settings.freeCooldownMinutes);
		instance.m_freeOfferNeedsSorting = true;
	}

    /// <summary>
    /// Returns a list of active offers that belong to a shop category
    /// </summary>
    /// <param name="_category">Shop category</param>
    /// <returns>List of offers sorted by order</returns>
    public static List<OfferPack> GetOfferPacksByCategory (ShopCategory _category)
    {
        List<OfferPack> result = instance.m_activeOffers.Where<OfferPack>(o => o.shopCategory == _category.sku).ToList();
		result.Sort(OfferPackComparer);
        return result;
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
    public static void LogPack(OfferPack _pack, string _msg, Color _color, params object[] _replacements) {
#if LOG_PACKS
		if(!string.IsNullOrEmpty(LOG_PACK_SKU) && !_pack.def.sku.Contains(LOG_PACK_SKU)) return;
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

		AppendCollection(ref str, m_allOffersBySku, "All", false);
		AppendCollection(ref str, m_allEnabledOffers, "All Enabled", false);
		AppendCollection(ref str, m_activeOffers, "Active", false);

		//str += "\n";
		str += Colors.skyBlue.OpenTag();
		AppendCollection(ref str, m_allEnabledRotationalOffers, "All Rotational Enabled", false);
		AppendCollection(ref str, m_activeRotationalOffers, "Active Rotational", false);
		str += Colors.skyBlue.CloseTag();

		//str += "\n";
		str += Colors.red.OpenTag();
		AppendCollection(ref str, m_offersToRemove, "To Remove", true);
		str += Colors.red.CloseTag();

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
	private void AppendCollection<T>(ref string _str, List<T> _collection, string _collectionName, bool _printList) where T : OfferPack {
		_str += "\t" + _collectionName + ": " + _collection.Count + "\n";
		if(_printList) {
			foreach(OfferPack pack in _collection) {
				_str += "\t\t" + pack.def.sku + "\n";
			}
		}
	}

	/// <summary>
	/// Appends the given collection to the target log string.
	/// </summary>
	/// <param name="_str">String.</param>
	/// <param name="_collection">Collection.</param>
	/// <param name="_collectionName">Name of the collection.</param>
	private void AppendCollection<K, T>(ref string _str, Dictionary<K, T> _collection, string _collectionName, bool _printList) where T : OfferPack {
		_str += "\t" + _collectionName + ": " + _collection.Count + "\n";
		if(_printList) {
			foreach(KeyValuePair<K, T> kvp in _collection) {
				_str += "\t\t" + kvp.Value.def.sku + "\n";
			}
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
    private void LogHistory(OfferPack.Type _type, ref Queue<string> _history) {
#if LOG
		if(!FeatureSettingsManager.IsDebugEnabled) return;

		string str = _type.ToString() + " History (" + _history.Count + "):";
		foreach(string data in _history) {
			str += "\n    " + data;
		}
		Log(str);
#endif
	}

	/// <summary>
	/// Aux method to select a debug color based on a pack's state.
	/// </summary>
	/// <param name="_pack"></param>
	/// <returns></returns>
	public static Color DEBUG_GetColorByState(OfferPack _pack) {
		Color c = Color.white;
		if(_pack != null) {
			switch(_pack.state) {
				case OfferPack.State.PENDING_ACTIVATION: c = Colors.cyan; break;
				case OfferPack.State.EXPIRED: c = Colors.red; break;
				case OfferPack.State.ACTIVE: c = Colors.lime; break;
			}
		}
		return c;
	}

	/// <summary>
	/// For debug purposes only!
	/// Skips the cooldown timer of the free offer.
	/// </summary>
	public static void DEBUG_SkipFreeOfferCooldown() {
		UsersManager.currentUser.freeOfferCooldownEndTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
	}
}