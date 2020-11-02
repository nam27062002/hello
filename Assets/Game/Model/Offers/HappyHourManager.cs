// HappyHourManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//#define LOG

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class to handle Happy Hour logic.
/// </summary>
public class HappyHourManager {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Aux class to save/load from persistence.
	/// </summary>
	public class SaveData {
		public string activeSku = "";
		public DateTime expirationTime = DateTime.MinValue;
		public float extraGemsFactor = 0f;
		public string lastPackSku = "";

		/// <summary>
		/// Load from a json data node.
		/// </summary>
		/// <param name="_data"></param>
		public void FromJson(JSONClass _data) {
			// Reset values first
			Reset();

			string key = "activeSku";
			if(_data.ContainsKey(key)) {
				activeSku = _data[key];
			}

			key = "expirationTime";
			if(_data.ContainsKey(key)) {
				expirationTime = new DateTime(PersistenceUtils.SafeParse<long>(_data[key]));
			}

			key = "extraGemsFactor";
			if(_data.ContainsKey(key)) {
				extraGemsFactor = PersistenceUtils.SafeParse<float>(_data[key]);
			}

			key = "lastPackSku";
			if(_data.ContainsKey(key)) {
				lastPackSku = _data[key];
			}
		}

		/// <summary>
		/// Dump into a json object.
		/// </summary>
		/// <returns></returns>
		public JSONClass ToJson() {
			// Create empty object
			JSONClass data = new JSONClass();

			data["activeSku"] = activeSku;
			data["expirationTime"] = PersistenceUtils.SafeToString(expirationTime.Ticks);
			data["extraGemsFactor"] = PersistenceUtils.SafeToString(extraGemsFactor);

			if(!string.IsNullOrEmpty(lastPackSku)) {	// No need to add it if none - more compact save file!
				data["lastPackSku"] = lastPackSku;
			}

			return data;
		}

		/// <summary>
		/// Reset to default values
		/// </summary>
		public void Reset() {
			activeSku = string.Empty;
			expirationTime = DateTime.MinValue;
			extraGemsFactor = 0f;
			lastPackSku = string.Empty;
		}
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	private Dictionary<string, HappyHour.Data> m_allHappyHours = new Dictionary<string, HappyHour.Data>();
	private List<HappyHour.Data> m_enabledHappyHours = new List<HappyHour.Data>();
	private HappyHour m_happyHour = new HappyHour();
	public HappyHour happyHour {
		get { return m_happyHour; }
	}

	// Aux data
	private DateTime m_lastHappyHourExpirationDate = DateTime.MinValue;

	private DefinitionNode m_lastPackDef = null;    // Keep a track of the pack that triggered the happy hour, so it can be displayed in the popup
	public DefinitionNode lastPackDef {
		get { return m_lastPackDef; }
		set { m_lastPackDef = value; }
	}

	// Aux vars
	private List<HappyHour.Data> m_toRemove = new List<HappyHour.Data>();   // Keep the list in memory to avoid runtime allocations

	// Popup control
	private bool m_pendingPopup = false;
	public bool pendingPopup {
		get { return m_pendingPopup; }
		set {
			m_pendingPopup = value;
			Save(); // Save persistence so the popup wont be shown again
		}
	}

	private int m_triggerPopupAtRun = 0;
	public int triggerPopupAtRun {
		get { return m_triggerPopupAtRun; }
	}



    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public bool IsActive()
    {
        return happyHour.IsActive();
    }


    /// <summary>
    /// Default constructor.
    /// </summary>
    public HappyHourManager() {
		// Subscribe to events
		Messenger.AddListener<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, OnHcPackAccquired);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HappyHourManager() {
		// Unsubscribe to events
		Messenger.RemoveListener<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, OnHcPackAccquired);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parse definitions and cache happy hour datas for simpler management.
	/// </summary>
	public void InitFromDefinitions() {
		// Clear current data
		m_allHappyHours.Clear();
		m_enabledHappyHours.Clear();
		m_happyHour.Finish();

		// Parse all happy hour definitions
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.HAPPY_HOUR);
		for(int i = 0; i < defs.Count; ++i) {
			// Create new HappyHour object
			HappyHour.Data newData = new HappyHour.Data(defs[i]);
			newData.def = defs[i];

			// Save it to collections
			m_allHappyHours.Add(newData.def.sku, newData);
			if(newData.enabled) {
				m_enabledHappyHours.Add(newData);
			}
		}
	}

	/// <summary>
	/// To be called periodically to check for new Happy Hour activations / expired Happy Hours.
	/// </summary>
	public void Update() {
		// Aux vars
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();

		// If we have an active happy hour, look for expiration
		if(m_happyHour.IsActive()) {
			if(serverTime >= m_happyHour.expirationTime) {
				// Expired! Clear it
				Log(Colors.red.Tag(m_happyHour.data.def.sku + " EXPIRED"));
				m_happyHour.Finish();
				Save();
			}
		}

		// Otherwise check activation
		// Except during ftux
		else if(UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_HAPPY_HOUR_AT_RUN) {
			// Check all enabled happy hours looking for the one that needs to be activated
			HappyHour.Data candidateData = null;
			for(int i = 0; i < m_enabledHappyHours.Count; ++i) {
				// Simpler naming
				candidateData = m_enabledHappyHours[i];
				Log(Colors.white.Tag("Checking candidate " + candidateData.def.sku));

                                
                // Only HH with automatic start are elegible for time activation
                if (!candidateData.autoStart)
                {
                    Log(Colors.orange.Tag("TO BE TRIGGERED MANUALLY, SKIP"));
                    continue;
                }
                
				// Only happy hours triggered by date are eligible for time activation!
				if(!candidateData.triggeredByDate) {
					Log(Colors.orange.Tag("NOT TRIGGERED BY TIME, SKIP"));
					continue;
				}


				// Did we reached the start date?
				if(candidateData.startDate <= serverTime) {
					// Make sure it's not expired
					Log(Colors.green.Tag("START DATE REACHED"));
					if(candidateData.endDate > serverTime) {
						// We can't activate it if we have triggered another Happy
						// Hour (most likely a local one) meant to finish after the start date
						// We don't need to check for it anymore, we can actually remove it from the list!
						Log(Colors.green.Tag("END DATE IN THE FUTURE"));
						if(m_lastHappyHourExpirationDate > candidateData.startDate) {
							// Remove it from the list
							Log(Colors.red.Tag("START DATE BEFORE THE LAST EXPIRATION DATE, REMOVE"));
							m_toRemove.Add(candidateData);
							continue;
						} else {
							// Valid data! Make it the active one!
							Log(Colors.lime.Tag("VALID DATA! MAKE IT THE ACTIVE ONE"));
							ActivateHappyHour(candidateData);
							break;	// No need to keep looping
						}
					} else {
						// HH expired, remove it from the pool
						Log(Colors.red.Tag("END DATE IN THE PAST (HH EXPIRED), REMOVE"));
						m_toRemove.Add(candidateData);
					}
				} else {
					Log(Colors.orange.Tag("START DATE NOT REACHED, SKIP"));
				}
			}

			// Purge those hours that cannot be triggered anymore
			if(m_toRemove.Count > 0) {
				for(int i = 0; i < m_toRemove.Count; ++i) {
					m_enabledHappyHours.Remove(m_toRemove[i]);
				}
				m_toRemove.Clear();
			}
		}
	}

	/// <summary>
	/// Activate the happy hour with the given data.
	/// </summary>
	/// <param name="_data">Data to be used for HH activation.</param>
	private void ActivateHappyHour(HappyHour.Data _data) {
		// Activate the happy hour object
		m_happyHour.Activate(_data);

		// Store new expiration date
		m_lastHappyHourExpirationDate = m_happyHour.expirationTime;

		// Program popup
		m_triggerPopupAtRun = UsersManager.currentUser.gamesPlayed + _data.popupTriggerRunNumber;

		// Save persistence
		Save();
	}

	/// <summary>
	/// Open, initialize and show the happy hour popup.
	/// </summary>
	/// <returns>The opened popup. Can be <c>null</c> if the popup couldn't be opened.</returns>
	public PopupHappyHour OpenPopup() {
		// Dont open the popup if we already are displaying a HH popup
		if (PopupManager.GetOpenPopup(PopupHappyHour.PATH) != null)
			return null;

		// Load the popup
		PopupController popup = PopupManager.LoadPopup(PopupHappyHour.PATH);
		PopupHappyHour popupHappyHour = popup.GetComponent<PopupHappyHour>();

		// Choose gems pack to display with the popup
		DefinitionNode packDef = null;
		if(m_lastPackDef != null) {
			packDef = m_lastPackDef;
		} else {
			// Get all HC pack definitions and choose the "bestValue" one.
			List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", "hc");
			for(int i = 0; i < defs.Count; ++i) {
				// Skip if not enabled
				if(!defs[i].GetAsBool("enabled")) continue;

				// Best value?
				if(defs[i].GetAsBool("bestValue")) {
					packDef = defs[i];
					break;
				}
			}
		}

		// Nothing to do if there is no pack to be displayed
		if(packDef == null) return null;

        // Initialize the popup (set the discount % values)
        popupHappyHour.Init(packDef);

		// And launch it
		popup.Open();

		// Done!
		return popupHappyHour;
	}

	/// <summary>
	/// Check whether the given pack is affected by this Happy Hour.
	/// No type checks will be performed, pack should be of type HC Currency.
	/// </summary>
	/// <param name="_packDef">The pack to be checked.</param>
	/// <returns>Whether the given pack is affected by the current Happy Hour.</returns>
	public bool IsPackAffected(DefinitionNode _packDef) {
		// Check params
		if(_packDef == null) return false;

		// Ignore if happy hour is not active
		if(!m_happyHour.IsActive()) return false;
        
        // Check price limits
        if (_packDef.GetAsFloat("refPrice") < m_happyHour.minPriceAffected)
            return false;
        
        if (_packDef.GetAsFloat("refPrice") > m_happyHour.maxPriceAffected)
            return false;

            // Depends on mode
		switch(m_happyHour.affectedPacks) {
			case HappyHour.AffectedPacks.ALL_PACKS: {
				return true;
			}

			case HappyHour.AffectedPacks.LAST_PACK_PURCHASED: {
				// Only if it's the same pack as the last purchased
				if(m_lastPackDef != null) {
					return m_lastPackDef.sku == _packDef.sku;
				}
			} break;

			case HappyHour.AffectedPacks.LAST_PACK_PURCHASED_AND_ABOVE: {
				// Use the "order" field
				if(m_lastPackDef != null) {
					return m_lastPackDef.GetAsInt("order") <= _packDef.GetAsInt("order");
				}
			} break;
		}

		// Fallback - don't apply happy hour to this pack
		return false;
	}

    /// <summary>
    /// Force the start of a happy hour with the specified SKU, even if such HH appears as
    /// disabled in the content.
    /// </summary>
    /// <param name="_happyHourSku">The SKU of the happy hour config in the content</param>
    public void ForceStart(string _happyHourSku)
    {
        
        // Check that such SKU is defined
        if (!m_allHappyHours.ContainsKey(_happyHourSku))
        {
            Debug.LogError("The specified Happy Hour configuration " + _happyHourSku +
                           " doesnt exist in the content");
            return;
        }

        HappyHour.Data data = m_allHappyHours[_happyHourSku];
        ActivateHappyHour(data);

    }

    /// <summary>
    /// Force the stop of the specified Happy Hour
    /// Usually happy hours expires after a time, but we provide this method to be
    /// used from the cheats panel for debug purposes.
    /// </summary>
    /// <param name="_happyHourSku"></param>
    public void ForceStop(string _happyHourSku)
    {
        // Check if the specified SKU is the current one
        if (m_happyHour != null &&
            m_happyHour.IsActive() &&
            m_happyHour.data.def.sku == _happyHourSku)
        {
            
            // End the happy hour
            m_happyHour.Finish();
            
            // Store new expiration date
            m_lastHappyHourExpirationDate = m_happyHour.expirationTime;
            
            // Save persistence
            Save();
        }
        
        
    }

	//------------------------------------------------------------------------//
	// PERSISTENCE															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load persistence from current user.
	/// </summary>
	public void Load() {
		// Reset current data to defaults
		Reset();

		// Get savedata
		SaveData saveData = UsersManager.currentUser.happyHourData;

		// Last purchased pack is independent from the happy hour object data, always restore it
		if(!string.IsNullOrEmpty(saveData.lastPackSku)) {
			m_lastPackDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, saveData.lastPackSku);
		}

		// Find data corresponding to stored sku
		if(m_allHappyHours.ContainsKey(saveData.activeSku)) {
			HappyHour.Data happyHourData = m_allHappyHours[saveData.activeSku];

			// We don't want to restore the happy hour if it is no longer enabled
			if(happyHourData.enabled) {
				// Make sure values are consistent
				if(saveData.expirationTime > DateTime.MinValue && saveData.extraGemsFactor > 0f) {
					// Initialize happy hour with chosen data
					m_happyHour.Activate(happyHourData);

					// Override with persistence values
					m_happyHour.expirationTime = saveData.expirationTime;
					m_happyHour.extraGemsFactor = saveData.extraGemsFactor;
					m_lastHappyHourExpirationDate = m_happyHour.expirationTime;
				}
			}
		}
	}

	/// <summary>
	/// Save to current user profile.
	/// </summary>
	public void Save() {
		// Store current values in the user's profile
		SaveData saveData = UsersManager.currentUser.happyHourData; // Shorter notation
		saveData.Reset();

		if(m_happyHour.IsActive()) {
			saveData.activeSku = m_happyHour.data.def.sku;
			saveData.extraGemsFactor = m_happyHour.extraGemsFactor;
		}

		saveData.expirationTime = m_lastHappyHourExpirationDate;

		if(m_lastPackDef != null) {
			saveData.lastPackSku = m_lastPackDef.sku;
		}

		// Make sure it's saved
		PersistenceFacade.instance.Save_Request();
	}

	/// <summary>
	/// Reset to default state.
	/// </summary>
	public void Reset() {
		m_lastPackDef = null;
		m_lastHappyHourExpirationDate = DateTime.MinValue;
		m_happyHour.Finish();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called when the player buys a gem pack
	/// </summary>
	private void OnHcPackAccquired(bool _forcePopup, string _packSku) {
		// This is redundant, but just in case. I have seen some weird stuff out there...
		m_happyHour = OffersManager.happyHourManager.happyHour;

		// Is there an active happy hour already?
		if (m_happyHour.IsActive()) {
			// Update bonus percentage
			m_happyHour.IncreaseExtraGemsFactor();
		}

		// No active happy hour, check whether there is a valid candidate to be triggered
		else {
			// Check all enabled happy hours looking for the one that needs to be activated
			for(int i = 0; i < m_enabledHappyHours.Count; ++i) {
                // Obly HH with auto start are elegible to be activated here!
                if (!m_enabledHappyHours[i].autoStart) continue;
                
				// Only happy hours NOT triggered by date are eligible for this type of activation!
				if(m_enabledHappyHours[i].triggeredByDate) continue;

				// Nothing else to check, this happy hour is a valid candidate :)
				ActivateHappyHour(m_enabledHappyHours[i]);
				
				// No need to iterate more
				break;
			}
		}

		// Did we successfully activated a Happy Hour?
		if(m_happyHour.IsActive()) {
			// Store the pack that triggered it
			m_lastPackDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _packSku);

			// If required and allowed, instanly trigger the popup. Otherwise just mark it as pending.
			if(_forcePopup && m_happyHour.data.popupTriggerRunNumber == 0) {

                // Wait some time before showing the popup so the coins/gems trail FX can finish
                UbiBCN.CoroutineManager.DelayedCall( () => OpenPopup(), 1.5f, false);
			} else {
				m_pendingPopup = true;
			}

		}

		// Save persistence
		Save();
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_str"></param>
	[System.Diagnostics.Conditional("LOG")]
	private void Log(string _str) {
		Debug.Log(Colors.orange.Tag("[HH] ") + _str);
	}
}