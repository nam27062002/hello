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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Header("Settings")]
	[SerializeField] protected Type m_type = Type.PILL_BIG;
	[SerializeField] protected bool m_allow3dPreview = false;   // [AOC] In some cases, we want to display a 3d preview when appliable (pets/eggs)

	[Header("Mandatory Fields")]
	[SerializeField] protected Transform m_previewContainer = null;
	[SerializeField] protected TextMeshProUGUI m_mainText = null;
	[SerializeField] protected TextMeshProUGUI m_secondaryText = null;
	[SerializeField] protected TextMeshProUGUI m_amountText = null;

	[Header("Optional Fields")]
	[SerializeField] protected TextMeshProUGUI m_descriptionText = null;
	[SerializeField] protected PowerIcon m_powerIcon = null;	// Will only be displayed for some types
	[SerializeField] protected Image m_tierIcon = null;		// Will only be displayed for some types

	// Convenience properties
	public RectTransform rectTransform {
		get { return this.transform as RectTransform; }
	}

	// Internal
	protected OfferPackItem m_item = null;
	public OfferPackItem item {
		get { return m_item; }
		set { InitFromItem(value); }
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
	/// <param name="_item">Item to be used to initialize the slot.</param>
	public virtual void InitFromItem(OfferPackItem _item) {
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
			ShopSettings.PrefabType preferredPreviewType = m_allow3dPreview ? ShopSettings.PrefabType.PREVIEW_3D : ShopSettings.PrefabType.PREVIEW_2D;
			GameObject previewPrefab = ShopSettings.GetPrefab(item.type, preferredPreviewType);
			if(previewPrefab == null) {
				// Loop will stop with a valid prefab
				for(int i = 0; i < (int)ShopSettings.PrefabType.COUNT && previewPrefab == null; ++i) {
					// Skip preferred type (already checked)
					if(i == (int)preferredPreviewType) continue;
					previewPrefab = ShopSettings.GetPrefab(item.type, (ShopSettings.PrefabType)i);
				}
			}

			// Instantiate preview! :)
			if(previewPrefab != null) {
				GameObject previewInstance = GameObject.Instantiate<GameObject>(previewPrefab);
				previewInstance.SetActive(true);
				m_preview = previewInstance.GetComponent<IOfferItemPreview>();
			}
		}

		// Initialize preview with item data
		if(m_preview != null) {
			m_preview.InitFromItem(m_item);
			m_preview.SetParentAndFit(m_previewContainer as RectTransform);
		}

		// Set text - preview will given us the text already localized and all
		if(m_mainText != null) {
			if(m_preview != null) {
				m_mainText.text = m_preview.GetLocalizedDescription();
			} else {
				// Something went very wrong :s
				m_mainText.text = "Couldn't find a preview prefab for reward type " + item.type;
			}

			// Text color based on item rarity!
			Gradient4 rarityGradient = null;
			if(m_item.type == Metagame.RewardPet.TYPE_CODE && m_item.reward != null) {
				rarityGradient = UIConstants.GetRarityTextGradient(m_item.reward.rarity);
			} else {
				rarityGradient = UIConstants.GetRarityTextGradient(Metagame.Reward.Rarity.COMMON);
			}
			m_mainText.enableVertexGradient = true;
			m_mainText.colorGradient = new VertexGradient(
				rarityGradient.topLeft,
				rarityGradient.topRight,
				rarityGradient.bottomLeft,
				rarityGradient.bottomRight
			);
		}

		// Description and Power Icon
		// [AOC] TODO!! All types must have description now, move it to the ItemPreview classes
		string localizedDescription = "";
		DefinitionNode powerDef = null;
		PowerIcon.Mode powerIconMode = PowerIcon.Mode.PET;
		switch(reward.type) {
			// Pet
			case Metagame.RewardPet.TYPE_CODE: {
				// Power description
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, reward.sku);
				if(petDef != null) {
					powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
					powerIconMode = PowerIcon.Mode.PET;
					localizedDescription = DragonPowerUp.GetDescription(powerDef, true, true);   // Custom formatting depending on powerup type, already localized
				}
			} break;

			// Skin
			case Metagame.RewardSkin.TYPE_CODE: {
				// Power description
				DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, reward.sku);
				if(skinDef != null) {
					powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup"));
					powerIconMode = PowerIcon.Mode.SKIN;
					localizedDescription = DragonPowerUp.GetDescription(powerDef, true, false);   // Custom formatting depending on powerup type, already localized
				}
			} break;

			// Dragon
			case Metagame.RewardDragon.TYPE_CODE: {
				// Dragon description
				localizedDescription = reward.def.GetLocalized("tidDesc");
			} break;

			default: {
				// No extra text to be displayed :)
			} break;
		}

		if(m_descriptionText != null) {
			m_descriptionText.text = localizedDescription;
		}

		if(m_powerIcon != null) {
			bool show = powerDef != null;
			m_powerIcon.gameObject.SetActive(show);
			if(show) {
				m_powerIcon.InitFromDefinition(powerDef, false, false, powerIconMode);
			}
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
}