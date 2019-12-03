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


        /// <summary>
        /// This method checks if the reward is already owned by the player. This is applicable in
        /// non-consumable items like dragons, skins and the remove ads offer. Currency packs will be always
        /// marked as not owned.
        /// </summary>
        /// <returns></returns>
        public override bool IsAlreadyOwned()
        {
            // If remove ads is active, means that the player already bought it
            return UsersManager.currentUser.removeAds.IsActive;
        }


        //------------------------------------------------------------------------//
        // GENERIC METHODS														  //
        //------------------------------------------------------------------------//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RewardRemoveAds() {
            base.Init(TYPE_CODE);
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