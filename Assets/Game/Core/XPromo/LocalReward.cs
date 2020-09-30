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
		// ENUM     															  //
		//------------------------------------------------------------------------//

		public enum State
		{
			COLLECTED,
			READY,
			COUNTDOWN,
			LOCKED
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
		public bool enabled { get { return m_enabled; } }

		protected int m_day;
        public int day { get { return m_day;  } }

		protected string m_sku;
		public string sku { get { return m_sku; } }

		protected int m_altRewardSC;
        public int altRewardSC { get { return m_altRewardSC; } }

		protected int m_altRewardPC;
		public int altRewardPC { get { return m_altRewardPC; } }        

		protected XPromoManager.Game m_destination;
		public XPromoManager.Game destination { get { return m_destination; } }

		protected XPromoCycle.ABGroup m_abGroup = XPromoCycle.ABGroup.UNDEFINED;
		public XPromoCycle.ABGroup abGroup { get { return m_abGroup; } }

		protected DefinitionNode m_def;
        public DefinitionNode def { get { return m_def; } }

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
			XPromoManager.Game destination = XPromoManager.GameStringToEnum(_def.GetAsString("destination"));

            if (destination == XPromoManager.Game.UNDEFINED)
            {
				// This shouldnt happen
				Debug.Log("The game destination " + _def.GetAsString("destination") + " is not defined");
				return null;
			}
            

			LocalReward localReward = null;



            if (destination == XPromoManager.Game.HD)
            {
                // This reward will be received in HD
				localReward = new LocalRewardHD();


				localReward.m_altRewardSC = _def.GetAsInt("altSC");
				localReward.m_altRewardPC = _def.GetAsInt("altPC");


				// Construct the reward object
				Metagame.Reward content = XPromoManager.CreateRewardFromDef(_def);
				((LocalRewardHD)localReward).reward = content;

			}
            else if(destination == XPromoManager.Game.HSE)
            {
				// This reward will be received in HSE
				localReward = new LocalRewardHSE();

				// Specific HSE reward params:
				((LocalRewardHSE)localReward).rewardSku = _def.GetAsString("rewardSku");
				((LocalRewardHSE)localReward).title_tid = _def.GetAsString("tid");
				((LocalRewardHSE)localReward).icon = _def.GetAsString("icon");
				((LocalRewardHSE)localReward).amount = _def.GetAsInt("amount");
			}

			// Keep a copy of the definition node
			localReward.m_def = _def;

			// Shared parameters:
			localReward.m_destination = destination;
			localReward.m_day = _def.GetAsInt("day");
			localReward.m_enabled = _def.GetAsBool("enabled");
			localReward.m_sku = _def.sku;
			localReward.m_abGroup = XPromoCycle.StringToABGroup(_def.GetAsString("abGroup", ""));

			return localReward;
                       
		}

		/// <summary>
		/// Compare two definitions by day
		/// </summary>
		/// <param name="_def1">First definitions to be compared.</param>
		/// <param name="_def2">Second definitions to be compared.</param>
		/// <returns>The result of the comparison (-1, 0, 1).</returns>
		public static int CompareDefsByDay(DefinitionNode _def1, DefinitionNode _def2)
		{
            int result = _def1.GetAsFloat("day", 0).CompareTo(_def2.GetAsFloat("day", 0));
			return result;
		}

		

	}
}