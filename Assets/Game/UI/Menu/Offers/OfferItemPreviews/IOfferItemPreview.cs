// OfferItemPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to encapsulate the preview of an item.
/// </summary>
public abstract class IOfferItemPreview : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	protected OfferPackItem m_item = null;
	protected DefinitionNode m_def = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the widget with the data of a specific offer item.
	/// </summary>
	/// <param name="_item">Item.</param>
	public void InitFromItem(OfferPackItem _item) {
		// Store new item
		m_item = _item;

		Debug.Assert(m_item != null && m_item.reward != null, "ITEM NOT PROPERLY INITIALIZED", this);

		// Call internal initializer
		InitInternal();
	}

	/// <summary>
	/// Set this preview's parent and adjust its size to fit it.
	/// </summary>
	/// <param name="_t">New parent!</param>
	public virtual void SetParentAndFit(RectTransform _t) {
		this.transform.SetParent(_t, false);

		// [AOC] Adjusting the rect transformt makes it super-difficult to keep all child hierarchy properly positioned and scaled
		//		 Let's go with a scale change instead
		/*rectTransform.anchorMin = GameConstants.Vector2.zero;
		rectTransform.anchorMax = GameConstants.Vector2.one;
		rectTransform.offsetMin = GameConstants.Vector2.zero;
		rectTransform.offsetMax = GameConstants.Vector2.zero;*/

		float sx = _t.rect.width / Mathf.Max(rectTransform.rect.width, float.Epsilon);// Prevent division by 0
		float sy = _t.rect.height / Mathf.Max(rectTransform.rect.height, float.Epsilon);// Prevent division by 0
		float scale = (sx < sy)? sx : sy;
	
		rectTransform.localScale = new Vector3(scale, scale, scale);
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected abstract void InitInternal();

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <returns>The localized description.</returns>
	public abstract string GetLocalizedDescription();
}