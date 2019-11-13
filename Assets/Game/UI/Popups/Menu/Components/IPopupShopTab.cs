// PopupCurrencyShopTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tab in the currency shop!
/// </summary>
public abstract class IPopupShopTab : Tab {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Event Types
	public class ShopTabEvent : UnityEvent<IPopupShopTab> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] protected GameObject m_pillPrefab = null;
	[SerializeField] protected ScrollRect m_scrollList = null;
	public ScrollRect scrollList {
		get { return m_scrollList; }
	}

	// Internal
	protected List<IPopupShopPill> m_pills = new List<IPopupShopPill>();
	public List<IPopupShopPill> pills {
		get { return m_pills; }
	}

	// Events
	public ShopTabEvent OnPillListChanged = new ShopTabEvent();	// To be broadcasted by heirs

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this tab and instantiate required pills.
	/// </summary>
	public abstract void Init();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		Debug.Assert(m_pillPrefab != null, "Missing required reference!");
		Debug.Assert(m_scrollList != null, "Missing required reference!");
		base.Awake();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear scroll list.
	/// </summary>
	public virtual void Clear() {
		// Clear current content
		m_scrollList.content.DestroyAllChildren(false);
		m_pills.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}