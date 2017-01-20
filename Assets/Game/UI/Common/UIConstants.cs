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
public class UIConstants : SingletonScriptableObject<UIConstants> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Text Mesh Pro shortcuts
	[Separator("Text Mesh Pro shortcuts")]
	[SerializeField] private string m_tmpSpriteSC = "<sprite name=\"icon_sc\">";
	public static string TMP_SPRITE_SC {
		get { return instance.m_tmpSpriteSC; }
	}

	[SerializeField] private string m_tmpSpritePC = "<sprite name=\"icon_pc\">";
	public static string TMP_SPRITE_PC {
		get { return instance.m_tmpSpritePC; }
	}

	// Colors
	[Separator("Colors")]
	[SerializeField] private Color m_coinsTextColor = new Color(1f, 0.8f, 0.1f);
	public static Color COINS_TEXT_COLOR {
		get { return instance.m_coinsTextColor; }
	}

	[SerializeField] private Color m_pcTextColor = new Color(0.9f, 0.5f, 0.8f);
	public static Color PC_TEXT_COLOR {
		get { return instance.m_pcTextColor; }
	}

	[Space]
	[SerializeField] private Color m_rarityCommonColor = new Color(1f, 1f, 1f);		// White
	public static Color RARITY_COMMON_COLOR {
		get { return instance.m_rarityCommonColor; }
	}

	[SerializeField] private Color m_rarityRareColor = new Color(0.8f, 1f, 1f);		// Blue-ish
	public static Color RARITY_RARE_COLOR {
		get { return instance.m_rarityRareColor; }
	}

	[SerializeField] private Color m_rarityEpicColor = new Color(1f, 0.8f, 0.1f);	// Gold
	public static Color RARITY_EPIC_COLOR {
		get { return instance.m_rarityEpicColor; }
	}

	[SerializeField] private Color m_raritySpecialColor = new Color(1f, 0.5f, 0f);	// Orange
	public static Color RARITY_SPECIAL_COLOR {
		get { return instance.m_raritySpecialColor; }
	}

	// Asset paths in Resources
	[Separator("Asset paths in Resources")]
	[SerializeField] private string m_uiSpritesheetPath = "UI/Common/SpriteSheets/UI_spritesheet";
	public static string UI_SPRITESHEET_PATH {
		get { return instance.m_uiSpritesheetPath; }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the color corresponding to a given rarity.
	/// </summary>
	/// <returns>The rarity color.</returns>
	/// <param name="_rarity">The rarity to be checked.</param>
	public static Color GetRarityColor(EggReward.Rarity _rarity) {
		switch(_rarity) {
			case EggReward.Rarity.COMMON:	return RARITY_COMMON_COLOR;		break;
			case EggReward.Rarity.RARE:		return RARITY_RARE_COLOR;		break;
			case EggReward.Rarity.EPIC:		return RARITY_EPIC_COLOR;		break;
			case EggReward.Rarity.SPECIAL:	return RARITY_SPECIAL_COLOR;	break;
		}
		return Color.white;
	}

	/// <summary>
	/// Gets the color corresponding to a given rarity.
	/// </summary>
	/// <returns>The rarity color.</returns>
	/// <param name="_raritySku">The rarity to be checcked.</param>
	public static Color GetRarityColor(string _raritySku) {
		// Get rarity enum equivalent from def
		DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, _raritySku);
		if(rarityDef != null) {
			return GetRarityColor((EggReward.Rarity)rarityDef.GetAsInt("order"));
		}

		// Unknown rarity, return white
		return Color.white;
	}
}