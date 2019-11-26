// RewardRemoveAds.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 26/11/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;


namespace Metagame {

    //----------------------------------------------------------------------------//
    // CLASSES																	  //
    //----------------------------------------------------------------------------//
    /// <summary>
    /// Remove ads reward
    /// </summary>
    public class RewardRemoveAds:Reward {
        //------------------------------------------------------------------------//
        // CONSTANTS															  //
        //------------------------------------------------------------------------//
        public const string TYPE_CODE = "removeAds";


        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//

        //------------------------------------------------------------------------//
        // GENERIC METHODS														  //
        //------------------------------------------------------------------------//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RewardRemoveAds() {

	    }

	    /// <summary>
	    /// Destructor
	    /// </summary>
	    ~RewardRemoveAds() {

	    }

        //------------------------------------------------------------------------//
        // OTHER METHODS														  //
        //------------------------------------------------------------------------//




        //------------------------------------------------------------------------//
        // IMPLEMENT PARENT METHODS												  //
        //------------------------------------------------------------------------//

        /// <summary>
        /// Implementation of the abstract Collect() method.
        /// </summary>
        override protected void DoCollect()
        {
            // Activate the remove ads feature
            UsersManager.currentUser.removeAds.SetActive(true);

            // [JOM] Do we need some tracking here?
        }

        /// <summary>
        /// Obtain the generic TID to describe this reward type.
        /// </summary>
        /// <returns>TID describing this reward type.</returns>
        /// <param name="_plural">Singular or plural TID?</param>
        public override string GetTID(bool _plural)
        {
            // This method wont be used for this particular reward
            return "";
        }

    }
}