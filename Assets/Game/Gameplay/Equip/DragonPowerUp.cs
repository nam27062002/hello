using UnityEngine;
using System.Collections;

public class DragonPowerUp : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly Color COLOR_HEALTH = new Color(0f, 1f, 0f);
	public static readonly Color COLOR_BOOST = new Color(0f, 0.75f, 0.6f);
	public static readonly Color COLOR_FIRE = new Color(1f, 0.6f, 0.1f);
	public static readonly Color COLOR_BITE = new Color(1f, 1f, 1f);
	public static readonly Color COLOR_SPEED = new Color(1f, 1f, 1f);

	public static readonly Color COLOR_GOLD = new Color(1f, 0.7f, 0.2f);
	public static readonly Color COLOR_PC = new Color(1f, 0f, 0.5f);

	public static readonly Color COLOR_CHESTS = new Color(1f, 0.6f, 0f);
	public static readonly Color COLOR_EGGS = new Color(0.75f, 0.33f, 1f);

	public static readonly Color COLOR_MINE = new Color(0.8f, 0.8f, 0.8f);
	public static readonly Color COLOR_POISON = new Color(0.6f, 1f, 0.1f);
	public static readonly Color COLOR_OBSTACLE = new Color(0.7f, 0.7f, 0.6f);
	public static readonly Color COLOR_ENTITY = new Color(0.2f, 0.8f, 0.9f);
	public static readonly Color COLOR_WATER = new Color(0.2f, 0.8f, 0.9f);

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
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

		string disguise = UsersManager.currentUser.GetEquipedDisguise(dragonSku);
		int level = UsersManager.currentUser.wardrobe.GetDisguiseLevel(disguise);

		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguise);

		if (def != null) {
			def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, def.GetAsString("powerupSet"));

			for( int i = 0; i<level;i++ )
			{
				string powerUp = def.Get("powerup"+(i+1).ToString());
				if ( !string.IsNullOrEmpty(powerUp))
					SetPowerUp(powerUp);
			}
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
					player.SetHealthModifier( def.GetAsFloat("param1"));
				}break;
				case "boost_increase":
				{
					player.SetBoostModifier( def.GetAsFloat("param1"));
				}break;
				case "fury_increase":
				{
					/*
					DragonBreathBehaviour breath = player.GetComponent<DragonBreathBehaviour>();
					if ( breath != null )
						breath.SetFuryModifier( def.GetAsFloat("param1") );
						*/
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
							player.SetMineShields( numHits );
						}break;
						case "poison":
						{
						}break;
					}
				}break;
				case "lives":
				{
					int numExtraLives = def.GetAsInt("param1");
					player.SetFreeRevives( numExtraLives );
				}break;
				case "dragonram":
				{
					int increase = def.GetAsInt("param1");
					player.SetOnBreakIncrease( increase );
				}break;
				case "preyHpBoost":
				{
					string from = def.Get("param1");
					float percentage = def.GetAsFloat("param2");
					PlayerEatBehaviour eatBehaviour = GetComponent<PlayerEatBehaviour>();
					eatBehaviour.AddEatingBost( from, percentage);
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
