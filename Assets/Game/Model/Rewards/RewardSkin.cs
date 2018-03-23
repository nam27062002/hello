// RewardSkin.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 22/03/2018.
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
	public class RewardSkin : Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public const string TYPE_CODE = "skin";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from skin sku.
		/// </summary>
		/// <param name="_sku">skin sku.</param>
		public RewardSkin(string _sku, string _source) {
			m_source = _source;
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _sku);
			InitFrom(def);
		}

		/// <summary>
		/// Constructor from skin definition.
		/// </summary>
		/// <param name="_def">Skin definition.</param>
		public RewardSkin(DefinitionNode _def, string _source) {
			m_source = _source;
			InitFrom(_def);
		}

		/// <summary>
		/// Internal initializer from pet definition.
		/// </summary>
		/// <param name="_def">Skin definition.</param>
		private void InitFrom(DefinitionNode _def) {
			base.Init(TYPE_CODE);

			m_sku = _def.sku;
			m_def = _def;

			m_rarity = Rarity.COMMON;	// Skins don't have rarity
		}

		/// <summary>
		/// Implementation of the abstract Collect() method.
		/// </summary>
		override protected void DoCollect() {
			// Own the skin
			UsersManager.currentUser.wardrobe.SetSkinState(m_sku, Wardrobe.SkinState.OWNED);

			// Immediately equip it!
			UsersManager.currentUser.EquipDisguise(m_def.GetAsString("dragonSku"), m_sku, true);

			// Notify game
			Messenger.Broadcast<string>(MessengerEvents.SKIN_ACQUIRED, m_sku);
		}
	}
}
