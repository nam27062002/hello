// XPromoDynamicLinksCollection.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 15/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
//[CreateAssetMenu(fileName = "XPromoDynamicLinksCollectionAsset", menuName = "XPromoDynamicLinksCollection", order = 2)]
public class XPromoDynamicLinksCollection : SingletonScriptableObject<XPromoDynamicLinksCollection>
{
	//------------------------------------------------------------------------//
	// ENUM															  //
	//------------------------------------------------------------------------//

	public enum ABGroup
	{
		A, B
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

    public List<XPromoRewardShortLink> xPromoShortLinks;


	//------------------------------------------------------------------------//
	// INNER CLASSES        												  //
	//------------------------------------------------------------------------//

	[System.Serializable]
	public class XPromoRewardShortLink
	{
		public ABGroup abGroup;
		public string rewardSKU;
		public string url;
	}

}

	