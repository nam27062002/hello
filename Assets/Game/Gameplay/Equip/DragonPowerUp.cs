using UnityEngine;
using System.Collections;

public class DragonPowerUp : MonoBehaviour {

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

		string disguise = Wardrobe.GetEquipedDisguise(dragonSku);
		int level = Wardrobe.GetDisguiseLevel(disguise);

		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES, disguise);

		if (def != null) {
			def = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, def.GetAsString("powerupSet"));

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
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);

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
					DragonEatBehaviour eatBehaviour = GetComponent<DragonEatBehaviour>();
					eatBehaviour.AddEatingBost( from, percentage);
				}break;
			}
		}
	}

	/// <summary>
	/// Gets the description of this powerup, properly translated and formatted 
	/// depending on powerup type.
	/// </summary>
	public static string GetDescription(string _powerSku) {
		// Get definition
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, _powerSku);
		if(def == null) return "";

		// Every power type has a different format
		switch(def.GetAsString("type")) {
			case "hp_increase":	 {
				return def.GetLocalized("tidDesc", StringUtils.FormatNumber(def.GetAsFloat("param1"), 0));
			} break;

			case "boost_increase": {
				return def.GetLocalized("tidDesc", StringUtils.FormatNumber(def.GetAsFloat("param1"), 0));
			} break;

			case "fury_increase": {
				return def.GetLocalized("tidDesc", StringUtils.FormatNumber(def.GetAsFloat("param1"), 0));
			} break;

			case "dive": {
				return def.GetLocalized("tidDesc");
			} break;
			
			case "avoid": {
				// Check sub type with param one
				string subtype = def.Get("param1");
				int numHits = def.GetAsInt("param2");
				switch(subtype) {
					case "mine": {
						return def.GetLocalized("tidDesc", StringUtils.FormatNumber(numHits));
					} break;

					case "poison": {
						return def.GetLocalized("tidDesc", StringUtils.FormatNumber(numHits));
					} break;

					default: {
						return def.GetLocalized("tidDesc");
					} break;
				}
			} break;

			case "lives": {
				return def.GetLocalized("tidDesc", StringUtils.FormatNumber(def.GetAsInt("param1")));
			} break;

			case "dragonram": {
				return def.GetLocalized("tidDesc", StringUtils.FormatNumber(def.GetAsInt("param1")));
			} break;
			
			case "preyHpBoost": {
				// Show target entity name
				// [AOC] TODO!! Plural
				DefinitionNode entityDef = DefinitionsManager.GetDefinition(DefinitionsCategory.ENTITIES, def.GetAsString("param1"));
				return def.GetLocalized("tidDesc", entityDef.GetLocalized("tidName"), StringUtils.FormatNumber(def.GetAsFloat("param2"), 0));
			} break;

			default: {
				return def.GetLocalized("tidDesc");
			} break;
		}

		return "";
	}
}
