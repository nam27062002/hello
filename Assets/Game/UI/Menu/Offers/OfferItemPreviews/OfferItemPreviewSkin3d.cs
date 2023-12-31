// OfferItemPreviewSkin3d.cs
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
public class OfferItemPreviewSkin3d : IOfferItemPreviewSkin {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public override Type type {
		get { return Type._3D; }
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	[SerializeField] private DragControl m_dragControl = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure dragon loader doesn't have any sku assigned
		m_dragonLoader.dragonSku = "";
		m_dragonLoader.onDragonLoaded += OnDragonLoaded;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_dragonLoader.onDragonLoaded -= OnDragonLoaded;
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

		// Initialize dragon loader with the target dragon and skin!
		if(m_def == null) {
			m_dragonLoader.LoadDragon("");
		} else {
			m_dragonLoader.LoadDragon(m_def.GetAsString("dragonSku"), m_def.sku);
		}

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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dragon has been loaded.
	/// </summary>
	/// <param name="_loader">Loader that triggered the event.</param>
	private void OnDragonLoaded(MenuDragonLoader _loader) {
		// Particle systems require a special initialization
		if (m_dragonLoader.dragonInstance != null)
		{
			InitParticles(m_dragonLoader.dragonInstance.gameObject);
		}
	}
}