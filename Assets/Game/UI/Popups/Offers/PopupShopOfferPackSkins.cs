// PopupShopOfferPack.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Offer Pack Popup.
/// </summary>
public class PopupShopOfferPackSkins : PopupShopOfferPack
{
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public new const string PATH = "UI/Popups/Economy/PF_PopupShopOfferPackSkins";
	private const float REFRESH_FREQUENCY = 1f; // Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField]
	private OfferItemSlot bigPreview;

	private int itemIndexSelected = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// Redraw the view
	/// </summary>
	protected override void Refresh()
    {
        base.Refresh();

        // Clear the slot
		bigPreview.InitFromItem(null);

		// Show the item in the preview panel
		bigPreview.InitFromItem(m_pack.items[itemIndexSelected]);

		// Select/deselect the proper tab
		List<OfferItemSlot> slots = m_rootPill.currentLayout.m_offerItems;
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].GetComponent<SelectableButton>().SetSelected(itemIndexSelected == i);
		}


	}

    /// <summary>
    /// Select one of the items from the pack and refresh the view
    /// </summary>
    /// <param name="_index"></param>
    private void SelectItem (int _index)
    {
        // Protect from index out of bounds
        if (_index > m_pack.items.Count - 1)
			return;

        // The item is already selected?
		if (_index == itemIndexSelected)
			return;

		itemIndexSelected = _index;

        // Apply the changes to the view
		Refresh();

	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
    /// <summary>
    /// The player clicked on one of the items in the tab selector
    /// </summary>
    /// <param name="_index"></param>
    public void OnSelectItem (int _index)
    {
		SelectItem(_index);
		
	}

}