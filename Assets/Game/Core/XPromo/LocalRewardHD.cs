// HDReward.cs
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
namespace XPromo {
	/// <summary>
	/// Represents a x-promo reward obtained in HD (local) and to be claimed in this same game.
	/// </summary>
	[Serializable]
    public class LocalRewardHD : LocalReward{

        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//
        
        protected Metagame.Reward m_reward;
        public Metagame.Reward reward { get => m_reward; set => m_reward = value; }


        //------------------------------------------------------------------------//
        // GENERIC METHODS														  //
        //------------------------------------------------------------------------//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocalRewardHD() {

	    }


    }
}