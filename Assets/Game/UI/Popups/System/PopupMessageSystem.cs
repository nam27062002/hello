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
public class PopupMessageSystem : IPopupMessage
{
	public const string PATH = "UI/Popups/Message/PF_PopupMessageSystem";

	[SerializeField]
	private Text m_titleText;

	[SerializeField]
	private Text m_messageText;

	[SerializeField]
	private Text m_buttonCancelText;

	[SerializeField]
	private Text m_buttonConfirmCenterText;

	[SerializeField]
	private Text m_buttonConfirmRightText;

	override protected void ConfigureTexts(Config _config) {
		m_titleText.gameObject.SetActive(_config.ShowTitle);
		// Tid has priority over the plain text
		if (_config.TitleTid != null)
		{
			m_titleText.text = LocalizationManager.SharedInstance.Localize(_config.TitleTid);
		}
		else if (_config.TitleText != null)
		{
			m_titleText.text = _config.TitleText;
		}

		if (_config.MessageTid != null)
		{
			if (_config.MessageParams == null)
			{
				m_messageText.text = LocalizationManager.SharedInstance.Localize(_config.MessageTid);
			}
			else
			{
				m_messageText.text = LocalizationManager.SharedInstance.Localize(_config.MessageTid, _config.MessageParams);
			}
		}
		else if (_config.MessageText != null)
		{
			m_messageText.text = _config.MessageText;
		}

		// Button texts
		switch (_config.ButtonMode)
		{            
			case Config.EButtonsMode.Confirm:
			{
				// Center button chosen since there's only one
				m_buttonConfirmCenterText.text = LocalizationManager.SharedInstance.Localize(_config.ConfirmButtonTid);
			}
			break;

			case Config.EButtonsMode.ConfirmAndCancel:
			case Config.EButtonsMode.ConfirmAndExtraAndCancel:
			{
				if (m_buttonCancelText != null)
				{
					m_buttonCancelText.text = LocalizationManager.SharedInstance.Localize(_config.CancelButtonTid);
				}

				// Confirm button: the right button is used because there are two buttons
				if (m_buttonConfirmRightText != null)
				{
					m_buttonConfirmRightText.text = LocalizationManager.SharedInstance.Localize(_config.ConfirmButtonTid);
				}

				if (_config.ButtonMode == Config.EButtonsMode.ConfirmAndExtraAndCancel)
				{
					m_buttonConfirmCenterText.text = LocalizationManager.SharedInstance.Localize(_config.ExtraButtonTid);
				}
			}
			break;
		} 
	}
}
