// HappyHour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/01/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class HappyHour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum AffectedPacks {
		ALL_PACKS,
		LAST_PACK_PURCHASED,
		LAST_PACK_PURCHASED_AND_ABOVE
	}

	/// <summary>
	/// Subset of a parsed definition.
	/// Convenient for frequently consulted data.
	/// </summary>
	public class Data {
		public DefinitionNode def = null;

		public DateTime startDate = DateTime.MinValue;
		public DateTime endDate = DateTime.MaxValue;

		public bool triggeredByDate { get { return startDate > DateTime.MinValue; } }

		public int popupTriggerRunNumber = 0;

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		/// <param name="_def">Def to be used to initialize this data.</param>
		public Data(DefinitionNode _def) {
			// Store definition object
			def = _def;

			// Parse start date
			if(def.Has("startDate")) {
				startDate = TimeUtils.TimestampToDate(def.GetAsLong("startDate", 0), false);
			}

			// End date
			if(def.Has("endDate")) {
				endDate = TimeUtils.TimestampToDate(def.GetAsLong("endDate", 0), false);
			}

			// Popup delay
			popupTriggerRunNumber = def.GetAsInt("triggerRunNumber", 0);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private Data m_data = null;
	public Data data {
		get { return m_data; }
	}

	// Working mode
	private AffectedPacks m_affectedPacks = AffectedPacks.ALL_PACKS;
	public AffectedPacks affectedPacks {
		get { return m_affectedPacks; }
	}

	// Current offer values
	private DateTime m_expirationTime = DateTime.MinValue; // Timestamp when the offer will finish
	public DateTime expirationTime {
		get { return m_expirationTime; }
		set { m_expirationTime = value; }
	}

	private float m_extraGemsFactor = 0f; // The current extra gem multiplier for this offer
	public float extraGemsFactor {
		get { return m_extraGemsFactor; }
		set { m_extraGemsFactor = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HappyHour() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HappyHour() {

	}

	/// <summary>
	/// Initialize with the given data and activate the Happy Hour.
	/// If already active, it will reset current state and data and activate it again with the new data.
	/// </summary>
	/// <param name="_data">Data to be used for initialization. If <c>null</c>, current Happy Hour will be finished.</param>
	public void Activate(Data _data) {
		// Clear current data
		Finish();

		// If null, do nothing else
		if(_data == null) return;
		if(_data.def == null) return;

		// Cache some vars
		m_data = _data;

		switch(m_data.def.GetAsString("gemsPacksAffected")) {
			case "allPacks":					m_affectedPacks = AffectedPacks.ALL_PACKS; break;
			case "lastPackPurchased":			m_affectedPacks = AffectedPacks.LAST_PACK_PURCHASED; break;
			case "lastPackPurchasedAndAbove":	m_affectedPacks = AffectedPacks.LAST_PACK_PURCHASED_AND_ABOVE; break;
		}

		// Initialize live data
		m_extraGemsFactor = m_data.def.GetAsFloat("percentageMinExtraGems");

		// Compute expiration date
		if(m_data.triggeredByDate) {
			// Expiration date defined directly in the happy hour definition
			m_expirationTime = m_data.endDate;
		} else {
			// Expiration date based on duration
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			m_expirationTime = serverTime.AddMinutes(data.def.GetAsFloat("happyHourTimer"));
		}
	}

	/// <summary>
	/// Finalize 
	/// </summary>
	public void Finish() {
		// Clear data
		m_data = null;
		m_extraGemsFactor = 0f;
		m_expirationTime = DateTime.MinValue;
	}

	/// <summary>
	/// Is this Happy Hour active?
	/// </summary>
	/// <returns>Whether the happy hour is active or not.</returns>
	public bool IsActive() {
		return m_data != null;
	}

	//------------------------------------------------------------------------//
	// OTHER PUBLIC METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Increment the extra gems each time the happy hour is reactivated.
	/// </summary>
	public void IncreaseExtraGemsFactor() {
		// Nothing to do if we don't have valid data
		if(!IsActive()) return;

		// Increment the extra gems each time the happy hour is reactivated
		m_extraGemsFactor += m_data.def.GetAsFloat("percentageIncrement");

		// Cap the value to the maximum
		float percentageMaxExtraGems = m_data.def.GetAsFloat("percentageMaxExtraGems");
		if(m_extraGemsFactor > percentageMaxExtraGems) {
			m_extraGemsFactor = percentageMaxExtraGems;
		}

		// If not triggered by date, restart timer
		if(!data.triggeredByDate) {
			// Extend the expiration time of this offer
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			m_expirationTime = serverTime.AddMinutes(data.def.GetAsFloat("happyHourTimer"));
		}
	}

	/// <summary>
	/// The total amount of seconds left for this happy hour.
	/// </summary>
	/// <returns>Negative value if the Happy Hour is expired or inactive.</returns>
	public double TimeLeftSecs() {
		// Check inactive Happy Hour
		if(!IsActive()) return -1;

		// Compute remaining time and return
		DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
		return expirationTime.Subtract(serverTime).TotalSeconds;
	}

	/// <summary>
	/// Apply the extra amount formula to the given base amount using this Happy Hour's current data.
	/// </summary>
	/// <returns>The total final amount.</returns>
	public int ApplyHappyHourExtra(int _amount) {
		// Only if active
		if(!IsActive()) return _amount;
		
		// Apply the extra gems factor
		return Mathf.RoundToInt((_amount) * (1 + extraGemsFactor));
	}
}