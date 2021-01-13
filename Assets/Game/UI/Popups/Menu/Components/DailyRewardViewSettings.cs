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
public class DailyRewardViewSettings : ScriptableObject {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Metagame/Rewards/DailyRewardViewSettings";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public Gradient4 defaultGradient = new Gradient4();
	public Gradient4 cooldownGradient = new Gradient4();
	public Gradient4 currentGradient = new Gradient4();
	public Gradient4 collectedGradient = new Gradient4();
	public Gradient4 specialGradient = new Gradient4();
}