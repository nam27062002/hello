// OfferItemUI.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
//#define LOG
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using System.Diagnostics;

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

	public enum PreviewType {
		ALWAYS_3D,
		ALWAYS_2D,
		DRAGONS_2D
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Header("Settings")]
	[FormerlySerializedAs("m_type")]
	[SerializeField] protected Type m_slotType = Type.PILL_BIG;
	[SerializeField] protected PreviewType m_previewType = PreviewType.ALWAYS_3D;

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

		Log("Init From Item: {0} ({1}) | {2}", Color.yellow, m_item.type, m_item.sku, reloadPreview);

		// Load new preview (if required)
		if(reloadPreview) {
			// Try loading the preferred preview type
			// If there is no preview of the preferred type, try other types until we have a valid preview
			IOfferItemPreview.Type preferredPreviewType = GetPreferredPreviewType(item.type);
			GameObject previewPrefab = ShopSettings.GetPrefab(item.type, preferredPreviewType);
			Log("Attempting to get preview prefab of type {0}: {1}", Color.yellow, preferredPreviewType, (previewPrefab == null ? Color.red.Tag("NULL") : previewPrefab.name));
			if(previewPrefab == null) {
				// Loop will stop with a valid prefab
				for(int i = 0; i < (int)IOfferItemPreview.Type.COUNT && previewPrefab == null; ++i) {
					// Skip preferred type (already checked)
					if(i == (int)preferredPreviewType) continue;
					previewPrefab = ShopSettings.GetPrefab(item.type, (IOfferItemPreview.Type)i);
					Log("\tCouldn't do it, checking type {0}...: {1}", Color.yellow, ((IOfferItemPreview.Type)i), (previewPrefab == null ? Color.red.Tag("NULL") : previewPrefab.name));
				}
			}

			// Instantiate preview! :)
			if(previewPrefab != null) {
				GameObject previewInstance = Instantiate<GameObject>(previewPrefab);
				previewInstance.SetActive(true);
				m_preview = previewInstance.GetComponent<IOfferItemPreview>();
			} else {
				Debug.LogError("Couldn't find prefab for item of type " + m_item.type + " (" + m_item.sku + ")");
			}
		}

		// Initialize preview with item data
		if(m_preview != null) {
			m_preview.InitFromItem(m_item, m_slotType);
			m_preview.SetParentAndFit(m_previewContainer as RectTransform);
		} else {
			// Skip if preview is not initialized (something went very wrong :s)
			Debug.LogError("Attempting to initialize slot for item " + m_item.sku + " but reward preview is null!");
		}

		// Initialize texts apart for visual clarity
		InitTexts();

		// Initialize power Icon
		if(m_powerIcon != null && m_preview != null) {
			m_preview.InitPowerIcon(m_powerIcon, m_slotType);
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
			string textStr = m_preview.GetLocalizedMainText(m_slotType);
			m_mainText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_mainText.text = textStr;
			}
		}

		// Secondary Text
		if(m_secondaryText != null) {
			string textStr = m_preview.GetLocalizedSecondaryText(m_slotType);
			m_secondaryText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_secondaryText.text = textStr;
			}
		}

		// Amount Text
		if(m_amountText != null) {
			string textStr = m_preview.GetLocalizedAmountText(m_slotType);
			m_amountText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_amountText.text = textStr;
			}
		}

		// Description Text
		if(m_descriptionText != null) {
			string textStr = m_preview.GetLocalizedDescriptionText(m_slotType);
			m_descriptionText.gameObject.SetActive(textStr != null);
			if(textStr != null) {
				m_descriptionText.text = textStr;
			}
		}
	}

	/// <summary>
	/// Find out which is the preferred preview type (2D or 3D) based on current
	/// slot setup and given reward type.
	/// </summary>
	/// <param name="_rewardType">The reward type to be considered.</param>
	/// <returns>The preview type to be used.</returns>
	protected virtual IOfferItemPreview.Type GetPreferredPreviewType(string _rewardType) {
		// 3D by default
		IOfferItemPreview.Type preferredPreviewType = IOfferItemPreview.Type._3D;

		// Check slot config
		switch(m_previewType) {
			case PreviewType.ALWAYS_3D: {
				preferredPreviewType = IOfferItemPreview.Type._3D;
			} break;

			case PreviewType.ALWAYS_2D: {
				preferredPreviewType = IOfferItemPreview.Type._2D;
			} break;

			case PreviewType.DRAGONS_2D: {
				// Depends on item type - 2D for dragons and skins, 3D for the rest
				switch(item.type) {
					case Metagame.RewardDragon.TYPE_CODE:
					case Metagame.RewardSkin.TYPE_CODE: {
						preferredPreviewType = IOfferItemPreview.Type._2D;
					} break;

					default: {
						preferredPreviewType = IOfferItemPreview.Type._3D;
					} break;
				}
				preferredPreviewType = IOfferItemPreview.Type._3D;
			} break;
		}

		return preferredPreviewType;
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

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	    /// <summary>
    /// Log into the console (if enabled).
    /// </summary>
    /// <param name="_msg">Message to be logged. Can have replacements like string.Format method would have.</param>
    /// <param name="_replacements">Replacements, to be used as string.Format method.</param>
#if LOG
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string _msg, params object[] _replacements) {
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		ControlPanel.Log(string.Format(_msg, _replacements), ControlPanel.ELogChannel.Offers);
	}

    /// <summary>
    /// Log into the console (if enabled).
    /// </summary>
    /// <param name="_msg">Message to be logged. Can have replacements like string.Format method would have.</param>
    /// <param name="_color">Message color.</param>
    /// <param name="_replacements">Replacements, to be used as string.Format method.</param>
#if LOG
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string _msg, Color _color, params object[] _replacements) {
		Log(_color.Tag(_msg), _replacements);
	}
}