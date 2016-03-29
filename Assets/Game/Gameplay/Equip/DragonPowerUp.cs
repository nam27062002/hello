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
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, disguise);


		for( int i = 0; i<level;i++ )
		{
			string powerUp = def.Get("powerup"+(i+1).ToString());
			if ( !string.IsNullOrEmpty(powerUp))
				SetPowerUp(powerUp);
		}

	}

	void SetPowerUp( string powerUpSku )
	{
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);
		if ( def != null )
		{
			string type = def.Get("type");

			switch( type )
			{
				case "hp_increase":	// gives the player extra health
				{
					DragonPlayer player = GetComponent<DragonPlayer>();
					player.SetHealthModifier( def.GetAsFloat("param1") );
				}break;
				case "boost_increase":
				{
					DragonPlayer player = GetComponent<DragonPlayer>();
					player.SetBoostModifier( def.GetAsFloat("param1") );
				}break;
				case "fury_increase":
				{
					DragonPlayer player = GetComponent<DragonPlayer>();
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
						}break;
						case "poison":
						{
						}break;
					}
				}break;
				case "lives":
				{
					int numExtraLives = def.GetAsInt("param1");
					DragonPlayer player = GetComponent<DragonPlayer>();
					player.SetFreeRevives( numExtraLives );
				}break;
			}

		}

	}



}
