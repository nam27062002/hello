﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPowerUp : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// ATTRIBUTES															  //
	//------------------------------------------------------------------------//
	private bool m_warnEntities = false;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	void Awake()
	{
		Entity.ResetSCMuliplier();
		Entity.ResetScoreMultiplier();
		Entity.ResetXpMultiplier();
	}

	void OnDestroy()
	{
		Entity.ResetSCMuliplier();
		Entity.ResetScoreMultiplier();
		Entity.ResetXpMultiplier();
	}

	void Start() 
	{
		DragonPlayer player = GetComponent<DragonPlayer>();
		string dragonSku = "";
		if (player != null) {
			dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			dragonSku = preview.sku;
		}

		// Disguise power up
		string disguise = UsersManager.currentUser.GetEquipedDisguise(dragonSku);
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguise);
		if (def != null) {
			string powerUp = def.Get("powerup");
			if(!string.IsNullOrEmpty(powerUp))
				SetPowerUp(powerUp);
		}


		// Pet power ups
		List<string> pets = UsersManager.currentUser.GetEquipedPets(dragonSku);
		for( int i = 0; i<pets.Count; i++ )
		{
			if ( !string.IsNullOrEmpty( pets[i] ) )
			{
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, pets[i]);
				if ( petDef != null )
				{
					string powerUp = petDef.Get("powerup");
					if ( !string.IsNullOrEmpty( powerUp ) )
					{
						SetPowerUp(powerUp);
					}
				}
			}
		}

		if ( m_warnEntities )
		{
			Messenger.Broadcast(GameEvents.APPLY_ENTITY_POWERUPS);
			m_warnEntities = false;
		}
	}

	void SetPowerUp( string powerUpSku )
	{
		DragonPlayer player = GetComponent<DragonPlayer>();
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);

		if ( def != null )
		{
			string type = def.Get("type");

			switch( type )
			{
				case "hp_increase":	// gives the player extra health
				{
					player.AddHealthBonus( def.GetAsFloat("param1"));
				}break;
				case "boost_increase":	// increases boost bar
				{
					player.AddBoostBonus( def.GetAsFloat("param1"));
				}break;
				case "faster_boost": // increases boost refill rate
				{
					DragonBoostBehaviour boost = player.dragonBoostBehaviour;
					if ( boost )
					{
						boost.AddRefillBonus( def.GetAsFloat("param1") );
					}
				}break;
				case "fury_duration":	// Adds fury duration
				{
					DragonBreathBehaviour breath = player.GetComponent<DragonBreathBehaviour>();
					if ( breath != null )
						breath.AddDurationBonus( def.GetAsFloat("param1") );
				}break;
				case "dive":	// lets you move inside water
				{
					DragonMotion motion = GetComponent<DragonMotion>();
					motion.canDive = true;
				}break;
				case "avoid":	// avoids numHits
				{
					// Check sub type with param one
					string subtype = def.Get("param1");
					int numHits = def.GetAsInt("param2");
					switch( subtype )
					{
						case "mine":
						{
							player.AddShields( DamageType.MINE, numHits );
						}break;
						case "poison":
						{
							player.AddShields( DamageType.POISON, numHits );
						}break;
					}
				}break;
				case "lower_damage":	// redices damage from sources
				{
					// Check sub type with param one
					string subtype = def.Get("param1");
					float percentage = def.GetAsFloat("param2");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					switch( subtype )
					{
						case "mine":
						{
							healthBehaviour.AddDamageReduction( DamageType.MINE, percentage );
						}break;
						case "poison":
						{
							healthBehaviour.AddDamageReduction( DamageType.POISON, percentage );
						}break;
						case "arrows":
						{
							healthBehaviour.AddDamageReduction( DamageType.ARROW, percentage );
						}break;
					}
				}break;

				case "lives":		// adds lives to the player
				{
					int numExtraLives = def.GetAsInt("param1");
					player.AddFreeRevives( numExtraLives );
				}break;
				case "dragonram":	// Allows you to break bigger obstaclers
				{
					int increase = def.GetAsInt("param1");
					player.SetOnBreakIncrease( increase );
				}break;
				case "prey_hp_boost":	// a prey gives you more hp
				{
					// string from = def.Get("param1");
					List<string> from = def.GetAsList<string>("param1");
					float percentage = def.GetAsFloat("param2");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					for( int i = 0; i<from.Count; i++ )
					{
						if (!string.IsNullOrEmpty(from[i]))
							healthBehaviour.AddEatingHpBoost( from[i], percentage);	
					}

				}break;
				case "food_increase":	// adds % bonus hp from any source
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddEatingHpBoost(percentage);
				}break; 
				case "reduce_life_drain":	// reduces lifedrain by param1 %
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddDrainReduceModifier( percentage );
				}break;
				case "more_coin":	// Increase SC given for all preys by param1 %
				{
					
					Entity.AddSCMultiplier( def.GetAsFloat("param1", 0));
					m_warnEntities = true;
				}break;
				case "score_increase":	// Increases score given for all preys by param1 %
				{
					// Increase score given by any prey by [param1]
					Entity.AddScoreMultiplier( def.GetAsFloat("param1", 0));
					m_warnEntities = true;
				}break;
				case "more_xp":
				{
					Entity.AddXpMultiplier( def.GetAsFloat("param1", 0));
					m_warnEntities = true;
				}break;
				case "fury_size_increase":	// Increases fire size by param1 %
				{
					DragonBreathBehaviour fireBreath = GetComponent<DragonBreathBehaviour>();
					float percentage = def.GetAsFloat("param1", 0);
					if (fireBreath != null )
					{
						fireBreath.AddPowerUpLengthMultiplier( percentage );
					}
				}break;
				case "speed_increase":	// Increases max speed by param1 %
				{
					DragonMotion motion = GetComponent<DragonMotion>();
					if ( motion != null )
					{
						motion.AddSpeedPowerup( def.GetAsFloat("param1", 0));
					}
				}break;
				case "vacuum":
				{
					DragonEatBehaviour eatBehaviour =  GetComponent<DragonEatBehaviour>();
					eatBehaviour.AddEatDistance( def.GetAsFloat("param1", 0) );
				}break;
				case "alcohol_resistance":
				{
					player.alcoholResistance = true;
				}break;
				case "immune_trash":
				{
					List<string> immuneTrash = def.GetAsList<string>("param1");
					DragonEatBehaviour eatBehaviour =  GetComponent<DragonEatBehaviour>();
					for( int i = 0; i<immuneTrash.Count; ++i )
					{
						if ( !string.IsNullOrEmpty( immuneTrash[i] ) )
						{
							eatBehaviour.AddImmuneTrash( immuneTrash[i] );
						}
					}
				}break;
				default:
				{
				}break;
			}
		}
	}

	/// <summary>
	/// Gets the description of this powerup, properly translated and formatted 
	/// depending on powerup type.
	/// </summary>
	/// <param name="_powerSku">Sku of the power whose description we want.</param>
	/// <param name="_short">Whether to return the short or the long description.</param>
	/// <returns>The description for the given power. Empty string if power not known or no description available.</returns>
	public static string GetDescription(string _powerSku, bool _short) {
		// Get definition
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
		return GetDescription(def, _short);
	}

	/// <summary>
	/// Gets the description of this powerup, properly translated and formatted 
	/// depending on powerup type.
	/// </summary>
	/// <param name="_powerSku">Sku of the power whose description we want.</param>
	/// <param name="_short">Whether to return the short or the long description.</param>
	/// <returns>The description for the given power. Empty string if power not known or no description available.</returns>
	public static string GetDescription(DefinitionNode _powerDef, bool _short) {
		// Check definition
		if(_powerDef == null) return "";

		// Short or long description?
		string fieldId = _short ? "tidDescShort" : "tidDesc";

		// Color and format based on type
		string type = _powerDef.GetAsString("type");
		Color color = GetColor(type);
		switch(type) {
			// Powers with custom formats
			case "lower_damage":
			case "lower_damage_origin": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param2")), color.ToHexString("#"));
			} break;

			case "dragonram": {
				if(_short) {
					return _powerDef.GetLocalized(fieldId, color.ToHexString("#"));
				} else {
					return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), color.ToHexString("#"));
				}
			} break;

			case "prey_hp_boost": {
				/*
				// Show target entity name
				// [AOC] TODO!! Plural
				DefinitionNode entityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, _powerDef.GetAsString("param1"));
				return _powerDef.GetLocalized(fieldId, entityDef.GetLocalized("tidName"), StringUtils.FormatNumber(_powerDef.GetAsFloat("param2"), 0), UIConstants.POWER_COLOR_ENTITY.ToHexString("#"), UIConstants.POWER_COLOR_HEALTH.ToHexString("#"));
				*/

				// [AOC] As of 05/07/2017, entity names are included in the TID (i.e. "Increased %U0 health on eating Birds")
				if(_short) {
					return _powerDef.GetLocalized(fieldId, color.ToHexString("#"));
				} else {
					return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param2"), 0), color.ToHexString("#"));
				}
			} break;

			// Powers with 1 int parameter
			case "hp_increase":
			case "boost_increase":
			case "fury_increase": 
			case "more_xp":
			case "fury_size_increase":
			case "fury_duration":
			case "speed_increase":
			case "reduce_life_drain":
			case "more_coin":
			case "score_increase":
			case "food_increase":
			case "vacuum":
			case "faster_boost":
			{
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), color.ToHexString("#"));
			} break;

			// Rest of powers (no params)
			default: {
				return _powerDef.GetLocalized(fieldId, color.ToHexString("#"));
			} break;
		}

		return "";
	}

	/// <summary>
	/// Gets the special color assigned to the given power type.
	/// </summary>
	/// <returns>The color assigned to the power's type.</returns>
	/// <param name="_powerDef">Power definition to be consulted.</param>
	public static Color GetColor(DefinitionNode _powerDef) {
		// Check definition
		if(_powerDef == null) return Color.white;

		// Get the color for this power type
		string type = _powerDef.GetAsString("type");
		return GetColor(type);
	}

	/// <summary>
	/// Gets the special color assigned to the given power type.
	/// </summary>
	/// <returns>The color assigned to the power's type.</returns>
	/// <param name="_type">Power type to be consulted.</param>
	public static Color GetColor(string _type) {
		// As of 09/10/2017, we're matching each power's color with the color of the pet category it belongs to
		switch(_type) {
			// Eat
			case "eat":
			case "vacuum":
			case "prey_hp_boost":
            case "drop_present":
                {
				return UIConstants.PET_CATEGORY_EAT;
			} break;

			// Health
			case "food_increase":
			case "hp_increase":
			{
				return UIConstants.PET_CATEGORY_HEALTH;
			} break;

			// Speed
			case "boost_increase":
			case "speed_increase":
			case "faster_boost":
			case "unlimited_boost":
			{
				return UIConstants.PET_CATEGORY_SPEED;
			} break;

			// Score
			case "more_coin":
			case "more_xp":
			case "score_increase":
			{
				return UIConstants.PET_CATEGORY_SCORE;
			} break;

			// Fire
			case "fury_size_increase":
			case "fury_duration":
			case "fireball":
			{
				return UIConstants.PET_CATEGORY_FIRE;
			} break;

			// Defense
			case "avoid":
			case "trash_eater":
			case "lower_damage":
			case "lower_damage_origin":
			case "explode_mine":
			case "reduce_life_drain":
			case "immune_trash":
			case "alcohol_resistance":
			case "cage_breaker":
			{
				return UIConstants.PET_CATEGORY_DEFENSE;
			} break;

			// Special
			case "lives":
			case "dragonram":
			case "magnet":
			case "freeze_aura":
			case "dive":
			case "phoenix":
			case "dog":
			case "bomb":
			case "stun":
			case "findBonus":
			{
				return UIConstants.PET_CATEGORY_SPECIAL;
			} break;

			// Default or unknown
			default: {
				return UIConstants.PET_CATEGORY_DEFAULT;
			}
		}

		return Color.white;
	}
}
