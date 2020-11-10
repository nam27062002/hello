// DailyRewardViewSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Scriptable object to store the gradient configuration for Daily Reward views.
/// Have to do this since custom properties (Gradient4) are not animatable by Unity -_-
/// </summary>
//[CreateAssetMenu]

public class BoostedDailyRewardViewSettings : DailyRewardViewSettings {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Metagame/Rewards/BoostedDailyRewardViewSettings";

}