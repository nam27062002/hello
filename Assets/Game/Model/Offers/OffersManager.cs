// OffersManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
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

	private OfferPack m_featuredOffer = null;
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

	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		// Check requirements
		Debug.Assert(ContentManager.ready, "Definitions Manager must be ready before invoking this method.");

		// Clear current offers cache
		instance.m_allOffers.Clear();
		instance.m_activeOffers.Clear();
		instance.m_featuredOffer = null;

		// Create data for each known offer pack definition
		List<DefinitionNode> offerDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.OFFER_PACKS);
		for(int i = 0; i < offerDefs.Count; ++i) {
			OfferPack newPack = new OfferPack();
			newPack.InitFromDefinition(offerDefs[i]);
			instance.m_allOffers.Add(newPack);
		}

		// Sort the offers list using custom criteria
		instance.m_allOffers.Sort(OfferPackComparer);

		// Refresh active and featured offers
		Refresh();
	}

	/// <summary>
	/// Will refresh the list of offers to be displayed as well as the featured offer.
	/// </summary>
	public static void Refresh() {
		// Delegate to the non-static method
		instance.RefreshInternal();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Will refresh the list of offers to be displayed as well as the featured offer.
	/// </summary>
	private void RefreshInternal() {
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
		return _p1.order.CompareTo(_p2.order);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}