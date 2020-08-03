// LeagueSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialized item.
/// </summary>
public class LeagueSelectorItem : IUISelectorItem {
	public HDLeagueData leagueData;
	public LeagueSelectorItem(HDLeagueData _leagueData) { leagueData = _leagueData; }
	public bool CanBeSelected() { return true; }
}

/// <summary>
/// Auxiliar class to help with the scrolling logic.
/// </summary>
public class LeagueSelector : UISelectorTemplate<LeagueSelectorItem> {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_prevButtonAnim = null;
	[SerializeField] private ShowHideAnimator m_nextButtonAnim = null;

	//------------------------------------------------------------------------//
	// METHODS														  		  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select the item with the given league data.
	/// </summary>
	/// <param name="_data">League to be selected.</param>
	public void SelectItem(HDLeagueData _data) {
		// Get the item corresponding to this league data
		LeagueSelectorItem item = GetItem(_data);

		// Select it!
		SelectItem(item);
	}

	/// <summary>
	/// Find the Selector Item matching a specific league data.
	/// </summary>
	/// <returns>The matching item. <c>null</c> if none matches.</returns>
	/// <param name="_data">League to be found.</param>
	public LeagueSelectorItem GetItem(HDLeagueData _data) {
		// Unfortunately we cannot use List.Find with a parameter, so let's just do a manual search
		// A league selector doesn't have so many items anyway
		for(int i = 0; i < m_items.Count; ++i) {
			if(m_items[i].leagueData == _data) {
				return m_items[i];
			}
		}
		return null;
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES												  		  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Changes item selected to the given one, by item index.
	/// Index will be clamped to the amount of items.
	/// </summary>
	/// <param name="_idx">Index of the item to be selected.</param>
	/// <param name="_looped">Did we looped when selecting this item?</param>
	protected override void SelectItemInternal(int _idx, bool _looped) {
		// Let parent do its thing
		base.SelectItemInternal(_idx, _looped);

		// Toggle arrows visibility
		m_prevButtonAnim.Set(m_selectedIdx > 0);	// Hide for first item
		m_nextButtonAnim.Set(m_selectedIdx < m_items.Count - 1);	// Hide for last item
	}
}