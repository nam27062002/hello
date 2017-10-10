// UIConstants.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Text;
using TMPro;

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
	// Common icons in the UISpritesheet atlas
	public enum IconType {
		NONE,

		COINS,
		PC,
		GOLDEN_FRAGMENTS,
		KEYS
	}

	// Icon alignment type, relative to a text
	public enum IconAlignment {
		NONE,
		LEFT,
		RIGHT
	}

	//------------------------------------------------------------------------//
	// STATIC MEMBERS														  //
	//------------------------------------------------------------------------//
	// Text Mesh Pro shortcuts
	#region TMP_Shortcuts
	[SerializeField] private string m_tmpSpriteSC = "icon_sc";
	public static string TMP_SPRITE_SC {
		get { return instance.m_tmpSpriteSC; }
	}

	[SerializeField] private string m_tmpSpritePC = "icon_pc";
	public static string TMP_SPRITE_PC {
		get { return instance.m_tmpSpritePC; }
	}

	[SerializeField] private string m_tmpSpriteGoldenEggFragment = "icon_dragon_scale";
	public static string TMP_SPRITE_GOLDEN_EGG_FRAGMENT {
		get { return instance.m_tmpSpriteGoldenEggFragment; }
	}

	[SerializeField] private string m_tmpSpriteKeys = "icon_key";
	public static string TMP_SPRITE_KEY {
		get { return instance.m_tmpSpriteKeys; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Rarities
	#region Rarities
	[SerializeField] private Color[] m_rarityColors = new Color[(int)EggReward.Rarity.COUNT];
	public static Color[] RARITY_COLORS {
		get { return instance.m_rarityColors; }
	}

	[SerializeField] private Sprite[] m_rarityIcons = new Sprite[(int)EggReward.Rarity.COUNT];
	public static Sprite[] RARITY_ICONS {
		get { return instance.m_rarityIcons; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Asset paths in Resources
	#region AssetPaths
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

	[SerializeField] private string m_powerIconsPath = "UI/Metagame/Powers/";
	public static string POWER_ICONS_PATH {
		get { return instance.m_powerIconsPath; }
	}

	[SerializeField] private string m_powerMiniIconsPath = "UI/Common/Icons";
	public static string POWER_MINI_ICONS_PATH {
		get { return instance.m_powerMiniIconsPath; }
	}

	[SerializeField] private string m_shopIconsPath = "UI/Metagame/Shop/";
	public static string SHOP_ICONS_PATH {
		get { return instance.m_shopIconsPath; }
	}

	[SerializeField] private string m_languageIconsPath = "UI/Metagame/Settings/";
	public static string LANGUAGE_ICONS_PATH {
		get { return instance.m_languageIconsPath; }
	}

	[SerializeField] private string m_missionIconsPath = "UI/Metagame/Missions/";
	public static string MISSION_ICONS_PATH {
		get { return instance.m_missionIconsPath; }
	}

	[SerializeField] private string m_eggIconsPath = "UI/Metagame/Missions/";
	public static string EGG_ICONS_PATH {
		get { return instance.m_eggIconsPath; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Misc colors
	#region MiscColors
	[SerializeField] private Color m_colorErrorMessage = Color.red;
	public static Color ERROR_MESSAGE_COLOR {
		get { return instance.m_colorErrorMessage; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Pet category colors
	#region PetCategoryColors
	[SerializeField] private Color m_petCategoryColorEat = Color.white;
	[SerializeField] private Color m_petCategoryColorHealth = Color.white;
	[SerializeField] private Color m_petCategoryColorSpeed = Color.white;
	[SerializeField] private Color m_petCategoryColorScore = Color.white;
	[SerializeField] private Color m_petCategoryColorFire = Color.white;
	[SerializeField] private Color m_petCategoryColorDefense = Color.white;
	[SerializeField] private Color m_petCategoryColorSpecial = Color.white;
	[SerializeField] private Color m_petCategoryColorDefault = Color.white;

	public static Color PET_CATEGORY_EAT { get { return instance.m_petCategoryColorEat; }}
	public static Color PET_CATEGORY_HEALTH { get { return instance.m_petCategoryColorHealth; }}
	public static Color PET_CATEGORY_SPEED { get { return instance.m_petCategoryColorSpeed; }}
	public static Color PET_CATEGORY_SCORE { get { return instance.m_petCategoryColorScore; }}
	public static Color PET_CATEGORY_FIRE { get { return instance.m_petCategoryColorFire; }}
	public static Color PET_CATEGORY_DEFENSE { get { return instance.m_petCategoryColorDefense; }}
	public static Color PET_CATEGORY_SPECIAL { get { return instance.m_petCategoryColorSpecial; }}
	public static Color PET_CATEGORY_DEFAULT { get { return instance.m_petCategoryColorDefault; }}
	#endregion

	// -------------------------------------------------------------------------
	// Open Egg animation setup
	#region OpenEggAnimSetup
	[SerializeField] private float m_openEggSpinIntensity = 50f;
	public static float openEggSpinIntensity {
		get { return instance.m_openEggSpinIntensity; }
	}

	[SerializeField] private Ease m_openEggSpinEase = Ease.OutExpo;
	public static Ease openEggSpinEase {
		get { return instance.m_openEggSpinEase;}
	}
	#endregion

	// -------------------------------------------------------------------------
	// Results animation setup
	#region ResultsAnimSetup
	[SerializeField] private float m_resultsIntroDuration = 0.25f;
	public static float resultsIntroDuration {
		get { return instance.m_resultsIntroDuration; }
	}
	
	[Tooltip("In seconds, will be increased in the case of high score")]
	[SerializeField] private float m_resultsPanelDuration = 1f;
	public static float resultsPanelDuration {
		get { return instance.m_resultsPanelDuration; }
	}
	
	[Tooltip("Units per second, the whole bar is 1 unit")]
	[SerializeField] private float m_resultsXPBarSpeed = 0.15f;
	public static float resultsXPBarSpeed {
		get { return instance.m_resultsXPBarSpeed; }
	}

	[Tooltip("Minimum and maximum duration of the XP bar animation. Speed will be adjusted accordingly.")]
	[SerializeField] private Range m_resultsXPBarMinMaxDuration = new Range(1f, 3f);
	public static Range resultsXPBarMinMaxDuration {
		get { return instance.m_resultsXPBarMinMaxDuration; }
	}

	[SerializeField] private float m_resultsChestsAndEggMinDuration = 1f;
	public static float resultsChestsAndEggMinDuration {
		get { return instance.m_resultsChestsAndEggMinDuration; }
	}

	[SerializeField] private float m_resultsChestDuration = 0.5f;
	public static float resultsChestDuration {
		get { return instance.m_resultsChestDuration; }
	}

	[SerializeField] private float m_resultsEggDuration = 1f;
	public static float resultsEggDuration {
		get { return instance.m_resultsEggDuration; }
	}
	#endregion

	//------------------------------------------------------------------------//
	// NON-STATIC MEMBERS													  //
	//------------------------------------------------------------------------//
	// Internal
	private StringBuilder m_sb = new StringBuilder();

    //------------------------------------------------------------------------//
    // STATIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Gets the color corresponding to a given rarity.
    /// </summary>
    /// <returns>The rarity color.</returns>
    /// <param name="_rarity">The rarity to be checked.</param>
	public static Color GetRarityColor(Metagame.Reward.Rarity _rarity) {
		return RARITY_COLORS[(int)_rarity];
	}

	/// <summary>
	/// Gets the color corresponding to a given rarity.
	/// </summary>
	/// <returns>The rarity color.</returns>
	/// <param name="_raritySku">The rarity to be checcked.</param>
	public static Color GetRarityColor(string _raritySku) {
		// Get rarity enum equivalent from sku
		return GetRarityColor(Metagame.Reward.SkuToRarity(_raritySku));
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

	/// <summary>
	/// Get the color assigned to a specific pet category.
	/// </summary>
	/// <returns>The color linked to the requested pet category.</returns>
	/// <param name="_categorySku">Sku of the pet category, as defined in the PET_CATEGORY_DEFINITIONS table.</param>
	public static Color GetPetCategoryColor(string _categorySku) {
		switch(_categorySku) {
			// [AOC] Not beautiful, but enough
			case "eat":		return instance.m_petCategoryColorEat; break;
			case "health":	return instance.m_petCategoryColorHealth; break;
			case "speed":	return instance.m_petCategoryColorSpeed; break;
			case "score":	return instance.m_petCategoryColorScore; break;
			case "fire":	return instance.m_petCategoryColorFire; break;
			case "defense":	return instance.m_petCategoryColorDefense; break;
			case "special":	return instance.m_petCategoryColorSpecial; break;
			default: 		return instance.m_petCategoryColorDefault; break;
		}
	}

	/// <summary>
	/// Get the name (sprite id within the atlas) for the given icon type.
	/// </summary>
	/// <returns>The name of the requested icon type within the atlas.</returns>
	/// <param name="_icon">Icon whose name we want.</param>
	public static string GetIconName(IconType _icon) {
		switch(_icon) {
			case IconType.COINS: 			return UIConstants.TMP_SPRITE_SC;
			case IconType.PC: 				return UIConstants.TMP_SPRITE_PC;
			case IconType.GOLDEN_FRAGMENTS: return UIConstants.TMP_SPRITE_GOLDEN_EGG_FRAGMENT;
			case IconType.KEYS:				return UIConstants.TMP_SPRITE_KEY;
		}
		return string.Empty;
	}
		
	/// <summary>
	/// Create a composite string consisting of a text and an icon.
	/// Specially useful for currency counters and price tags.
	/// </summary>
	/// <returns>The full string.</returns>
	/// <param name="_text">Text to be attached, typically a formatted amount. <see cref="StringUtils"/>.</param>
	/// <param name="_icon">Icon to be attached..</param>
	/// <param name="_alignment">Position of the icon relative to the text.</param>
	public static string GetIconString(string _text, IconType _icon, IconAlignment _alignment) {
		// Figure out icon string
		string iconString = string.Empty;
		switch(_icon) {
			case IconType.COINS:
			case IconType.PC:
			case IconType.GOLDEN_FRAGMENTS:
			case IconType.KEYS: {
				iconString = GetSpriteTag(GetIconName(_icon));
			} break;

			default: {
				iconString = string.Empty;
			} break;
		}

		// Compose final string with the proper alignment
		instance.m_sb.Length = 0;	// Reset string builder
		switch(_alignment) {
			case IconAlignment.NONE: {
				instance.m_sb.Append(_text);
			} break;

			case IconAlignment.LEFT: {
				if(!string.IsNullOrEmpty(iconString)) {
					instance.m_sb.Append(iconString);//.Append(" ");
				}
				instance.m_sb.Append(_text);
			} break;

			case IconAlignment.RIGHT: {
				instance.m_sb.Append(_text);
				if(!string.IsNullOrEmpty(iconString)) {
					instance.m_sb/*.Append(" ")*/.Append(iconString);
				}
			} break;
		}

		// Done!
		return instance.m_sb.ToString();
	}

	/// <summary>
	/// Create a composite string consisting of a number and an icon.
	/// Specially useful for currency counters and price tags.
	/// </summary>
	/// <returns>The full string.</returns>
	/// <param name="_amount">Number to be attached, will be formatted with the default format using <see cref="StringUtils"/>.</param>
	/// <param name="_icon">Icon to be attached..</param>
	/// <param name="_alignment">Position of the icon relative to the text.</param>
	public static string GetIconString(long _amount, IconType _icon, IconAlignment _alignment) {
		// Just use text version with the formatted string
		return GetIconString(StringUtils.FormatNumber(_amount), _icon, _alignment);
	}

	/// <summary>
	/// Create a composite string consisting of a number and a currency icon.
	/// Specially useful for currency counters and price tags.
	/// </summary>
	/// <returns>The full string.</returns>
	/// <param name="_amount">Number to be attached, will be formatted with the default format using <see cref="StringUtils"/>.</param>
	/// <param name="_currency">Displayed currency.</param>
	/// <param name="_alignment">Position of the icon relative to the text.</param>
	public static string GetIconString(long _amount, UserProfile.Currency _currency, IconAlignment _alignment) {
		// Convert currency to icon and use the other constructors
		return GetIconString(_amount, GetCurrencyIcon(_currency), _alignment);
	}

	/// <summary>
	/// Get the icon corresponding to a currency.
	/// </summary>
	/// <returns>The currency icon.</returns>
	/// <param name="_currency">Currency type.</param>
	public static IconType GetCurrencyIcon(UserProfile.Currency _currency) {
		IconType icon = IconType.NONE;
		switch(_currency) {
			case UserProfile.Currency.SOFT:				icon = IconType.COINS;				break;
			case UserProfile.Currency.HARD:				icon = IconType.PC;					break;
			case UserProfile.Currency.GOLDEN_FRAGMENTS:	icon = IconType.GOLDEN_FRAGMENTS;	break;
			case UserProfile.Currency.KEYS:				icon = IconType.KEYS;				break;
		}
		return icon;
	}

	/// <summary>
	/// Get the sprite corresponding to a given icon.
	/// </summary>
	/// <returns>The icon sprite.</returns>
	/// <param name="_icon">Icon whose sprite we want.</param>
	public static Sprite GetIconSprite(IconType _icon) {
		// TMPro has the sprites indexed for us and provide us the tools get them!
		int spriteIdx = TMP_Settings.defaultSpriteAsset.GetSpriteIndexFromName(GetIconName(_icon));
		if(spriteIdx < 0) return null;

		// Just get the sprite at the given index!
		return TMP_Settings.defaultSpriteAsset.spriteInfoList[spriteIdx].sprite;
	}

	/// <summary>
	/// Create a sprite tag to be inserted in a TMPro text.
	/// </summary>
	/// <returns>The full tag with the given sprite: "<sprite name="_spriteName">".</returns>
	/// <param name="_spriteName">ID of the sprite we want to show. Same ID as in the UI spritesheet asset.</param>
	public static string GetSpriteTag(string _spriteName) {
		// Clear stringbuilder
		instance.m_sb.Length = 0;

		// Do the composition
		instance.m_sb.Append("<sprite name=\"").Append(_spriteName).Append("\">");

		// Done!
		return instance.m_sb.ToString();
	}
}