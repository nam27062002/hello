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
    /// Represents a x-promo reward obtained in HD (local) but to be claimed in HSE.
    /// </summary>
    [Serializable]
    public class LocalRewardHSE : LocalReward{

        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//
        private string m_rewardSku;
        private string m_icon;
        private string m_title_tid;

        public string rewardSku { get => m_rewardSku; set => m_rewardSku = value; }
        public string icon { get => m_icon; set => m_icon = value; }
        public string title_tid { get => m_title_tid; set => m_title_tid = value; }


        //------------------------------------------------------------------------//
        // GENERIC METHODS														  //
        //------------------------------------------------------------------------//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocalRewardHSE() {

	    }

	    //------------------------------------------------------------------------//
	    // OTHER METHODS														  //
	    //------------------------------------------------------------------------//

    }
}