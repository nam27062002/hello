// XPromoRewardPreview.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 04/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using XPromo;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Represents a preview of a Xpromo remoward, could be for HSE or HD
/// </summary>
public class XPromoRewardPreview : MetagameRewardView {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

    public const string HSE_REWARD_ICONS_PATH = "UI/Menu/XPromo/";

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [Separator("XPromo reward specifics")]

	[SerializeField]
	private Image m_gameLogo;

	[SerializeField]
	private GameObject m_tickIcon;

	[SerializeField]
	private GameObject m_glow;

	[SerializeField]
	private GameObject m_particlesFX;

    // Internal
    private LocalReward m_localReward;

    private LocalReward.State m_state;
    public LocalReward.State state
    {
        set
        {
            m_state = value;
        }
    }

    // The preview has been selected
    private bool m_selected;
    public bool selected
    {
        set
        {
            m_selected = value;
        }
    }


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize all the elements from the reward data
    /// </summary>
    /// <param name="_reward">The local reward represented in this preview</param>
    public void Init(XPromo.LocalReward _reward)
    {
        // The local Xpromo reward
        m_localReward = _reward;

        if (_reward is LocalRewardHD)
        {
            // The metagame.Reward object contained in a local HD Xpromo reward
            m_reward = ((LocalRewardHD)m_localReward).reward;
        }

    }


    /// <summary>
    /// Update visuals
    /// </summary>
    public override void Refresh()
    {
        // Common stuff
        m_tickIcon.SetActive(m_state == LocalReward.State.COLLECTED);
        m_glow.SetActive(m_state == LocalReward.State.READY);

        // Show particles when selected only
        m_particlesFX.SetActive(m_selected);



        if (m_localReward is LocalRewardHD)
        {

            // Use parent method to load the proper icon and text
            base.Refresh();

            // Get the localized logo filename
            string logoName = GetLocalizedLogoFilename(XPromoManager.Game.HD);

            // Load the new logo
            m_gameLogo.sprite = Resources.Load<Sprite>(UIConstants.HD_LOGO_PATH + logoName);

            

        }
        else if (m_localReward is LocalRewardHSE)
        {
            // Load the reward icon
            string rewardIcon = ((LocalRewardHSE)m_localReward).icon;
            Sprite sprite = Resources.Load<Sprite>(HSE_REWARD_ICONS_PATH + rewardIcon);

            m_icon.sprite = sprite;

            // Just show the title defined in the content
            m_rewardText.text = LocalizationManager.SharedInstance.Localize(((LocalRewardHSE)m_localReward).title_tid);


            // Get the localized logo filename
            string logoName = GetLocalizedLogoFilename(XPromoManager.Game.HSE);

            // Load the new logo
            m_gameLogo.sprite = Resources.Load<Sprite>(UIConstants.HSE_LOGO_PATH + logoName);

        }



}

    /// <summary>
    /// Return the sprite name of the game logo as defined in localization definitions.
    /// It will be localized according to the current language.
    /// </summary>
    /// <param name="_game">HSE or HD</param>
    /// <returns></returns>
    private string GetLocalizedLogoFilename (XPromoManager.Game _game)
    {


        // Get new language definition
        string langSku = LocalizationManager.SharedInstance.GetCurrentLanguageSKU();
        DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, langSku);
        if (langDef == null) return "";

        if (_game == XPromoManager.Game.HD)
        {
            // Find hungry dragon logo for the current language
            return langDef.GetAsString("hseLogo");
        }
        else if (_game == XPromoManager.Game.HSE)
        {
            // Find hungry shark logo for the current language
            return langDef.GetAsString("logo");
        }

        // Something went wrong.
        return "";
        
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}