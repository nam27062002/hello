using UnityEngine;
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
				case "preyHpBoost":	// a prey gives you more hp
				{
					string from = def.Get("param1");
					float percentage = def.GetAsFloat("param2");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddEatingHpBoost( from, percentage);
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

		// Every power type has a different format
		switch(_powerDef.GetAsString("type")) {
			case "hp_increase":	 {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), UIConstants.POWER_COLOR_HEALTH.ToHexString("#"));
			} break;

			case "boost_increase": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), UIConstants.POWER_COLOR_BOOST.ToHexString("#"));
			} break;

			case "fury_increase": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), UIConstants.POWER_COLOR_FIRE.ToHexString("#"));
			} break;

			case "more_xp": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), UIConstants.POWER_COLOR_XP.ToHexString("#"));
			} break;

			case "dive": {
				return _powerDef.GetLocalized(fieldId, UIConstants.POWER_COLOR_WATER.ToHexString("#"));
			} break;
			
			case "avoid": {
				// Check sub type with param one
				string subtype = _powerDef.Get("param1");
				int numHits = _powerDef.GetAsInt("param2");
				switch(subtype) {
					case "mine": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), UIConstants.POWER_COLOR_MINE.ToHexString("#"));
					} break;

					case "poison": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), UIConstants.POWER_COLOR_POISON.ToHexString("#"));
					} break;

					default: {
						return _powerDef.GetLocalized(fieldId);
					} break;
				}
			} break;

			case "lower_damage": {
				// Check sub type with param one
				string subtype = _powerDef.Get("param1");
				int numHits = _powerDef.GetAsInt("param2");
				switch(subtype) {
					case "mine": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), UIConstants.POWER_COLOR_MINE.ToHexString("#"));
					} break;

					case "poison": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), UIConstants.POWER_COLOR_POISON.ToHexString("#"));
					} break;

					case "arrows": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), UIConstants.POWER_COLOR_ENTITY.ToHexString("#"));
					} break;

					default: {
						return _powerDef.GetLocalized(fieldId);
					} break;
				}
			} break;

			case "lives": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_HEALTH.ToHexString("#"));
			} break;

			case "dragonram": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_OBSTACLE.ToHexString("#"));
			} break;

            case "fury_size_increase":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_FIRE.ToHexString("#"));
            }break;

            case "fury_duration":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_FIRE.ToHexString("#"));
                }break;

            case "speed_increase":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_SPEED.ToHexString("#"));
                }break;

            case "reduce_life_drain":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_DRAIN.ToHexString("#"));
                }break;

            case "more_coin":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.COINS_TEXT_COLOR.ToHexString("#"));
                }break;

            case "score_increase":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_SCORE.ToHexString("#"));
                }break;

            case "food_increase":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_FOOD.ToHexString("#"));
                }break;

            case "freeze_aura":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_WATER.ToHexString("#"));
                }break;

            case "fireball":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_FIRE.ToHexString("#"));
                }break;

            case "magnet":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_MAGNETIC.ToHexString("#"));
                }break;

            case "vacuum":
                {
                    return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), UIConstants.POWER_COLOR_VACUUM.ToHexString("#"));
                }break;

            case "preyHpBoost": {
				// Show target entity name
				// [AOC] TODO!! Plural
				DefinitionNode entityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, _powerDef.GetAsString("param1"));
				return _powerDef.GetLocalized(fieldId, entityDef.GetLocalized("tidName"), StringUtils.FormatNumber(_powerDef.GetAsFloat("param2"), 0), UIConstants.POWER_COLOR_ENTITY.ToHexString("#"), UIConstants.POWER_COLOR_HEALTH.ToHexString("#"));
			} break;

			default: {
				return _powerDef.GetLocalized(fieldId);
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

		// Every power type has a different format
		switch(_powerDef.GetAsString("type")) {
			case "hp_increase":	 {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "boost_increase": {
				return UIConstants.POWER_COLOR_BOOST;
			} break;

			case "fury_size_increase": {
				return UIConstants.POWER_COLOR_FIRE;
			} break;

			case "magnetic": {
				return UIConstants.POWER_COLOR_MAGNETIC;
			} break;

			case "vacuum": {
				return UIConstants.POWER_COLOR_VACUUM;
			} break;

			case "freeze_aura": {
				return UIConstants.POWER_COLOR_WATER;
			} break;

			case "fury_duration_increase": {
				return UIConstants.POWER_COLOR_FIRE;
			} break;

			case "lower_damage": {
				return UIConstants.POWER_COLOR_FIRE;
			} break;

			case "dive": {
				return UIConstants.POWER_COLOR_WATER;
			} break;

			case "avoid": {
				// Check sub type with param one
				string subtype = _powerDef.Get("param1");
				int numHits = _powerDef.GetAsInt("param2");
				switch(subtype) {
					case "mine": {
						return UIConstants.POWER_COLOR_MINE;
					} break;

					case "poison": {
						return UIConstants.POWER_COLOR_POISON;
					} break;

					default: {
						return Color.white;
					} break;
				}
			} break;

			case "lives": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "dragonram": {
				return UIConstants.POWER_COLOR_OBSTACLE;
			} break;

			case "preyHpBoost": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "speed_increase": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "reduce_life_drain": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "more_coin": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "score_increase": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "food_increase": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			case "fireball": {
				return UIConstants.POWER_COLOR_HEALTH;
			} break;

			default: {
				return Color.white;
			} break;
		}

		return Color.white;
	}
}
