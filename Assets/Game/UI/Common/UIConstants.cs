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

#if UNITY_IOS
using UnityEngine.iOS;
#endif

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Static class for some global UI constants and utils.
/// </summary>
[CreateAssetMenu]
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

	// Special devices: extreme aspect ratios, custom safe areas, etc.
	public enum SpecialDevice {
		NONE,
		IPHONE_X,
		ANDROID_MAX_ASPECT_RATIO,
		COUNT
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
	[SerializeField] private Color[] m_rarityColors = new Color[(int)Metagame.Reward.Rarity.COUNT];
	public static Color[] RARITY_COLORS {
		get { return instance.m_rarityColors; }
	}

	[SerializeField] private Gradient4[] m_rarityTextGradients = new Gradient4[(int)Metagame.Reward.Rarity.COUNT];
	public static Gradient4[] RARITY_TEXT_GRADIENTS {
		get { return instance.m_rarityTextGradients; }
	}

	[SerializeField] private Sprite[] m_rarityIcons = new Sprite[(int)Metagame.Reward.Rarity.COUNT];
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

	[SerializeField] private string m_disguiseIconsPath = "UI/Metagame/Dragons/Disguises/";
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

	[SerializeField] private string m_missionIconsPath = "UI/Metagame/Missions/Icons/";
	public static string MISSION_ICONS_PATH {
		get { return instance.m_missionIconsPath; }
	}

    [SerializeField] private string m_eggIconsPath = "UI/Metagame/Eggs/";
	public static string EGG_ICONS_PATH {
		get { return instance.m_eggIconsPath; }
	}

	[SerializeField] private string m_seasonIconsPath = "UI/Metagame/Seasons/";
	public static string SEASON_ICONS_PATH {
		get { return instance.m_seasonIconsPath; }
	}

    [SerializeField] private string m_seasonalParticlesPath = "UI/FX/Seasons/";
    public static string SEASONAL_PARTICLES_PATH
    {
        get { return instance.m_seasonalParticlesPath; }
    }

    [SerializeField] private string m_modifierIconsPath = "UI/Metagame/Powers/";
	public static string MODIFIER_ICONS_PATH {
		get { return instance.m_modifierIconsPath; }
	}

	[SerializeField] private string m_liveEventsIconsPath = "UI/Metagame/Powers/";
	public static string LIVE_EVENTS_ICONS_PATH {
		get { return instance.m_liveEventsIconsPath; }
	}

	[SerializeField] private string m_dragonStatsIconsPath = "UI/Metagame/Dragons/Stats/";
	public static string DRAGON_STATS_ICONS_PATH {
		get { return instance.m_dragonStatsIconsPath; }
	}

	[SerializeField] private string m_leagueIconsPath = "UI/Metagame/Leagues/";
	public static string LEAGUE_ICONS_PATH {
		get { return instance.m_leagueIconsPath; }
	}

	[SerializeField] private string m_hdLogoPath = "UI/Common/Logo/";
	public static string HD_LOGO_PATH {
		get { return instance.m_hdLogoPath; }
	}

	[SerializeField] private string m_shareScreensPath = "UI/Social/";
	public static string SHARE_SCREENS_PATH {
		get { return instance.m_shareScreensPath; }
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

	public static Color PET_CATEGORY_EAT { get { return instance.m_petCategoryColorEat; } }
	public static Color PET_CATEGORY_HEALTH { get { return instance.m_petCategoryColorHealth; } }
	public static Color PET_CATEGORY_SPEED { get { return instance.m_petCategoryColorSpeed; } }
	public static Color PET_CATEGORY_SCORE { get { return instance.m_petCategoryColorScore; } }
	public static Color PET_CATEGORY_FIRE { get { return instance.m_petCategoryColorFire; } }
	public static Color PET_CATEGORY_DEFENSE { get { return instance.m_petCategoryColorDefense; } }
	public static Color PET_CATEGORY_SPECIAL { get { return instance.m_petCategoryColorSpecial; } }
	public static Color PET_CATEGORY_DEFAULT { get { return instance.m_petCategoryColorDefault; } }
	#endregion

	// -------------------------------------------------------------------------
	// Misc colors
	#region MiscColors
	[SerializeField] private Color m_dragonStatColorHp = Color.red;
	[SerializeField] private Color m_dragonStatColorSpeed = Color.cyan;
	[SerializeField] private Color m_dragonStatColorEnergy = Color.yellow;

	[SerializeField] private Color[] m_dragonTierColors = new Color[0];
	#endregion

	// -----------------------------------------------------------------------//
	// Open Egg animation setup												  //
	// -----------------------------------------------------------------------//
	#region OpenEggAnimSetup
	[SerializeField] private float m_openEggExplosionDuration = 3.6f;   // Sync with actual animation
	public static float openEggExplosionDuration {
		get { return instance.m_openEggExplosionDuration; }
	}

	[SerializeField] private float m_openEggSpinIntensity = 50f;
	public static float openEggSpinIntensity {
		get { return instance.m_openEggSpinIntensity; }
	}

	[SerializeField] private Ease m_openEggSpinEase = Ease.OutExpo;
	public static Ease openEggSpinEase {
		get { return instance.m_openEggSpinEase; }
	}
	#endregion

	// -----------------------------------------------------------------------//
	// Results animation setup												  //
	//------------------------------------------------------------------------//
	#region ResultsAnimSetup
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

	[SerializeField] private float m_resultsEggDuration = 1f;
	public static float resultsEggDuration {
		get { return instance.m_resultsEggDuration; }
	}
	#endregion

	// -----------------------------------------------------------------------//
	// Menu animation setup													  //
	//------------------------------------------------------------------------//
	#region MenuAnimationSetup
	[SerializeField] private float m_menuCameraTransitionDuration = 0.5f;
	public static float menuCameraTransitionDuration {
		get {
			// Check if it has been override in the Control Panel
			float duration = DebugSettings.Prefs_GetFloatPlayer(DebugSettings.MENU_CAMERA_TRANSITION_DURATION, instance.m_menuCameraTransitionDuration);
			if(duration <= 0) duration = instance.m_menuCameraTransitionDuration;
			return duration;
		}
	}
	#endregion

	// -----------------------------------------------------------------------//
	// Other Constants														  //
	//------------------------------------------------------------------------//
	#region OtherConstants
	[SerializeField] private Vector2 m_canvasReferenceResolution = new Vector2(2048f, 1536f);
	public static Vector2 CANVAS_REFERENCE_RESOLUTION {
		get { return instance.m_canvasReferenceResolution; }
	}

	public static Vector2 CANVAS_SIZE {
		get {
			return new Vector2(
				CANVAS_REFERENCE_RESOLUTION.y * ASPECT_RATIO,
				CANVAS_REFERENCE_RESOLUTION.y
			);
		}
	}

	public static float ASPECT_RATIO {
		get { return (float)Screen.width / (float)Screen.height; }
	}


	[SerializeField] private float m_mapMarkersDepth = -50f;
	public static float MAP_MARKERS_DEPTH {
		get { return instance.m_mapMarkersDepth; }
	}

	[SerializeField] private string[] m_dragonTiersSFX = new string[(int)DragonTier.COUNT];
	public static string[] dragonTiersSFX {
		get { return instance.m_dragonTiersSFX; }
	}

	[Space]
	[Tooltip("width/height, 16:9 is 1.77")]
	[SerializeField] private float m_androidSafeAreaAspectRatioThreshold = 1.95f; // (16:9) is 1.77 )
	private bool m_specialDeviceInitialized = false;
	private SpecialDevice m_specialDevice = SpecialDevice.NONE;
	public static SpecialDevice specialDevice {
		get {
			// Has the special device been initialized?
			if(!instance.m_specialDeviceInitialized || Application.isEditor) {
				// No! Do it now
				instance.m_specialDevice = SpecialDevice.NONE;

				// Override if debugging
				if(DebugSettings.simulatedSpecialDevice != SpecialDevice.NONE) {
					instance.m_specialDevice = DebugSettings.simulatedSpecialDevice;
				}

#if UNITY_IOS
				// Is it an iPhone X?
				else if(Device.generation >= DeviceGeneration.iPhoneX && 
				        Device.generation < DeviceGeneration.iPhoneUnknown &&	// [AOC] HACK!! Small trick to support newest iPhones starting at iPhoneX
				        ASPECT_RATIO > 1.5f) {		// [AOC] Make sure it's not an iPad / Tablet, where the UI resizing would create a lot of issues
					instance.m_specialDevice = SpecialDevice.IPHONE_X;
				}
#elif UNITY_ANDROID
				// Is it an Android with huge aspect ratio? (likely to have rounded corners or notches)
                else {
					float ar = (float)Screen.width / (float)Screen.height;
					if(ar > instance.m_androidSafeAreaAspectRatioThreshold) {
						instance.m_specialDevice = SpecialDevice.ANDROID_MAX_ASPECT_RATIO;
					}
				}
#endif
				// Mark as initialized!
				instance.m_specialDeviceInitialized = true;
			}
			return instance.m_specialDevice;
		}
	}

	[SerializeField] private UISafeArea[] m_safeAreas = new UISafeArea[0];
	private static UISafeArea m_safeArea = null;
	public static UISafeArea safeArea {
		get {
			// If not yet initialized, do it now
			if(m_safeArea == null) {

#if (!UNITY_EDITOR && UNITY_ANDROID)
                //  Use Calety cutout safe area for Android 9
                if (DeviceUtilsManager.SharedInstance.DeviceHasCutout())
                {
                    short[] safe = DeviceUtilsManager.SharedInstance.DeviceGetCutoutSafeArea();
                    Debug.Log("DeviceUtilsManager.SharedInstance.DeviceGetCutoutSafeArea() --> left: " + safe[0] + " right: " + safe[1] + " top: " + safe[2] + " bottom: " + safe[3]);
                    int maxx = Mathf.Max(safe[0], safe[1]);
                    m_safeArea = new UISafeArea(
                        (float)maxx, (float)safe[2], (float)maxx, (float)safe[3]);

                }
                else
#endif
                // Use Unity's safeArea or custom ones based on device?
                // [AOC] Unity's safeArea doesn't seem to work properly with some resolutions, needs further research but eventually should be the way to go
                if (!DebugSettings.useUnitySafeArea) {
					// Select target safe area based on special device
					return instance.m_safeAreas[(int)specialDevice];
				} else {
					// Unity's safe area is in Screen pixels
					// Normalize and multiply by our Canvases reference resolution (hardcoded)
#if UNITY_EDITOR
					// Use DebugSettings to simulate different safe areas
					Rect systemSafeArea = DebugSettings.debugSafeArea;
#else
				Rect systemSafeArea = Screen.safeArea;
#endif
					ControlPanel.Log(Colors.orange.Tag("SYSTEM SAFE AREA: " + systemSafeArea.ToString()));

					// Normalize with screen size
					Rect normalizedSafeArea = new Rect(
						systemSafeArea.x / Screen.width,
						systemSafeArea.y / Screen.height,
						systemSafeArea.width / Screen.width,
						systemSafeArea.height / Screen.height
					);
					ControlPanel.Log(Colors.orange.Tag("NORMALIZED SAFE AREA: " + normalizedSafeArea.ToString()));

					// Scale back to our canvases reference resolution
					// Keep Aspect Ratio!
					Vector2 canvasSize = CANVAS_SIZE;
					ControlPanel.Log(Colors.orange.Tag("ar: " + ASPECT_RATIO + " | canvas: " + canvasSize.x + ", " + canvasSize.y));
					m_safeArea = new UISafeArea(
						normalizedSafeArea.xMin * canvasSize.x,
						(1f - normalizedSafeArea.yMax) * canvasSize.y,
						(1f - normalizedSafeArea.xMax) * canvasSize.x,
						normalizedSafeArea.yMin * canvasSize.y
					);
					ControlPanel.Log(Colors.orange.Tag("SAFE AREA: " + m_safeArea.ToString()));
				}
			}
			return m_safeArea;
		}
	}
#endregion

	//------------------------------------------------------------------------//
	// NON-STATIC MEMBERS													  //
	//------------------------------------------------------------------------//
	// Internal
	private StringBuilder m_sb = new StringBuilder();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scriptable object has been enabled.
	/// </summary>
	private void OnEnable() {
		// Reset some runtime vars to their initial value
		// [AOC] ScriptableObject Singletons are permanently loaded in the editor, thus not resetting runtime variables :(
		m_safeArea = null;
	}

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
	/// Gets the text color gradient corresponding to a given rarity.
	/// </summary>
	/// <returns>The rarity text gradient.</returns>
	/// <param name="_rarity">The rarity to be checked.</param>
	public static Gradient4 GetRarityTextGradient(Metagame.Reward.Rarity _rarity) {
		return RARITY_TEXT_GRADIENTS[(int)_rarity];
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
	/// Get the color assigned to a specific dragon stat.
	/// </summary>
	/// <returns>The color linked to the requested dragon stat.</returns>
	/// <param name="_stat">Stat whose color we want.</param>
	public static Color GetDragonStatColor(DragonDataSpecial.Stat _stat) {
		switch(_stat) {
			case DragonDataSpecial.Stat.HEALTH:	return instance.m_dragonStatColorHp;		break;
			case DragonDataSpecial.Stat.SPEED:	return instance.m_dragonStatColorSpeed;		break;
			case DragonDataSpecial.Stat.ENERGY:	return instance.m_dragonStatColorEnergy;	break;
			default: return Color.white; break;
		}
	}

	/// <summary>
	/// Get the color assigned to a specific dragon tier.
	/// </summary>
	/// <returns>The color linked to the given dragon tier.</returns>
	/// <param name="_tier">Tier whose color we want.</param>
	public static Color GetDragonTierColor(DragonTier _tier) {
		return instance.m_dragonTierColors[(int)_tier];
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

	/// <summary>
	/// Given a dragon tier, get the sound effect linked to it.
	/// </summary>
	/// <returns>The ID of the sound effect linked to the requested tier.</returns>
	/// <param name="_tier">Dragon tier whose SFX we want.</param>
	public static string GetDragonTierSFX(DragonTier _tier) {
		return instance.m_dragonTiersSFX[(int)_tier];
	}

	/// <summary>
	/// Given a dragon tier, get the icon linked to it.
	/// Use in combination with GetSpriteTag() to insert the icon within a text
	/// </summary>
	/// <returns>The id of the requested dragon tier icon.</returns>
	/// <param name="_tier">Tier whose icon is required.</param>
	public static string GetDragonTierIcon(DragonTier _tier) {
		// Get definition of the wanted tier
		DefinitionNode tierDef = DefinitionsManager.SharedInstance.GetDefinition(
			DefinitionsCategory.DRAGON_TIERS, 
			IDragonData.TierToSku(_tier)
		);
		if(tierDef == null) return string.Empty;

		// Return icon name
		return tierDef.GetAsString("icon");
	}
}