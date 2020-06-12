// ItemSizeSelector.cs
// Hungry Dragon
// 
// Created by JOM on 29/05/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This component choose the proper item preview in a shop pill according
/// to the size of the pill. This way we can use the pills in different layout groups
/// making sure that we display the proper preview according to the height of the pill.
/// </summary>
public class ItemSizeSelector : MonoBehaviour {
	//------------------------------------------------------------------------//
	// ENUM															  //
	//------------------------------------------------------------------------//

    private enum Size
    {
        SMALL,
        LARGE
    }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
    [Comment("Chooses the proper item preview in a shop pill according to the size of the pill.")]
	[SerializeField]
	private ShopMonoRewardPill m_shopPill;

	[SerializeField]
	private OfferItemSlot m_smallPreview;

	[SerializeField]
	private OfferItemSlot m_largePreview;

	[SerializeField]
    [Tooltip("Shop pills with height lower than this, will show the small preview, in other case will show the large preview")]
	private float m_pillHeightThreshold;

	// Cache
	private RectTransform m_pillRect;

	// Internal
	private Size m_curentSize = Size.LARGE;
	private Size newSize;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Start() {


		m_pillRect = m_shopPill.GetComponent<RectTransform>();
		UpdatePill();


	}

	/// <summary>
	/// This method is call everytime the transform size changes
	/// </summary>
	private void OnRectTransformDimensionsChange()
    {
		newSize = (m_pillRect.sizeDelta.y >= m_pillHeightThreshold) ? Size.LARGE : Size.SMALL;

		// Did the size changed?
		if (newSize != m_curentSize)
		{
			m_curentSize = newSize;

			UpdatePill();

		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    public void UpdatePill ()
    {

        if (m_largePreview != null)
        {
			m_largePreview.gameObject.SetActive(m_curentSize == Size.LARGE);

            if (m_curentSize == Size.LARGE)
				m_shopPill.offerItemSlot = m_largePreview;

		}

		if (m_smallPreview != null)
		{
			m_smallPreview.gameObject.SetActive(m_curentSize == Size.SMALL);

			if (m_curentSize == Size.SMALL)
				m_shopPill.offerItemSlot = m_smallPreview;

		}
               
	}

}