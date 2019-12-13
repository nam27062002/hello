// CPRemoveAds.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 06/11/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class CPRemoveAds : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [SerializeField]
    private Toggle toggleSelector;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	
	/// <summary>
	/// Component is enabled
	/// </summary>
    private void OnEnable()
    {
        toggleSelector.isOn = UsersManager.currentUser.removeAds.IsActive;
    }



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    public void OnClick()
    {
        // Toggle the UI checkbox
        toggleSelector.isOn = !toggleSelector.isOn;

        // Activate/deactivate the remove ads feature
        UsersManager.currentUser.removeAds.SetActive(toggleSelector.isOn);

        // Save the changes
        UsersManager.currentUser.Save();
        PersistenceFacade.instance.Save_Request();
    }
}