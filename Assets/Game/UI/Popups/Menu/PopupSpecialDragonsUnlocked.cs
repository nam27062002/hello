// PopupLabIntro.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupSpecialDragonsUnlocked : MonoBehaviour
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
    public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoLegendaryDragon";

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The popup is about to be opened.
    /// </summary>
    public void OnOpenPreAnimation()
    {
        // Mark tutorial as completed
        // Do it now to make sure that no one triggers the popup again!
        UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.SPECIAL_DRAGONS_UNLOCKED, true);
    }


    //------------------------------------------------------------------------//
    // METHODS																  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Check whether the popup must be triggered considering the current profile.
    /// </summary>
    /// <returns>Must the popup be displayed?</returns>
    public static bool Check()
    {
        // Don't if already displayed
        if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SPECIAL_DRAGONS_UNLOCKED))
        {
            return false;
        }

        // If any special dragon is available now, return true
        List<IDragonData> specialDragons = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL);
        for (int i = 0; i < specialDragons.Count; ++i)
        {
            if (specialDragons[i].lockState == IDragonData.LockState.AVAILABLE)
            {
                return true;
            }
        }

        // All special dragons are still locked. Don't show the popup
        return false;
    }


    /// <summary>
    /// Initialize this popup
    /// </summary>
    /// <param name="_sourceScreen">The screen that triggered this popup.</param>
    public void Init(MenuScreen _sourceScreen)
    {
    }


    /// <summary>
    /// The buton "Show me" has been clicked
    /// </summary>
    public void OnShowMeClicked ()
    {
        
        // Scroll to the first special dragon
        List<IDragonData> dragons = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL);
        InstanceManager.menuSceneController.SetSelectedDragon(dragons[0].sku);

        // Close the popup
        GetComponent<PopupController>().Close(true);

    }
}
