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
					player.SetHealthModifier( def.GetAsFloat("param1") );
				}break;
				case "boost_increase":
				{
					player.SetBoostModifier( def.GetAsFloat("param1") );
				}break;
				case "fury_increase":
				{
					player.SetFuryModifier( def.GetAsFloat("param1") );
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



}
