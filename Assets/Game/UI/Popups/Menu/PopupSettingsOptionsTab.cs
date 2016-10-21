﻿// PopupSettingsOptionsTab.cs
// Hungry Dragon
// 
// Created by David Germade on 30th August 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling the options tab in the settings popup.
/// </summary>
public class PopupSettingsOptionsTab : MonoBehaviour
{    
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    [SerializeField]
    private Localizer m_languageText = null;

    // Internal
    private List<DefinitionNode> m_languageDefs;
    private int m_selectedIdx = -1;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Awake()
    {
        // Check required params
        Debug.Assert(m_languageText != null, "Required field");

        // Cache language definitions and init with selected language
        // [AOC] TODO!! Exclude languages marked to be excluded based on platform
        m_languageDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.LOCALIZATION, "iOS", "true");
        DefinitionsManager.SharedInstance.SortByProperty(ref m_languageDefs, "order", DefinitionsManager.SortType.NUMERIC);

        // Find current language
        for (int i = 0; i < m_languageDefs.Count; i++)
        {
            if (m_languageDefs[i].sku == LocalizationManager.SharedInstance.GetCurrentLanguageSKU())
            {
                m_selectedIdx = i;
                break;
            }
        }

        // Update text
        RefreshText();
    }

    /// <summary>
    /// Update textfield with currently selected language
    /// </summary>
    private void RefreshText()
    {
        m_languageText.Localize(m_languageDefs[m_selectedIdx].Get("tidName"));  // [AOC] CHECK!! Each language in its own language
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Select previous language.
    /// </summary>
    public void OnSelectPreviousLanguage()
    {
        // Change selected index
        m_selectedIdx--;
        if (m_selectedIdx < 0) m_selectedIdx = m_languageDefs.Count - 1;

        // Change localization!
        if (LocalizationManager.SharedInstance.SetLanguage(m_languageDefs[m_selectedIdx].sku))
        {
            PlayerPrefs.SetString(PopupSettings.KEY_SETTINGS_LANGUAGE, m_languageDefs[m_selectedIdx].sku);
        }

        Messenger.Broadcast(EngineEvents.LANGUAGE_CHANGED);
        // Update text!
        RefreshText();
    }

    /// <summary>
    /// Select next language.
    /// </summary>
    public void OnSelectNextLanguage()
    {
        // Change selected index
        m_selectedIdx++;
        if (m_selectedIdx >= m_languageDefs.Count) m_selectedIdx = 0;

        // Change localization!
        if (LocalizationManager.SharedInstance.SetLanguage(m_languageDefs[m_selectedIdx].sku))
        {
            PlayerPrefs.SetString(PopupSettings.KEY_SETTINGS_LANGUAGE, m_languageDefs[m_selectedIdx].sku);
        }

        Messenger.Broadcast(EngineEvents.LANGUAGE_CHANGED);
        // Update text!
        RefreshText();
    }
}