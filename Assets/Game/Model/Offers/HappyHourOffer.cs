// HappyHourOffer.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 16/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Trigger a discounted pack of gems for a player who just bought a pack of gems in the shop.
/// The offer is available for only "x" minutes.
///  https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+9.+Happy+Hour+IAP
/// </summary>
[Serializable]
public class HappyHourOffer {
    

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    private DefinitionNode m_def;
        
    // Cached values from the definition node:
    private float m_offerDurationMinutes; // Total duration of a happy hour offer
    private float m_percentageMinExtraGems; 
    private float m_percentageMaxExtraGems;
    private float m_percentageExtraGemsIncrement; // Accumulative increment in the extra gems each time the happy hour is renewed
    private int m_triggerRunNumber;

    // Current offer values:

    private DateTime m_expirationTime = DateTime.MinValue; // Timestamp when the offer will finish
    public DateTime expirationTime
    {   get
        {
            return m_expirationTime;
        }
        set
        {
            m_expirationTime = value;
        }
    }


    private float m_extraGemsFactor; // The current extra gem multiplier for this offer
    public float extraGemsFactor
    {   get
        {
            return m_extraGemsFactor;
        }
        set
        {
            m_extraGemsFactor = value;
        }
    }


    private bool m_pendingPopup = false; // Wheter the popup was already shown or is still pending in the queue
    public bool pendingPopup
    {   get
        {
            return m_pendingPopup;
        }
        set
        {
            m_pendingPopup = value;

            // Save it to persistence, so the popup wont be shown again
            Save(); 
        }
    }



    private int m_triggerPopupAtRun; // Runs needed before showing the happy hour offer
    public int triggerPopupAtRun
    {   get
        {
            return m_triggerPopupAtRun;
        }
    }

    private string m_lastOfferSku; // Keep a track of the offer that triggered the happy hour, so it can be displayed in the popup
    public string lastOfferSku
    {   get
        {
            return m_lastOfferSku;
        }
    }

    //------------------------------------------------------------------------//
    // STATIC   															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Factory pattern to build the happy hour from definition
    /// </summary>
    public static HappyHourOffer CreateFromDefinition()
    {
        DefinitionNode def;
        List<DefinitionNode> happyHourDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.HAPPY_HOUR);

        // No definition found
        if (happyHourDefs.Count == 0)
        {
            Debug.LogError("Couldn't find any definition of happy hour in the content");
            return null;
        }

        // Happy hours definitions only should contain one row
        def = happyHourDefs[0];

        // In case the happy hour is not enabled (featured) return null
        if (!def.GetAsBool("featured"))
        {
            return null;
        }

        HappyHourOffer happyHour = new HappyHourOffer();

        // Initialize the object from the definition node
        happyHour.InitializeFromDefinition(def);

        return happyHour;
    }


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Default constructor.
    /// </summary>
    private HappyHourOffer() {
        
        // Subscribe to events
        Messenger.AddListener<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, OnHcPackAccquired);

    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~HappyHourOffer ()
    {
        // Unsubscribe to events
        Messenger.RemoveListener<bool, string>(MessengerEvents.HC_PACK_ACQUIRED, OnHcPackAccquired);
    }


    /// <summary>
    /// Initialize the happy hour with the values in the definition node. If
    /// there are values stored in the persistence, load them too.
    /// </summary>
    private void InitializeFromDefinition(DefinitionNode _def) {

        m_def = _def;


        // Initialize definition values from definition
        m_triggerRunNumber = m_def.GetAsInt("triggerRunNumber");
        m_offerDurationMinutes = m_def.GetAsInt("happyHourTimer");
        m_percentageMinExtraGems = m_def.GetAsFloat("percentageMinExtraGems");
        m_percentageMaxExtraGems = m_def.GetAsFloat("percentageMaxExtraGems");
        m_percentageExtraGemsIncrement = m_def.GetAsFloat("percentageIncrement");

        // Load persisted data (if any)
        Load();

    }


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// The happy hour was activated by the player
    /// It could be already active, in that case, we extend it and increment the extra gems
    /// </summary>
    public void StartOffer()
    {

        if (m_offerDurationMinutes > 0)
        {

            if (IsActive())
            {
                // Increment the extra gems each time the happy hour is reactivated
                m_extraGemsFactor += m_percentageExtraGemsIncrement;

                // Cap the value to the maximum
                if (m_extraGemsFactor > m_percentageMaxExtraGems)
                {
                    m_extraGemsFactor = m_percentageMaxExtraGems;
                }
            }
            else
            {
                // The offer was not active, so starts with the minimum value
                m_extraGemsFactor = m_percentageMinExtraGems;
            }


            // Extend the expiration time of this offer
            DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
            expirationTime = serverTime.AddMinutes(m_offerDurationMinutes);

            // The popup will be delayed to be shown after X runs
            m_triggerPopupAtRun = UsersManager.currentUser.gamesPlayed + m_triggerRunNumber;

        }

    }


    /// <summary>
    /// Return wether the happy hour is active at this moment or not
    /// </summary>
    public bool IsActive()
    {
        return (TimeLeftSecs() > 0);
    }


    /// <summary>
    /// The total amount of seconds left for this happy hour
    /// Returns negative values if the happy hour is expired or inactive
    /// </summary>
    public double TimeLeftSecs()
    {
        DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();

        return expirationTime.Subtract(serverTime).TotalSeconds;

    }


    /// <summary>
    /// Finish the active happy hour and store the changes in the persistence
    /// </summary>
    public void EndOffer ()
    {
        if (IsActive())
        {
            ResetValues();
            Save();
        }
    }


    /// <summary>
    /// Reset current offer values to its default
    /// </summary>
    public void ResetValues ()
    {
        m_expirationTime = DateTime.MinValue;
        m_extraGemsFactor = 0;
        m_pendingPopup = false;
    }

    /// <summary>
    /// Returns false if the popup wants to be shown right after the buy
    /// returns true if we are delaying after X runs
    /// </summary>
    public bool IsPopupDelayed ()
    {
        return m_triggerRunNumber > 0;
    }


    /// <summary>
    /// Returns the elapsed time for this offer from 0 to 1
    /// 0 meaning just started, and 1 meaning offer reching the expiration date
    /// </summary>
    public float GetElapsedTimeNormalized ()
    {

        double offerDuration = (m_offerDurationMinutes * 60);
        double elapsedSeconds = offerDuration - TimeLeftSecs();

        // Return normalized value
        return (float) (elapsedSeconds / offerDuration);

    }


    /// <summary>
    /// Save happy hour offer values in the user profile
    /// </summary>
    private void Save ()
    {
        UsersManager.currentUser.SaveHappyHour(this);
    }

    /// <summary>
    /// Initialize this object from the persistence
    /// </summary>
    private void Load()
    {
        UsersManager.currentUser.LoadHappyHour(this);
    }

    /// <summary>
    /// Check if there is a happy hour offer active and apply the extra amount
    /// </summary>
    public int ApplyHappyHourExtra(int _amount)
    {        
        // Is there a happy hour offer?
        HappyHourOffer happyHour = OffersManager.instance.happyHour;
        if (happyHour.IsActive())
        {
            // Apply the extra gems factor
            return Mathf.RoundToInt((_amount) * (1 + happyHour.extraGemsFactor));
        }

        return _amount;
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Called when the player buys a gem pack
    /// </summary>
    private void OnHcPackAccquired(bool _forcePopup, string _offerSku)
    {
        // Restart the happy hour timer
        StartOffer();

        // Keep a track of the offer that triggered the popup
        m_lastOfferSku = _offerSku;

        // If the popup doesnt need te be delayed
        if (_forcePopup && m_triggerRunNumber == 0)
        {

            // Load the popup
            PopupController popup = PopupManager.LoadPopup(PopupHappyHour.PATH);
            PopupHappyHour popupHappyHour = popup.GetComponent<PopupHappyHour>();

            // Initialize the popup (set the discount % values)
            popupHappyHour.Init(m_lastOfferSku);

            // And launch it
            popup.Open();

        }
        else
        {

            

            // Try to show the happy hour popup
            m_pendingPopup = true;

        }

        // Save in persistence
        Save();
    }
}