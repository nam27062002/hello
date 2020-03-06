// RewardDragon.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 21/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// Dragon skin reward.
	/// </summary>
	public class RewardDragon : Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "dragon";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from dragon sku.
		/// </summary>
		/// <param name="_sku">Dragon sku.</param>
		public RewardDragon(string _sku, string _source) {
			m_source = _source;
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
			InitFrom(def);
		}

		/// <summary>
		/// Constructor from dragon definition.
		/// </summary>
		/// <param name="_def">Dragon definition.</param>
		public RewardDragon(DefinitionNode _def, string _source) {
			m_source = _source;
			InitFrom(_def);
		}

		/// <summary>
		/// Internal initializer from dragon definition.
		/// </summary>
		/// <param name="_def">Dragon definition.</param>
		private void InitFrom(DefinitionNode _def) {
			base.Init(TYPE_CODE);

			m_sku = _def.sku;
			m_def = _def;

			m_rarity = Rarity.COMMON;	// Dragons don't have rarity
		}

		/// <summary>
		/// Checks whether this reward needs to be replaced and optionally creates a replacement
		/// reward if needed.
		/// </summary>
		/// <param name="_createReplacement">Optionally create the replacement reward also. Only supported by some types.</param>
		/// <returns>Whether this reward needs to be replaced or not.</returns>
		public override bool CheckReplacement(bool _createReplacement) {
			// If the dragon is already owned, give replacement instead
			bool duplicated = IsAlreadyOwned();

			// If duplicated, create alternative reward
			if(duplicated && _createReplacement) {
				// For dragons there is no generic replacement
			}

			return duplicated;
		}

		/// <summary>
		/// This method checks if the reward is already owned by the player. This is applicable in
		/// non-consumable items like dragons, skins and the remove ads offer. Currency packs will be always
		/// marked as not owned.
		/// </summary>
		/// <returns></returns>
		public override bool IsAlreadyOwned() {
			return DragonManager.IsDragonOwned(m_sku);
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			// Aux vars
			IDragonData dragonData = DragonManager.GetDragonData(m_sku);

			// Just acquire target dragon!
			dragonData.Acquire();

			// Track
			switch( dragonData.type )
			{
				default:
				case IDragonData.Type.CLASSIC:{
					HDTrackingManager.Instance.Notify_DragonUnlocked(dragonData.def.sku, dragonData.GetOrder());
				}break;
				case IDragonData.Type.SPECIAL:
				{
					// TODO. Removed to fix HDK-5276
				}break;
			}
				
		}

		/// <summary>
		/// Obtain the generic TID to describe this reward type.
		/// </summary>
		/// <returns>TID describing this reward type.</returns>
		/// <param name="_plural">Singular or plural TID? (Not supported for Dragon type rewards.</param>
		public override string GetTID(bool _plural) {
			// Return dragon name (no support for plural)
			return m_def.GetAsString("tidName");
		}
	}
}
