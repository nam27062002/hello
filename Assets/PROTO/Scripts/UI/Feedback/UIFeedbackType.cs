// UIFeedbackType.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Add any new feedback type in here and fill up its setup in the Feedback Spawner object's inspector window.
/// </summary>
public enum EUIFeedbackType {
	SCORE,
	MULTIPLIER,
	MULTIPLIER_MESSAGE,
	EAT,
	BURN,
	DAMAGE_RECEIVED,
	STARVING,
	COLLECTIBLE
}

/// <summary>
/// To define visuals for each type.
/// </summary>
[Serializable]
public class UIFeedbackType {
	[HideInInspector] public EUIFeedbackType type;
	public uint poolSize;
	public GameObject prefab;
}
#endregion