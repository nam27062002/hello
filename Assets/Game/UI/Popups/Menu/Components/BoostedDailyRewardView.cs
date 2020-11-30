// DailyRewardView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/02/2019.
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
/// View controller for a boosted daily reward item. We extend the regular daily reward
/// but we use a different settings scriptable object.
/// </summary>
public class BoostedDailyRewardView : DailyRewardView
{
    //------------------------------------------------------------------------//
    // OVERRIDE PARENT															  //
    //------------------------------------------------------------------------//
    private BoostedDailyRewardViewSettings m_settings = null;
    protected override DailyRewardViewSettings settings
    {
        get
        {
            if (m_settings == null)
            {
                m_settings = Resources.Load<BoostedDailyRewardViewSettings>(BoostedDailyRewardViewSettings.PATH);
            }
            return m_settings;
        }
    }

   
}