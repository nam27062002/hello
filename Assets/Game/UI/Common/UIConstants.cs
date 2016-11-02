// UIConstants.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Static class for some global UI constants and utils.
/// </summary>
public static class UIConstants {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Text Mesh Pro shortcuts
	public const string TMP_SPRITE_SC = "<sprite name=\"icon_sc\">";
	public const string TMP_SPRITE_PC = "<sprite name=\"icon_pc\">";

	// Colors
	public static readonly Color COINS_TEXT_COLOR = new Color(1f, 0.8f, 0.1f);
	public static readonly Color PC_TEXT_COLOR = new Color(0.9f, 0.5f, 0.8f);

	// Asset paths in Resources
	public const string UI_SPRITESHEET_PATH = "UI/Common/SpriteSheets/UI_spritesheet";
}