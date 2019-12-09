// OfferItemPrefabs.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/03/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class.
/// </summary>
[System.Serializable]
public class ItemPrefabSetup {
	[List(
        Metagame.RewardRemoveAds.TYPE_CODE,
        Metagame.RewardSoftCurrency.TYPE_CODE,
		Metagame.RewardHardCurrency.TYPE_CODE,
		Metagame.RewardGoldenFragments.TYPE_CODE,
		Metagame.RewardEgg.TYPE_CODE,
		Metagame.RewardPet.TYPE_CODE,
		Metagame.RewardSkin.TYPE_CODE,
		Metagame.RewardDragon.TYPE_CODE
	)]
	public string type = Metagame.RewardSoftCurrency.TYPE_CODE;

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab2d = "";

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab3d = "";
}

/// <summary>
/// Auxiliar class.
/// </summary>
[System.Serializable]
public class OfferColorGradient {
	[Range(0f, 100f)]
	public float discountThreshold = 50f;
	public Gradient4 titleGradient = new Gradient4();
	public Gradient4 discountGradient = new Gradient4();
	public Gradient4 pillFrameGradient = new Gradient4();
	public Gradient4 pillBackgroundGradient = new Gradient4();
}

/// <summary>
/// Static scriptable object class to setup the offer item prefabs.
/// </summary>
//[CreateAssetMenu]
public class OfferItemPrefabs : SingletonScriptableObject<OfferItemPrefabs> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Metagame/Offers/OfferItemPrefabs";

	public enum PrefabType {
		PREVIEW_2D,
		PREVIEW_3D,

		COUNT
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	[SerializeField] private List<ItemPrefabSetup> m_itemTypesSetup = new List<ItemPrefabSetup>();
	[Separator]
	[SerializeField] private List<OfferColorGradient> m_gradientsSetup = new List<OfferColorGradient>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the prefab corresponding to a item type.
	/// </summary>
	/// <returns>The prefab for an item preview. <c>null</c> if type not defined or prefab type not defined for the target item type.</returns>
	/// <param name="_itemType">Item type.</param>
	/// <param name="_prefabType">Type of prefab to be returned.</param>
	public static GameObject GetPrefab(string _itemType, PrefabType _prefabType) {
		// Find item type setup
		ItemPrefabSetup setup = instance.m_itemTypesSetup.Find(
			(ItemPrefabSetup _setup) => {
				return _setup.type == _itemType;
			}
		);
		if(setup == null) return null;

		// Select the target prefab type
		switch(_prefabType) {
			case PrefabType.PREVIEW_2D: return Resources.Load<GameObject>(setup.prefab2d);
			case PrefabType.PREVIEW_3D: return Resources.Load<GameObject>(setup.prefab3d);
		}

		// Something went really wrong xD
		return null;
	}

	/// <summary>
	/// Get the gradient corresponding to a given discount amount.
	/// </summary>
	/// <returns>The gradient corresponding to the given discount amount.</returns>
	/// <param name="_discount">Discount (0..1).</param>
	public static OfferColorGradient GetGradient(float _discount) {
		// Convert to 0-100
		_discount = Mathf.Abs(_discount * 100f);

		// Make sure array is sorted
		List<OfferColorGradient> sorted = new List<OfferColorGradient>();
		sorted.AddRange(instance.m_gradientsSetup);
		sorted.Sort(
			(OfferColorGradient _item1, OfferColorGradient _item2) => {
				return _item1.discountThreshold.CompareTo(_item2.discountThreshold);
			}
		);

		// Find out threshold for this discount
		// Reverse iterate since threshold marks the min value
		for(int i = sorted.Count - 1; i >= 0; --i) {
			if(_discount >= sorted[i].discountThreshold) {
				return sorted[i];
			}
		}
		return new OfferColorGradient();
	}
}