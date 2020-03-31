﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This component will show/hide the object depending on the value of a boolean
/// setting in the current flavour configuration
/// </summary>
public class ToggleByFlavour : MonoBehaviour
{

    //------------------------------------------------------------------------//
    // ENUMS		    													  //
    //------------------------------------------------------------------------//


    //------------------------------------------------------------------------//
    // PROPERTIES		    											     //
    //------------------------------------------------------------------------//

    [SerializeField] private Flavour.SettingKey m_settingKey;
    [SerializeField] private bool m_settingValue;

    public bool settingValue { get { return m_settingValue; }  set { m_settingValue = value; } }



    /// <summary>
    /// Check the flavour settings in the Awake method. Flavours wont change during the game.
    /// </summary>
    public void Awake()
    {

        Flavour currentFlavour = FlavourManager.Instance.GetCurrentFlavour();
        bool settingValue = currentFlavour.GetSetting<bool>(m_settingKey);

        gameObject.SetActive(settingValue == m_settingValue);

    }
}
