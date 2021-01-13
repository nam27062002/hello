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

using System.Collections.Generic;

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
	public enum Type {
		_2D,
		_3D,

		COUNT
	}

	// Abstract
	public abstract IOfferItemPreview.Type type {
		get;
	}

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
	protected OfferItemSlot.Type m_slotType = OfferItemSlot.Type.PILL_BIG;

	// Coroutine pointer used to stop the coroutine when object is destroyed
	private Coroutine m_delayedSetParentAndFit = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	///
	/// OnDestroy
	/// We stop delayed coroutine to avoid accesing an object that was destroyed
	void OnDestroy() {
		if(m_delayedSetParentAndFit != null) {
			StopCoroutine(m_delayedSetParentAndFit);
			m_delayedSetParentAndFit = null;
		}
	}

	/// <summary>
	/// Initialize the widget with the data of a specific offer item.
	/// </summary>
	/// <param name="_item">Item.</param>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	public void InitFromItem(OfferPackItem _item, OfferItemSlot.Type _slotType) {
		// Store new item and slot type
		m_item = _item;
		m_slotType = _slotType;

		Debug.Assert(m_item != null && m_item.reward != null, "ITEM NOT PROPERLY INITIALIZED", this);
		m_def = m_item.reward.def;

		// Call internal initializer
		InitInternal();
	}

	/// <summary>
	/// Set this preview's parent and adjust its size to fit it.
	/// </summary>
	/// <param name="_t">New parent!</param>
	public virtual void SetParentAndFit(RectTransform _t) {
		// Delay by one frame to make sure rect transforms are properly initialized
		m_delayedSetParentAndFit = UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
			m_delayedSetParentAndFit = null;

			// Add several null checks to prevent exceptions reported by Crashlytics
			// https://console.firebase.google.com/project/hungry-dragon-45530774/crashlytics/app/android:com.ubisoft.hungrydragon/issues/5c11ba3ef8b88c29639bbaf8?time=last-seven-days&sessionId=5DDE1FF902250001769E076D6CC4452B_DNE_2_v2
			if(_t != null) {
				// Set parent
				this.transform.SetParent(_t, false);

				// Adjust scale
				RectTransform rt = rectTransform;
				if(rt != null) {
					float sx = _t.rect.width / Mathf.Max(rt.rect.width, float.Epsilon);      // Prevent division by 0
					float sy = _t.rect.height / Mathf.Max(rt.rect.height, float.Epsilon);    // Prevent division by 0
					float scale = (sx < sy) ? sx : sy;
					rt.localScale = new Vector3(scale, scale, scale);
				}
			}

			// Scale particles as well
			ParticleScaler scaler = this.GetComponentInChildren<ParticleScaler>();
			if(scaler != null) {
				scaler.DoScale();
			}
		}, 1);
	}

	/// <summary>
	/// Process all particle systems of the preview so they work as expected.
	/// </summary>
	/// <param name="_rootObject">Object whose nested particle systems we want to initialize.</param>
	public virtual void InitParticles(GameObject _rootObject) {
		// Check params
		if(_rootObject == null) return;

		// Process all nested particle systems
		ParticleSystem[] nestedPS = _rootObject.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < nestedPS.Length; ++i) {
			// Aux vars
			ParticleSystem ps = nestedPS[i];
			if(ps == null) continue;

			// Disable VFX whenever a popup is opened in top of this preview (they don't render well with a popup on top)
			DisableOnPopup disabler = ps.gameObject.AddComponent<DisableOnPopup>();
			disabler.refPopupCount = PopupManager.openPopupsCount;

			// Start particle with a couple of frames of delay to give time for the particle scalers to be applied
			ps.gameObject.SetActive(false);
			UbiBCN.CoroutineManager.DelayedCallByFrames(() => {
				if(ps != null) ps.gameObject.SetActive(true);
			}, 5);
		}
	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATE METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the amount of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized amount. <c>null</c> if this item type doesn't have to show the amount for the given type of slot (i.e. dragon).</returns>
	public virtual string GetLocalizedAmountText(OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		return null;
	}

	/// <summary>
	/// Gets the main text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized main text. <c>null</c> if this item type doesn't have to show main text for the given type of slot (i.e. coins).</returns>
	public virtual string GetLocalizedMainText(OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		return null;
	}

	/// <summary>
	/// Gets the secondary text of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized secondary text. <c>null</c> if this item type doesn't have to show any secondary text for the given type of slot (i.e. coins).</returns>
	public virtual string GetLocalizedSecondaryText(OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		return null;
	}

	/// <summary>
	/// Gets the description of this item, already localized and formatted.
	/// </summary>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	/// <returns>The localized description. <c>null</c> if this item type doesn't have to show any description for the given type of slot.</returns>
	public virtual string GetLocalizedDescriptionText(OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		return null;
	}

	/// <summary>
	/// Initialize the given power icon instance with data from this reward.
	/// Will disable it item doesn't have a power assigned.
	/// </summary>
	/// <param name="_powerIcon">The power icon to be initialized.</param>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	public virtual void InitPowerIcon(PowerIcon _powerIcon, OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		// Disable by default
		_powerIcon.InitFromDefinition(null, null, false, false);	// This will do the trick
	}

	/// <summary>
	/// Initialize the given tier icon instance with data from this reward.
	/// Will disable it if reward type doesn't support tiers, as well as depending on the setup from offerSettings.
	/// </summary>
	/// <param name="_tierIconContainer">Where to instantiate the tier icon.</param>
	/// <param name="_slotType">The type of slot where the item will be displayed.</param>
	public virtual void InitTierIcon(GameObject _tierIconContainer, OfferItemSlot.Type _slotType) {
		// To be implemented by heirs if needed
		// Hide by default
		_tierIconContainer.SetActive(false);
	}

	/// <summary>
	/// Can the tier icon be displayed in the given slot type?
	/// </summary>
	/// <param name="_slotType">Type of slot to be checked.</param>
	/// <returns>Whether the tier icon should be displayed or not for the given slot type.</returns>
	protected bool ShowTierIconBySlotType(OfferItemSlot.Type _slotType) {
		// Get accepted slot types from settings
		DefinitionNode offerSettings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "offerSettings");
		List<string> acceptedSlots = offerSettings.GetAsList<string>("showTierInOffers");

		// Check if the given slot type is accepted
		return acceptedSlots.Contains(OfferItemSlot.TypeToString(_slotType));
	}

	/// <summary>
	/// Initialize the given tooltip with data from this reward.
	/// </summary>
	/// <param name="_tooltip">The tooltip to be initialized.</param>
	public virtual void InitTooltip(UITooltip _tooltip) {
		// Default implementation - show short text
		// Shouldn't get here anyway, since only rewards supporting tooltips should get this method invoked
		// To be overriden by heirs if needed
		_tooltip.InitWithText(
			GetLocalizedMainText(OfferItemSlot.Type.TOOLTIP),
			GetLocalizedDescriptionText(OfferItemSlot.Type.TOOLTIP)
		);
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected abstract void InitInternal();
}