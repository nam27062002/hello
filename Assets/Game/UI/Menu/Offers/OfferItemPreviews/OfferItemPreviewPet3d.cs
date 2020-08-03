// OfferItemPreviewPet3d.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to encapsulate the preview of an item.
/// </summary>
public class OfferItemPreviewPet3d : IOfferItemPreviewPet {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override Type type {
		get { return Type._3D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private MenuPetLoader m_petPreview = null;
	[SerializeField] private DragControl m_dragControl = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS                                                        //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
		m_petPreview.OnLoadingComplete.RemoveListener(OnLoadingComplete);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize preview with current item (m_item)
	/// </summary>
	protected override void InitInternal() {
		// Call parent
		base.InitInternal();

		// Initialize pet loader with the target pet preview!
		m_petPreview.Load(m_item.reward.sku);
		m_petPreview.OnLoadingComplete.AddListener(OnLoadingComplete);

		// Drag control only enabled in certain types of slots
		if(m_dragControl != null) {
			switch(m_slotType) {
				case OfferItemSlot.Type.POPUP_BIG: {
					m_dragControl.gameObject.SetActive(true);
				} break;

				default: {
					m_dragControl.gameObject.SetActive(false);
				} break;
			}
		}
	}

	/// <summary>
	/// Set this preview's parent and adjust its size to fit it.
	/// </summary>
	/// <param name="_t">New parent!</param>
	public override void SetParentAndFit(RectTransform _t) {
		// Let parent do its thing
		base.SetParentAndFit(_t);

		// Refresh particle scaler
		m_petPreview.pscaler.DoScale();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS                                                              //
	//------------------------------------------------------------------------//
	private void OnLoadingComplete(MenuPetLoader _loader) {
		if(m_petPreview.petInstance != null) {
			InitParticles(m_petPreview.petInstance.gameObject);
			_loader.OnLoadingComplete.RemoveListener(OnLoadingComplete);
		}
	}
}