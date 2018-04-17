/// <summary>
/// This class is responsible for controlling a popup that can be used to show any message. It consists of the following elements that can be configurated:
/// 1)Title.
/// 2)Message to be shown in the body.
/// 3)Buttons. Two layouts are supported:
///    3.1)Two buttons: Confirm, cancel. A delegate can be assigned to each.
///    3.2)A single button: Confirm. A delegate can be assinged to it.
/// 
/// The configuration of the class is done by using the class <c>PopupMessage.Config</c>
/// </summary>

using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PopupController))]
public class PopupMessage : IPopupMessage
{
    public const string PATH = "UI/Popups/Message/PF_PopupMessage";

    [SerializeField]
    private Localizer m_titleText;

    [SerializeField]
    private Localizer m_messageText;

    [SerializeField]
    private Localizer m_buttonCancelText;

    [SerializeField]
    private Localizer m_buttonConfirmCenterText;

    [SerializeField]
    private Localizer m_buttonConfirmRightText;

	override protected void ConfigureTexts(Config _config) {
		m_titleText.gameObject.SetActive(_config.ShowTitle);
		m_titleText.enabled = true;
		// Tid has priority over the plain text
		if (_config.TitleTid != null)
		{
			m_titleText.Localize(_config.TitleTid);
		}
		else if (_config.TitleText != null)
		{
			m_titleText.text.text = _config.TitleText;
			m_titleText.enabled = false;
		}

		if (_config.MessageTid != null)
		{
			if (_config.MessageParams == null)
			{
				m_messageText.Localize(_config.MessageTid);
			}
			else
			{
				m_messageText.Localize(_config.MessageTid, _config.MessageParams);
			}
		}
		else if (_config.MessageText != null)
		{
			m_messageText.text.text = _config.MessageText;
		}

		// Button texts
		switch (_config.ButtonMode)
		{            
			case Config.EButtonsMode.Confirm:
			{
				// Center button chosen since there's only one
				m_buttonConfirmCenterText.Localize(_config.ConfirmButtonTid);
			}
			break;

			case Config.EButtonsMode.ConfirmAndCancel:
			case Config.EButtonsMode.ConfirmAndExtraAndCancel:
			{
				if (m_buttonCancelText != null)
				{
					m_buttonCancelText.Localize(_config.CancelButtonTid);
				}

				// Confirm button: the right button is used because there are two buttons
				if (m_buttonConfirmRightText != null)
				{
					m_buttonConfirmRightText.Localize(_config.ConfirmButtonTid);
				}

				if (_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel)
				{
					m_buttonConfirmCenterText.Localize(_config.ExtraButtonTid);
				}
			}
			break;
		} 
	}
}
