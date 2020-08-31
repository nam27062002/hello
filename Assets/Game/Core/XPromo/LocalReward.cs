// XPromoReward.cs
// Hungry Dragon
// 
// Created by  on 28/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace XPromo
{
	/// <summary>
	/// Represents a local xpromo reward. Its destination could be HD or HSE.
	/// </summary>
	[Serializable]
	public class LocalReward
	{

		//------------------------------------------------------------------------//
		// ENUM                     											  //
		//------------------------------------------------------------------------//
		public enum Destination
		{
			HD,
			HSE
		}

		//------------------------------------------------------------------------//
		// CONSTS               												  //
		//------------------------------------------------------------------------//

		// Tracking constants
		private const string DEFAULT_SOURCE = "";

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//

		protected bool m_enabled;
        protected int m_day;
		protected XPromoManager.Game m_destination;

        public bool enabled { get => m_enabled;}


        //------------------------------------------------------------------------//
        // GENERIC METHODS														  //
        //------------------------------------------------------------------------//




        //------------------------------------------------------------------------//
        // OTHER METHODS														  //
        //------------------------------------------------------------------------//

        /// <summary>
        /// Creates a new local Reward initialized with the data in the given Definition from rewards table.
        /// This local reward could be destinated to HD or to HSE.
        /// </summary>
        /// <param name="_def">Definition from localRewards table.</param>
        /// <returns>New reward created from the given definition.</returns>
        public static LocalReward CreateLocalRewardFromDef(DefinitionNode _def)
		{


			// For which game is destinated this reward?
			XPromoManager.Game destination = XPromoManager.Game.UNDEFINED;
            switch (_def.GetAsString("game"))
            {
				case XPromoManager.GAME_CODE_HD:
					destination = XPromoManager.Game.HD;
					break;
				case XPromoManager.GAME_CODE_HSE:
					destination = XPromoManager.Game.HSE;
					break;
				default:
                    // This shouldnt happen
					Debug.Log("The game destination " + _def.GetAsString("game")  + " is not defined");
					return null;
			}


			LocalReward localReward = null;

            if (destination == XPromoManager.Game.HD)
            {
                // This reward will be received in HD
				localReward = new LocalRewardHD();

				Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
				rewardData.typeCode = _def.GetAsString("type");
				rewardData.amount = _def.GetAsLong("amount");
				rewardData.sku = _def.GetAsString("rewardSku");

				// Assign an economy group based on the xpromo reward origin
				HDTrackingManager.EEconomyGroup economyGroup;
				economyGroup = HDTrackingManager.EEconomyGroup.REWARD_XPROMO_LOCAL;

				// Construct the reward object
				((LocalRewardHD)localReward).reward =  Metagame.Reward.CreateFromData(rewardData, economyGroup, DEFAULT_SOURCE);

			}
            else if(destination == XPromoManager.Game.HSE)
            {
				// This reward will be received in HSE
				localReward = new LocalRewardHSE();

				// Specific HSE reward params:
				((LocalRewardHSE)localReward).rewardSku = _def.GetAsString("rewardSku");
				((LocalRewardHSE)localReward).title_tid = _def.GetAsString("tid");
				((LocalRewardHSE)localReward).icon = _def.GetAsString("icon");
			}
            

			// Shared parameters:
			localReward.m_destination = destination;
			localReward.m_day = _def.GetAsInt("day");
			localReward.m_enabled = _def.GetAsBool("enabled");

			return localReward;
                       
		}

	}
}