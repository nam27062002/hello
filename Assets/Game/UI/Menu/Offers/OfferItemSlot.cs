// OfferItemUI.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using SoftMasking;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Widget to display the info of an offer pack reward.
/// </summary>
public class OfferItemSlot : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// We'll use it to let the item previews decide which texts should be displayed.
	public enum Type {
		PILL_BIG,
		PILL_SMALL,
		POPUP_BIG,
		POPUP_SMALL,
		TOOLTIP
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Header("Settings")]
	[SerializeField] protected Type m_type = Type.PILL_BIG;
	[SerializeField] protected bool m_allow3dPreview = false;   // [AOC] In some cases, we want to display a 3d preview when appliable (pets/eggs)

	[Header("Mandatory Fields")]
	[SerializeField] protected Transform m_previewContainer = null;
	
	[Header("Optional Fields")]
	[SerializeField] protected TextMeshProUGUI m_mainText = null;
	[SerializeField] protected TextMeshProUGUI m_secondaryText = null;
	[SerializeField] protected TextMeshProUGUI m_amountText = null;
	[SerializeField] protected TextMeshProUGUI m_descriptionText = null;
	[Space]
	[SerializeField] protected PowerIcon m_powerIcon = null;	// Will only be displayed for some types

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	protected OfferPackItem m_item = null;
	public OfferPackItem item {
		get { return m_item; }
	}

	protected IOfferItemPreview m_preview = null;
	public IOfferItemPreview preview {
		get { return m_preview; }
	}


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	//------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Refresh the widget with the data of a specific offer item.
    /// </summary>
    /// <param name="_item">Item to be used to initialize the slot.</param>
    /// <param name="_order">Used to select the proper HC or SC icon</param>
    public virtual void InitFromItem(OfferPackItem _item, int _order = 0) {
		// Force reloading preview if item is different than the current one
		bool reloadPreview = false;
		if(m_item != _item) reloadPreview = true;

		// Store new item
		m_item = _item;

		// If given item is null, disable game object and don't do anything else
		if(m_item == null) {
			this.gameObject.SetActive(false);
			if(reloadPreview) ClearPreview();
			return;
		}

		// Aux vars
		Metagame.Reward reward = item.reward;

		// Activate game object
		this.gameObject.SetActive(true);

		// If a preview was already created, destroy it
		if(reloadPreview) ClearPreview();

		// Load new preview (if required)
		if(reloadPreview) {
			// Try loading the preferred preview type
			// If there is no preview of the preferred type, try other types untill we have a valid preview
			IOfferItemPreview.Type preferredPreviewType = m_allow3dPreview ? IOfferItemPreview.Type._3D : IOfferItemPreview.Type._2D;
			GameObject previewPrefab = ShopSettings.GetPrefab(item.type, preferredPreviewType);
			if(previewPrefab == null) {
				// Loop will stop with a valid prefab
				for(int i = 0; i < (int)IOfferItemPreview.Type.COUNT && previewPrefab == null; ++i) {
					// Skip preferred type (already checked)
					if(i == (int)preferredPreviewType) continue;
					previewPrefab = ShopSettings.GetPrefab(item.type, (IOfferItemPreview.Type)i);
				}
			}

			// Instantiate preview! :)
			if(previewPrefab != null) {
				GameObject previewInstance = Instantiate<GameObject>(previewPrefab);
				previewInstance.SetActive(true);
				m_preview = previewInstance.GetComponent<IOfferItemPreview>();
			}
		}

		// Initialize preview with item data
		if(m_preview != null) {
			m_preview.InitFromItem(m_item);
			m_preview.SetParentAndFit(m_previewContainer as RectTransform);
		} else {
			// Skip if preview is not initialized (something went very wrong :s)
			Debug.LogError("Attempting to initialize slot for item " + m_item.sku + " but reward is null!");
		}

		// Initialize texts apart for visual clarity
		InitTexts();

		// Initialize power Icon
		if(m_powerIcon != null && m_preview != null) {
			m_preview.InitPowerIcon(m_powerIcon, m_type);
		}
	}

	/// <summary>
	/// Initialize all texts in this slot.
	/// </summary>
	protected virtual void InitTexts() {
		// [AOC] All textfields not required by the item type (that is, the preview
		//		 returning null to the Get***Text() call) are disabled

		// Skip if preview is not initialized (something went very wrong :s)
		if(m_preview == null) return;

		// Main Text
		if(m_mainText != null) {
			string textStr = m_preview.GetLocalizedMainText(m_type);
			m_mainText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_mainText.text = textStr;
			}
		}

		// Secondary Text
		if(m_secondaryText != null) {
			string textStr = m_preview.GetLocalizedSecondaryText(m_type);
			m_secondaryText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_secondaryText.text = textStr;
			}
		}

		// Amount Text
		if(m_amountText != null) {
			string textStr = m_preview.GetLocalizedAmountText(m_type);
			m_amountText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_amountText.text = textStr;
			}
		}

		// Description Text
		if(m_descriptionText != null) {
			string textStr = m_preview.GetLocalizedDescriptionText(m_type);
			m_descriptionText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_descriptionText.text = textStr;
			}
		}
	}

	/// <summary>
	/// Destroy current preview, if any.
	/// </summary>
	protected void ClearPreview() {
		if(m_preview != null) {
			Destroy(m_preview.gameObject);
			m_preview = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// External event listener.
	/// </summary>
	/// <param name="eventType"></param>
	/// <param name="broadcastEventInfo"></param>
	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
		switch(eventType) {
			case BroadcastEventType.LANGUAGE_CHANGED: {
				OnLanguageChanged();
			} break;
		}
	}

	/// <summary>
	/// Localization language has changed, refresh texts.
	/// </summary>
	private void OnLanguageChanged() {
		// Reapply current reward
		InitFromItem(m_item);
	}
}