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

	[SerializeField] private string m_disguiseIconsPath = "UI/Metagame/Disguises/";
	public static string DISGUISE_ICONS_PATH {
		get { return instance.m_disguiseIconsPath; }
	}

	[SerializeField] private string m_petIconsPath = "UI/Metagame/Pets/Icons/";
	public static string PET_ICONS_PATH {
		get { return instance.m_petIconsPath; }
	}

	[SerializeField] private string m_powerIconsPath = "UI/Metagame/Powers/icons_powers";
	public static string POWER_ICONS_PATH {
		get { return instance.m_powerIconsPath; }
	}

	[SerializeField] private string m_powerMiniIconsPath = "UI/Metagame/Powers/icons_mini_powers";
	public static string POWER_MINI_ICONS_PATH {
		get { return instance.m_powerMiniIconsPath; }
	}

	// More colors
	[Separator("Powerups Colors")]
	[SerializeField] private Color m_powerColorHealth = new Color(0.7f, 0.8f, 0.24f);
	public static Color POWER_COLOR_HEALTH {
		get { return instance.m_powerColorHealth; }
	}

	[SerializeField] private Color m_powerColorBoost = new Color(0.38f, 1f, 0.78f);
	public static Color POWER_COLOR_BOOST {
		get { return instance.m_powerColorBoost; }
	}

	[SerializeField] private Color m_powerColorFire = new Color(1f, 0.64f, 0.22f);
	public static Color POWER_COLOR_FIRE {
		get { return instance.m_powerColorFire; }
	}

	[SerializeField] private Color m_powerColorBite = new Color(1f, 0.5f, 0.44f);
	public static Color POWER_COLOR_BITE {
		get { return instance.m_powerColorBite; }
	}

	[SerializeField] private Color m_powerColorSpeed = new Color(0.38f, 1f, 0.78f);
	public static Color POWER_COLOR_SPEED {
		get { return instance.m_powerColorSpeed; }
	}

	[SerializeField] private Color m_powerColorGold = new Color(1f, 0.71f, 0.1f);
	public static Color POWER_COLOR_GOLD {
		get { return instance.m_powerColorGold; }
	}

	[SerializeField] private Color m_powerColorPC = new Color(0.96f, 0.46f, 1f);
	public static Color POWER_COLOR_PC {
		get { return instance.m_powerColorPC; }
	}

	[SerializeField] private Color m_powerColorMine = new Color(0.63f, 0.63f, 0.63f);
	public static Color POWER_COLOR_MINE {
		get { return instance.m_powerColorMine; }
	}

	[SerializeField] private Color m_powerColorPoison = new Color(0f, 1f, 0f);
	public static Color POWER_COLOR_POISON {
		get { return instance.m_powerColorPoison; }
	}

	[SerializeField] private Color m_powerColorObstacle = new Color(0.87f, 0.67f, 0.20f);
	public static Color POWER_COLOR_OBSTACLE {
		get { return instance.m_powerColorObstacle; }
	}

	[SerializeField] private Color m_powerColorEntity = new Color(1f, 1f, 1f);
	public static Color POWER_COLOR_ENTITY {
		get { return instance.m_powerColorEntity; }
	}

    [SerializeField]
    private Color m_powerColorWater = new Color(0f, 0.91f, 1f);
    public static Color POWER_COLOR_WATER
    {
        get { return instance.m_powerColorWater; }
    }

    [SerializeField]
    private Color m_powerColorDrain = new Color(0f, 0.91f, 1f);
    public static Color POWER_COLOR_DRAIN
    {
        get { return instance.m_powerColorDrain; }
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

	/// <summary>
	/// Gets the special color assigned to the given power type.
	/// </summary>
	/// <returns>The color assigned to the power's type.</returns>
	/// <param name="_powerDef">Power to be consulted.</param>
	public static Color GetPowerColor(string _powerSku) {
		// Get definition
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
		if(def != null) {
			return DragonPowerUp.GetColor(def);
		}

		// Unknown power, return white
		return Color.white;
	}
}