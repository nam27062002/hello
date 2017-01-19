using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonPowerUp : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly Color COLOR_HEALTH = new Color(0.7f, 0.8f, 0.24f);
	public static readonly Color COLOR_BOOST = new Color(0.38f, 1f, 0.78f);
	public static readonly Color COLOR_FIRE = new Color(1f, 0.64f, 0.22f);
	public static readonly Color COLOR_BITE = new Color(1f, 0.5f, 0.44f);
	public static readonly Color COLOR_SPEED = new Color(0.38f, 1f, 0.78f);

	public static readonly Color COLOR_GOLD = new Color(1f, 0.71f, 0.1f);
	public static readonly Color COLOR_PC = new Color(0.96f, 0.46f, 1f);

	public static readonly Color COLOR_MINE = new Color(0.63f, 0.63f, 0.63f);
	public static readonly Color COLOR_POISON = new Color(0f, 1f, 0f);
	public static readonly Color COLOR_OBSTACLE = new Color(0.87f, 0.67f, 0.20f);
	public static readonly Color COLOR_ENTITY = new Color(1f, 1f, 1f);
	public static readonly Color COLOR_WATER = new Color(0f, 0.91f, 100f);

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
	}

	void OnDestroy()
	{
		Entity.ResetSCMuliplier();
		Entity.ResetScoreMultiplier();
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
				case "boost_increase":
				{
					player.AddBoostBonus( def.GetAsFloat("param1"));
				}break;
				case "fury_increase":
				{
					/*
					DragonBreathBehaviour breath = player.GetComponent<DragonBreathBehaviour>();
					if ( breath != null )
						breath.SetFuryModifier( def.GetAsFloat("param1") );
						*/
				}break;
				case "fury_duration":
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
				case "avoid":
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
				case "lower_damage":
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
				case "lives":
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
				case "food+":
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddEatingHpBoost(percentage);
				}break; 
				case "reduce_lifedrain":
				{
					float percentage = def.GetAsFloat("param1");
					DragonHealthBehaviour healthBehaviour = GetComponent<DragonHealthBehaviour>();
					healthBehaviour.AddDrainReduceModifier( percentage );
				}break;
				case "sc+":
				{
					// Increase SC given for all preys by [param1]
					Entity.AddSCMultiplier( def.GetAsFloat("param1", 0));
					m_warnEntities = true;
				}break;
				case "score+":
				{
					// Increase score given by any prey by [param1]
					Entity.AddScoreMultiplier( def.GetAsFloat("param1", 0));
					m_warnEntities = true;
				}break;
				case "fire_size":
				{
					FireBreathNew fireBreath = GetComponent<FireBreathNew>();
					float percentage = def.GetAsFloat("param1", 0);
					if (fireBreath != null )
					{
						fireBreath.AddPowerUpLengthMultiplier( percentage );
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

		// Every power type has a different format
		switch(_powerDef.GetAsString("type")) {
			case "hp_increase":	 {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), COLOR_HEALTH.ToHexString("#"));
			} break;

			case "boost_increase": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), COLOR_BOOST.ToHexString("#"));
			} break;

			case "fury_increase": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsFloat("param1"), 0), COLOR_FIRE.ToHexString("#"));
			} break;

			case "dive": {
				return _powerDef.GetLocalized(fieldId, COLOR_WATER.ToHexString("#"));
			} break;
			
			case "avoid": {
				// Check sub type with param one
				string subtype = _powerDef.Get("param1");
				int numHits = _powerDef.GetAsInt("param2");
				switch(subtype) {
					case "mine": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), COLOR_MINE.ToHexString("#"));
					} break;

					case "poison": {
						return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(numHits), COLOR_POISON.ToHexString("#"));
					} break;

					default: {
						return _powerDef.GetLocalized(fieldId);
					} break;
				}
			} break;

			case "lives": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), COLOR_HEALTH.ToHexString("#"));
			} break;

			case "dragonram": {
				return _powerDef.GetLocalized(fieldId, StringUtils.FormatNumber(_powerDef.GetAsInt("param1")), COLOR_OBSTACLE.ToHexString("#"));
			} break;
			
			case "preyHpBoost": {
				// Show target entity name
				// [AOC] TODO!! Plural
				DefinitionNode entityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.ENTITIES, _powerDef.GetAsString("param1"));
				return _powerDef.GetLocalized(fieldId, entityDef.GetLocalized("tidName"), StringUtils.FormatNumber(_powerDef.GetAsFloat("param2"), 0), COLOR_ENTITY.ToHexString("#"), COLOR_HEALTH.ToHexString("#"));
			} break;

			default: {
				return _powerDef.GetLocalized(fieldId);
			} break;
		}

		return "";
	}
}
