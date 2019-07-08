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

	private static float sm_petPowerUpPercentage = 0f;
	public static void AddPetPowerUpPercentage(float _percentage) {
		sm_petPowerUpPercentage += _percentage;
	}

	private float m_entitySCMultiplier;
	private float m_entityXPMultiplier;
	private float m_entityScoreMultiplier;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	void Awake()
	{
		m_entitySCMultiplier = 0f;
		m_entityXPMultiplier = 0f;
		m_entityScoreMultiplier = 0f;
	}

	void OnDestroy()
	{
		Entity.AddSCMultiplier(-m_entitySCMultiplier);
		Entity.AddXpMultiplier(-m_entityXPMultiplier);
		Entity.AddScoreMultiplier(-m_entityScoreMultiplier);
	}

	void Start() 
	{
		InstanceManager.APPLY_DRAGON_MODIFIERS();

		CPModifiers.ApplyDragonMods();

		HDLiveDataManager.instance.ApplyDragonMods();
								
		DragonPlayer player = GetComponent<DragonPlayer>();
		string dragonSku = "";
		if (player != null) {
			dragonSku = player.data.def.sku;
		} else {
			MenuDragonPreview preview = GetComponent<MenuDragonPreview>();
			dragonSku = preview.sku;
		}

		// Disguise power up
		string disguise;
		if (HDLiveDataManager.tournament.isActive) {
			disguise = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData.disguise;
		} else {
			disguise = UsersManager.currentUser.GetEquipedDisguise(dragonSku);
		}

		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguise);
		if (def != null) {
			string powerUp = def.Get("powerup");
			if(!string.IsNullOrEmpty(powerUp)){
				SetPowerUp(powerUp, false);
			}
		}

		// Pet power ups
		List<string> pets;
		// Check if tournament
		if (HDLiveDataManager.tournament.isActive) {
			pets = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData.pets;
		} else {
			pets = UsersManager.currentUser.GetEquipedPets(dragonSku);
		}

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
						SetPowerUp(powerUp, true);
					}
				}
			}
		}

		if ( m_warnEntities )
		{
			Broadcaster.Broadcast(BroadcastEventType.APPLY_ENTITY_POWERUPS);
			m_warnEntities = false;
		}
	}

	void SetPowerUp( string powerUpSku, bool _fromPet )
	{
		DragonPlayer player = InstanceManager.player;
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);

		if ( def != null )
		{
			string type = def.Get("type");
			string category = def.Get("category");

			float multiplier = 1f;
			if (_fromPet) {
				if (category.Equals("stats")) {
					multiplier += sm_petPowerUpPercentage / 100f;
				}
			}

			switch( type )
			{
				case "hp_increase":	// gives the player extra health
				{
					player.AddHealthBonus( def.GetAsFloat("param1") * multiplier );
				}break;
				case "boost_increase":	// increases boost bar
				{
					player.AddBoostBonus( def.GetAsFloat("param1") * multiplier);
				}break;
				case "faster_boost": // increases boost refill rate
				{
					DragonBoostBehaviour boost = player.dragonBoostBehaviour;
					if ( boost )
					{
						boost.AddRefillBonus( def.GetAsFloat("param1") * multiplier );
					}
				}break;
				case "fury_duration":	// Adds fury duration
				{
					DragonBreathBehaviour breath = player.breathBehaviour;
					if ( breath != null )
						breath.AddDurationBonus( def.GetAsFloat("param1") * multiplier );
				}break;
				case "dive":	// lets you move inside water
				{
					DragonMotion motion = player.dragonMotion;
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
							healthBehaviour.AddArmorModifier( DamageType.MINE, percentage * multiplier );
						}break;
						case "poison":
						{
							healthBehaviour.AddArmorModifier( DamageType.POISON, percentage * multiplier );
						}break;
						case "arrows":
						{
							healthBehaviour.AddArmorModifier( DamageType.ARROW, percentage * multiplier );
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
							healthBehaviour.AddEatingHpBoost( from[i], percentage * multiplier);	
					}

				}break;
				case "food_increase":	// adds % bonus hp from any source
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddEatingHpBoost(percentage * multiplier);
				}break; 
				case "reduce_life_drain":	// reduces lifedrain by param1 %
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddDrainModifier( percentage * multiplier );
				}break;
				case "more_coin":	// Increase SC given for all preys by param1 %
				{
					float coins = def.GetAsFloat("param1", 0) * multiplier;
					m_entitySCMultiplier += coins;
					Entity.AddSCMultiplier(coins);
					m_warnEntities = true;
				}break;
				case "score_increase":	// Increases score given for all preys by param1 %
				{
					float score = def.GetAsFloat("param1", 0) * multiplier;
					m_entityScoreMultiplier += score;

					// Increase score given by any prey by [param1]
					Entity.AddScoreMultiplier(score);
					m_warnEntities = true;
				}break;
				case "more_xp":
				{
					float xp = def.GetAsFloat("param1", 0) * multiplier;
					m_entityXPMultiplier += xp;

					Entity.AddXpMultiplier(m_entityXPMultiplier);
					m_warnEntities = true;
				}break;
				case "fury_size_increase":	// Increases fire size by param1 %
				{
					DragonBreathBehaviour fireBreath = GetComponent<DragonBreathBehaviour>();
					float percentage = def.GetAsFloat("param1", 0);
					if (fireBreath != null )
					{
						fireBreath.AddPowerUpLengthMultiplier( percentage * multiplier );
					}
				}break;
				case "speed_increase":	// Increases max speed by param1 %
				{
					DragonMotion motion = GetComponent<DragonMotion>();
					if ( motion != null )
					{
						motion.AddSpeedModifier( def.GetAsFloat("param1", 0) * multiplier);
					}
				}break;
				case "vacuum":
				{
					DragonEatBehaviour eatBehaviour =  GetComponent<DragonEatBehaviour>();
					eatBehaviour.AddEatDistance( def.GetAsFloat("param1", 0) * multiplier );
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
                case "mummy": {
                    int mummyStacks = def.GetAsInt("param1");
                    player.AddMummyPower(mummyStacks);
                }break;
				case "combined":
				{
					string powerUp1 = def.Get("param1");
					if ( !string.IsNullOrEmpty(powerUp1) )
						SetPowerUp( powerUp1, _fromPet );
					string powerUp2 = def.Get("param2");
					if ( !string.IsNullOrEmpty(powerUp2) )
						SetPowerUp( powerUp2, _fromPet );
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
	public static string GetDescription(string _powerSku, bool _short, bool _fromPet) {
		// Get definition
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
		return GetDescription(def, _short, _fromPet);
	}

	/// <summary>
	/// Gets the description of this powerup, properly translated and formatted 
	/// depending on powerup type.
	/// </summary>
	/// <param name="_powerSku">Sku of the power whose description we want.</param>
	/// <param name="_short">Whether to return the short or the long description.</param>
	/// <returns>The description for the given power. Empty string if power not known or no description available.</returns>
	public static string GetDescription(DefinitionNode _powerDef, bool _short, bool _fromPet) {
		// Check definition
		if(_powerDef == null) return "";

		string category = _powerDef.Get("category");

		float multiplier = 1f;
		if (_fromPet) {
			if (category.Equals("stats")) {
				multiplier += sm_petPowerUpPercentage / 100f;
			}
		}

		// Short or long description?
		string fieldId = _short ? "tidDescShort" : "tidDesc";

		// Color and format based on type
		string type = _powerDef.GetAsString("type");
		Color color = GetColor(_powerDef);
		switch(type) {
			// Powers with custom formats
			case "lower_damage":
			case "lower_damage_origin": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber((int)(_powerDef.GetAsFloat("param2") * multiplier)), color.ToHexString("#"));
			} break;

			case "dragonram": {
				if(_short) {
					return _powerDef.GetLocalized(fieldId, color.ToHexString("#"));
				} else {
					return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber((int)(_powerDef.GetAsFloat("param1") * multiplier)), color.ToHexString("#"));
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
					return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param2") * multiplier, 0), color.ToHexString("#"));
				}
			} break;

			//TONI
			//We need to get all parameters for each single powerup. Combined powers could be a combination of 3 powers. As we can have just 2 parameters by power, a powerup that
			//combines 3 powers is a combination of a 'normal' power and a combined power of 2
			case "combined":
			{
				DefinitionNode powerUp1 = DefinitionsManager.SharedInstance.GetDefinition (DefinitionsCategory.POWERUPS, _powerDef.GetAsString ("param1"));
				string typePower1 = powerUp1.GetAsString("type");
				if (typePower1 == "combined") {
					DefinitionNode powerUp1c = DefinitionsManager.SharedInstance.GetDefinition (DefinitionsCategory.POWERUPS, powerUp1.GetAsString ("param1"));
					string paramX1 = "param1";
					if (powerUp1c.GetAsString ("type") == "lower_damage")
						paramX1 = "param2";

					DefinitionNode powerUp2c = DefinitionsManager.SharedInstance.GetDefinition (DefinitionsCategory.POWERUPS, powerUp1.GetAsString ("param2"));
					string paramX2 = "param1";
					if (powerUp2c.GetAsString ("type") == "lower_damage")
						paramX2 = "param2";

					DefinitionNode powerUp3 = DefinitionsManager.SharedInstance.GetDefinition (DefinitionsCategory.POWERUPS, _powerDef.GetAsString ("param2"));
					string paramX3 = "param1";
					if (powerUp3.GetAsString ("type") == "lower_damage")
						paramX3 = "param2";					

					return _powerDef.GetLocalized (
						fieldId, 
						StringUtils.FormatNumber ((int)(powerUp1c.GetAsFloat (paramX1) * multiplier)), 
						StringUtils.FormatNumber ((int)(powerUp2c.GetAsFloat (paramX2) * multiplier)), 
						StringUtils.FormatNumber ((int)(powerUp3.GetAsFloat (paramX3) * multiplier)), 
						color.ToHexString ("#")
					);				
				} else {
					DefinitionNode powerUp2 = DefinitionsManager.SharedInstance.GetDefinition (DefinitionsCategory.POWERUPS, _powerDef.GetAsString ("param2"));
					
					string paramX1 = "param1";
					if (powerUp1.GetAsString ("type") == "lower_damage")
						paramX1 = "param2";

					string paramX2 = "param1";
					if (powerUp2.GetAsString ("type") == "lower_damage")
						paramX2 = "param2";

					return _powerDef.GetLocalized (
						fieldId, 
						StringUtils.FormatNumber ((int)(powerUp1.GetAsFloat (paramX1) * multiplier)), 
						StringUtils.FormatNumber ((int)(powerUp2.GetAsFloat (paramX2) * multiplier)), 
						color.ToHexString ("#")
					);
				}
			} break;

			//TONI
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
				return _powerDef.GetLocalized(
					fieldId, 
					StringUtils.FormatNumber((int)(_powerDef.GetAsFloat("param1") * multiplier)), 
					color.ToHexString("#")
				);
			} break;
			// Rest of powers (no params)
			default: {
				return _powerDef.GetLocalized(
					fieldId, 
					color.ToHexString("#")
				);
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

		// Some types need special treatment
		switch(type) {
			case "combined": {
				// Use the color from second combined power type
				string powerUp2 = _powerDef.Get("param2");
				DefinitionNode powerUp2Def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerUp2);
				if(powerUp2Def != null) {
					type = powerUp2Def.GetAsString("type");
				}
				//TONI
				type = "special";
				//TONI
			} break;
		}

		return GetColor(type);
	}

	/// <summary>
	/// Gets the special color assigned to the given power type.
	/// </summary>
	/// <returns>The color assigned to the power's type.</returns>
	/// <param name="_type">Power type to be consulted.</param>
	private static Color GetColor(string _type) {
		// As of 09/10/2017, we're matching each power's color with the color of the pet category it belongs to
		switch(_type) {
			// Eat
			case "eat":
			case "vacuum":
			case "prey_hp_boost":
            case "drop_present":
			case "food_increase":			
            {
				return UIConstants.PET_CATEGORY_EAT;
			} break;

			// Health
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
			//TONI
			case "special":
			case "sun":
			case "shoot_bubble":
			//TONI
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
			case "tranformGold":
			case "shoot_horns":
			case "kill":
			case "mummy":			
			{
				return UIConstants.PET_CATEGORY_SPECIAL;
			} break;

			// Special Cases
			case "combined": {
				// Should never be called, since we're parsing the type of the second combined power instead
				return Color.black;
			} break;

			// Default or unknown
			default: {
				return UIConstants.PET_CATEGORY_DEFAULT;
			}
		}

		return Color.white;
	}
}
