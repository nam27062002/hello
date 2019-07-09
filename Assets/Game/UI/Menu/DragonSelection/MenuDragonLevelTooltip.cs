// MenuDragonLevelTooltip.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 08/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonLevelTooltip : UITooltipMultidirectional
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [Separator("MenuDragonLevelBarTooltip")]
    [SerializeField] private TextMeshProUGUI m_unlockLevelText = null;
    [SerializeField] private UISpriteAddressablesLoader m_skinIcon = null;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize the tooltip with the given texts and icons.
    /// If the tooltip has no textfields or icon assigned, will be ignored.
    /// If a text or icon is left empty, its corresponding game object will be disabled.
    /// </summary>
    /// <param name="_title">Title string.</param>
    /// <param name="_powerUpDescriptiontext">Text string.</param>
    /// <param name="_skinIconId">Addressible ID of the skin icon</param>
    /// <param name="_powerUpIcon">Power up icon sprite.</param>
    /// <param name="_levelRequiredText">Level required</param>
    public void Init(string _title, string _powerUpDescriptiontext, string _skinIconId, Sprite _powerUpIcon, string _levelRequiredText)
    {
        // Title
        if (m_titleText != null)
        {
            m_titleText.text = _title;
            m_titleText.gameObject.SetActive(!string.IsNullOrEmpty(_title));
        }

        // Message
        if (m_messageText != null)
        {
            m_messageText.text = _powerUpDescriptiontext;
            m_messageText.gameObject.SetActive(!string.IsNullOrEmpty(_powerUpDescriptiontext));
        }

        // Skin Icon
        if (m_skinIcon != null)
        {
            if (_skinIconId != null)
            {
                m_skinIcon.LoadAsync(_skinIconId);
            }
            m_skinIcon.gameObject.SetActive(_skinIconId != null);
        }

        // Powerup Icon
        if (m_icon != null)
        {
            if (_powerUpIcon != null)
            {
                m_icon.sprite = _powerUpIcon;
                m_icon.color = Color.white;
            }
            m_icon.gameObject.SetActive(_powerUpIcon != null);
        }

        // Unlock level
        if (m_unlockLevelText != null)
        {
            m_unlockLevelText.text = _levelRequiredText;
        }


    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}