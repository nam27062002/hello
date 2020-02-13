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
		Metagame.RewardEgg.TYPE_CODE,
		Metagame.RewardPet.TYPE_CODE,
		Metagame.RewardSkin.TYPE_CODE,
		Metagame.RewardDragon.TYPE_CODE
	)]
	[HideInInspector] public string type = Metagame.RewardSoftCurrency.TYPE_CODE;

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab2d = "";

	[FileList("Resources/UI/Metagame/Offers", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	public string prefab3d = "";
}


[System.Serializable]
public class ShopPill {

    [List(
			OfferPack.PROGRESSION,
			OfferPack.PUSH,
			OfferPack.ROTATIONAL,
			OfferPack.FREE,
            OfferPack.REMOVE_ADS,
            OfferPack.SC
    )]
    public string offerPackType;

    [FileList("Resources/UI/Shop", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
    public string prefab = "";


}


/// <summary>
/// Static scriptable object class to setup the offer item prefabs.
/// </summary>
//[CreateAssetMenu]
public class ShopSettings : SingletonScriptableObject<ShopSettings> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Metagame/Offers/ShopSettings";

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	[Separator("Item Types Setup")]
	[SerializeField] private List<ItemPrefabSetup> m_itemTypesSetup = new List<ItemPrefabSetup>();

	[Separator("Pill prefabs per shop category")]
	[InfoBox("HC pills are defined in its category containers")]
    [SerializeField] private List<ShopPill> m_shopPillsSetup = new List<ShopPill>();

	[Separator("HC Icons")]
	[Comment("These icons are linked to the order of the pack in the shop")]
    [SerializeField] private List<IOfferItemPreviewHC> m_HcIconPrefabs = new List<IOfferItemPreviewHC>();

	[Separator("SC Icons")]
    [Comment("These icons are linked to the order of the pack in the shop")]
    [SerializeField] private List<IOfferItemPreviewSC> m_ScIconPrefabs = new List<IOfferItemPreviewSC>();

	[Separator("Paths")]
	[SerializeField] private string m_shopTooltipPath = "UI/Shop/PF_ShopTooltip";
	public static string shopTooltipPath { get { return instance.m_shopTooltipPath; }}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the prefab corresponding to a item type.
	/// </summary>
	/// <returns>The prefab for an item preview. <c>null</c> if type not defined or prefab type not defined for the target item type.</returns>
	/// <param name="_itemType">Item type.</param>
	/// <param name="_previewType">Type of prefab to be returned.</param>
	public static GameObject GetPrefab(string _itemType, IOfferItemPreview.Type _previewType) {

		// Find item type setup
		ItemPrefabSetup setup = instance.m_itemTypesSetup.Find(
			(ItemPrefabSetup _setup) => {
				return _setup.type == _itemType;
			}
		);
		if(setup == null) return null;

		// Select the target prefab type
		switch(_previewType) {
			case IOfferItemPreview.Type._2D: return Resources.Load<GameObject>(setup.prefab2d);
			case IOfferItemPreview.Type._3D: return Resources.Load<GameObject>(setup.prefab3d);
		}

		// Something went really wrong xD
		return null;
	}

    /// <summary>
    /// Returns the pill prefab associated to an offer pack type
    /// </summary>
    /// <param name="_type">Type of the offer pack</param>
    /// <returns></returns>
    public static IShopPill GetPrefab (OfferPack.Type _type)
    {
        // Find the offer pill setup
        ShopPill pill = instance.m_shopPillsSetup.Find(p => p.offerPackType == OfferPack.TypeToString(_type));

        if (pill!= null)
        {
            return Resources.Load<IShopPill> (pill.prefab);
        }

        return null;
    }

    /// <summary>
    /// Get the proper Gems pack icon based on its order
    /// </summary>
    /// <param name="_order"></param>
    /// <returns></returns>
    public static IOfferItemPreviewHC GetHcIconPrefab (int _order)
    {
        return instance.m_HcIconPrefabs[_order];
    }

    /// <summary>
    /// Get the proper coins pack icon based on its order
    /// </summary>
    /// <param name="_order"></param>
    /// <returns></returns>
    public static IOfferItemPreviewSC GetScIconPrefab(int _order)
    {
        return instance.m_ScIconPrefabs[_order];
    }

}