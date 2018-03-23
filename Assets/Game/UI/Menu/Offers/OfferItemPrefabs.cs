﻿// OfferItemPrefabs.cs
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
		Metagame.RewardSoftCurrency.TYPE_CODE,
		Metagame.RewardHardCurrency.TYPE_CODE,
		Metagame.RewardGoldenFragments.TYPE_CODE,
		Metagame.RewardEgg.TYPE_CODE,
		Metagame.RewardPet.TYPE_CODE,
		Metagame.RewardSkin.TYPE_CODE
	)]
	public string type = Metagame.RewardSoftCurrency.TYPE_CODE;

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab2d = "";

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab3d = "";
}

/// <summary>
/// Static scriptable object class to setup the offer item prefabs.
/// </summary>
[CreateAssetMenu]
public class OfferItemPrefabs : SingletonScriptableObject<OfferItemPrefabs> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Metagame/Offers/OfferItemPrefabs";

	public enum PrefabType {
		PREVIEW_2D,
		PREVIEW_3D
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	[SerializeField] private List<ItemPrefabSetup> m_itemTypesSetup = new List<ItemPrefabSetup>();

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
}