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
		GOLDEN_FRAGMENTS
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
	[SerializeField] private string m_tmpSpriteSC = "<sprite name=\"icon_sc\">";
	public static string TMP_SPRITE_SC {
		get { return instance.m_tmpSpriteSC; }
	}

	[SerializeField] private string m_tmpSpritePC = "<sprite name=\"icon_pc\">";
	public static string TMP_SPRITE_PC {
		get { return instance.m_tmpSpritePC; }
	}

	[SerializeField] private string m_tmpSpriteGoldenEggFragment = "<sprite name=\"icon_dragon_scale\">";
	public static string TMP_SPRITE_GOLDEN_EGG_FRAGMENT {
		get { return instance.m_tmpSpriteGoldenEggFragment; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Colors
	#region Colors
	[SerializeField] private Color m_coinsTextColor = new Color(1f, 0.8f, 0.1f);
	public static Color COINS_TEXT_COLOR {
		get { return instance.m_coinsTextColor; }
	}

	[SerializeField] private Color m_pcTextColor = new Color(0.9f, 0.5f, 0.8f);
	public static Color PC_TEXT_COLOR {
		get { return instance.m_pcTextColor; }
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

	[SerializeField] private string m_powerMiniIconsPath = "UI/Metagame/Powers/icons_mini_powers";
	public static string POWER_MINI_ICONS_PATH {
		get { return instance.m_powerMiniIconsPath; }
	}

	[SerializeField] private string m_mapUpgradesIconsPath = "UI/Metagame/Map/";
	public static string MAP_UPGRADES_ICONS_PATH {
		get { return instance.m_mapUpgradesIconsPath; }
	}

	[SerializeField] private string m_shopIconsPath = "UI/Metagame/Shop/";
	public static string SHOP_ICONS_PATH {
		get { return instance.m_shopIconsPath; }
	}
	#endregion

	// -------------------------------------------------------------------------
	// Power colors
	#region PowerColors
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

	[SerializeField] private Color m_powerColorXp = new Color(0f, 1f, 0f);
	public static Color POWER_COLOR_XP {
		get { return instance.m_powerColorXp; }
	}

	[SerializeField] private Color m_powerColorObstacle = new Color(0.87f, 0.67f, 0.20f);
	public static Color POWER_COLOR_OBSTACLE {
		get { return instance.m_powerColorObstacle; }
	}

	[SerializeField] private Color m_powerColorEntity = new Color(1f, 1f, 1f);
	public static Color POWER_COLOR_ENTITY {
		get { return instance.m_powerColorEntity; }
	}

    [SerializeField] private Color m_powerColorWater = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_WATER {
		get { return instance.m_powerColorWater; }
	}

	[SerializeField] private Color m_powerColorDrain = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_DRAIN {
		get { return instance.m_powerColorDrain; }
	}

	[SerializeField] private Color m_powerColorVacuum = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_VACUUM {
		get { return instance.m_powerColorDrain; }
	}

	[SerializeField] private Color m_powerColorMagnetic = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_MAGNETIC {
		get { return instance.m_powerColorDrain; }
	}

	[SerializeField] private Color m_powerColorScore = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_SCORE {
		get { return instance.m_powerColorScore; }
	}

	[SerializeField] private Color m_powerColorFood = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_FOOD {
		get { return instance.m_powerColorFood; }
	}

	[SerializeField] private Color m_powerColorLowerDamage = new Color(0f, 0.91f, 1f);
	public static Color POWER_COLOR_LOWERDAMAGE {
		get { return instance.m_powerColorLowerDamage; }
	}
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

	[SerializeField] private float m_resultsDragonUnlockSpeedMultiplier = 1f;
	public static float resultsDragonUnlockSpeedMultiplier {
		get { return instance.m_resultsDragonUnlockSpeedMultiplier; }
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
    public static Color GetRarityColor(EggReward.Rarity _rarity) {
		return RARITY_COLORS[(int)_rarity];
	}

	/// <summary>
	/// Gets the color corresponding to a given rarity.
	/// </summary>
	/// <returns>The rarity color.</returns>
	/// <param name="_raritySku">The rarity to be checcked.</param>
	public static Color GetRarityColor(string _raritySku) {
		// Get rarity enum equivalent from sku
		return GetRarityColor(EggReward.SkuToRarity(_raritySku));
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
	/// Create a composite string consisting of a text and an icon.
	/// Specially useful for currency counters and price tags.
	/// </summary>
	/// <returns>The full string.</returns>
	/// <param name="_text">Text to be attached, typically a formatted amount. <see cref="StringUtils"/>.</param>
	/// <param name="_icon">Icon to be attached..</param>
	/// <param name="_alignment">Position of the icon relative to the text.</param>
	public static string GetIconString(string _text, IconType _icon, IconAlignment _alignment) {
		// Reset string builder
		instance.m_sb.Length = 0;

		// Figure out icon string
		string iconString = "";
		switch(_icon) {
			case IconType.COINS: {
				iconString = UIConstants.TMP_SPRITE_SC;
			} break;

			case IconType.PC: {
				iconString = UIConstants.TMP_SPRITE_PC;
			} break;

			case IconType.GOLDEN_FRAGMENTS: {
				iconString = UIConstants.TMP_SPRITE_GOLDEN_EGG_FRAGMENT;
			} break;

			case IconType.NONE: {
				iconString = string.Empty;
			} break;
		}

		// Compose final string with the proper alignment
		switch(_alignment) {
			case IconAlignment.NONE: {
				instance.m_sb.Append(_text);
			} break;

			case IconAlignment.LEFT: {
				if(!string.IsNullOrEmpty(iconString)) {
					instance.m_sb.Append(iconString).Append(" ");
				}
				instance.m_sb.Append(_text);
			} break;

			case IconAlignment.RIGHT: {
				instance.m_sb.Append(_text);
				if(!string.IsNullOrEmpty(iconString)) {
					instance.m_sb.Append(" ").Append(iconString);
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
		switch(_currency) {
			case UserProfile.Currency.SOFT:				return IconType.COINS;				break;
			case UserProfile.Currency.HARD:				return IconType.PC;					break;
			case UserProfile.Currency.GOLDEN_FRAGMENTS:	return IconType.GOLDEN_FRAGMENTS;	break;
		}
		return IconType.NONE;
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