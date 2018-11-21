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
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Transform m_previewContainer = null;
	[SerializeField] private TextMeshProUGUI m_text = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_infoButton = null;
	[Tooltip("Optional")] [SerializeField] protected PowerIcon m_powerIcon = null;    // Will only be displayed for some types
	[Space]
	[SerializeField] private bool m_allow3dPreview = false;	// [AOC] In some cases, we want to display a 3d preview when appliable (pets/eggs)
	[Space]
	[Tooltip("Optional")] [SerializeField] private GameObject m_separator = null;

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	private OfferPackItem m_item = null;
	public OfferPackItem item {
		get { return m_item; }
		set { InitFromItem(value); }
	}

	private IOfferItemPreview m_preview = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Update associated separator
		if(m_separator != null) m_separator.SetActive(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Update associated separator
		if(m_separator != null) m_separator.SetActive(false);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the widget with the data of a specific offer item.
	/// </summary>
	public void InitFromItem(OfferPackItem _item) {
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
			OfferItemPrefabs.PrefabType preferredPreviewType = m_allow3dPreview ? OfferItemPrefabs.PrefabType.PREVIEW_3D : OfferItemPrefabs.PrefabType.PREVIEW_2D;
			GameObject previewPrefab = OfferItemPrefabs.GetPrefab(item.type, preferredPreviewType);
			if(previewPrefab == null) {
				// Loop will stop with a valid prefab
				for(int i = 0; i < (int)OfferItemPrefabs.PrefabType.COUNT && previewPrefab == null; ++i) {
					// Skip preferred type (already checked)
					if(i == (int)preferredPreviewType) continue;
					previewPrefab = OfferItemPrefabs.GetPrefab(item.type, (OfferItemPrefabs.PrefabType)i);
				}
			}

			// Instantiate preview! :)
			if(previewPrefab != null) {
				GameObject previewInstance = GameObject.Instantiate<GameObject>(previewPrefab);
				m_preview = previewInstance.GetComponent<IOfferItemPreview>();
			}
		}

		// Initialize preview with item data
		if(m_preview != null) {
			m_preview.InitFromItem(m_item);
			m_preview.SetParentAndFit(m_previewContainer as RectTransform);
		}

		// Set text - preview will given us the text already localized and all
		if(m_preview != null) {
			m_text.text = m_preview.GetLocalizedDescription();
		} else {
			// Something went very wrong :s
			m_text.text = "Couldn't find a preview prefab for reward type " + item.type;
		}

		// Text color based on item rarity!
		Gradient4 rarityGradient = null;
		if(m_item is Metagame.RewardPet && m_item.reward != null) {
			rarityGradient = UIConstants.GetRarityTextGradient(m_item.reward.rarity);
		} else {
			rarityGradient = UIConstants.GetRarityTextGradient(Metagame.Reward.Rarity.COMMON);
		}
		m_text.enableVertexGradient = true;
		m_text.colorGradient = new VertexGradient(
			rarityGradient.topLeft,
			rarityGradient.topRight,
			rarityGradient.bottomLeft,
			rarityGradient.bottomRight
		);

		// Info button - depends on preview type
		if(m_infoButton != null) {
			if(m_preview != null) {
				m_infoButton.SetActive(m_preview.showInfoButton);
			} else {
				m_infoButton.SetActive(false);
			}
		}

		// Power info - only for some types
		if(m_powerIcon != null) {
			DefinitionNode powerDef = null;
			switch(reward.type) {
				case Metagame.RewardPet.TYPE_CODE: {
					// Get the pet preview
					DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, reward.sku);
					if(petDef != null) {
						powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
					}
				} break;

				case Metagame.RewardSkin.TYPE_CODE: {
					DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, reward.sku);
					if(skinDef != null) {
						powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup"));
					}
				} break;

				default: {
					// No power to be displayed :)
				} break;
			}

			// Show?
			m_powerIcon.gameObject.SetActive(powerDef != null);

			// Initialize
			m_powerIcon.InitFromDefinition(powerDef, false);
		}
	}

	/// <summary>
	/// Destroy current preview, if any.
	/// </summary>
	private void ClearPreview() {
		if(m_preview != null) {
			Destroy(m_preview.gameObject);
			m_preview = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, refresh texts.
	/// </summary>
	private void OnLanguageChanged() {
		// Reapply current reward
		InitFromItem(m_item);
	}

	/// <summary>
	/// Info button has been pressed.
	/// </summary>
	public void OnInfoButton() {
		// If we have a valid preview, and this one supports info button, propagate the event
		if(m_preview != null && m_preview.showInfoButton) {
			m_preview.OnInfoButton();
		}
	}
}