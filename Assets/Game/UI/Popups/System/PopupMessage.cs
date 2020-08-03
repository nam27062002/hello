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
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(PopupController))]
public class PopupMessage : IPopupMessage
{
    public const string PATH = "UI/Popups/Message/PF_PopupMessage";

	[Space]
    [SerializeField] private Localizer m_titleText;

    [SerializeField] private Localizer m_messageText;

    [SerializeField] private Localizer m_buttonCancelText;

	[FormerlySerializedAs("m_buttonConfirmCenterText")]
	[SerializeField] private Localizer m_buttonExtraText;

	[FormerlySerializedAs("m_buttonConfirmRightText")]
	[SerializeField] private Localizer m_buttonConfirmText;

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
		if(m_buttonCancelText != null) {
			switch(_config.ButtonMode) {
				case Config.EButtonsMode.ConfirmAndCancel:
				case Config.EButtonsMode.ConfirmAndExtraAndCancel: {
					m_buttonCancelText.Localize(_config.CancelButtonTid);
				} break;
			}
		}

		if(m_buttonConfirmText != null) {
			switch(_config.ButtonMode) {
				case Config.EButtonsMode.Confirm:
				case Config.EButtonsMode.ConfirmAndCancel:
				case Config.EButtonsMode.ConfirmAndExtra:
				case Config.EButtonsMode.ConfirmAndExtraAndCancel: {
					m_buttonConfirmText.Localize(_config.ConfirmButtonTid);
				} break;
			}
		}

		if(m_buttonExtraText != null) {
			switch(_config.ButtonMode) {
				case Config.EButtonsMode.ConfirmAndExtra:
				case Config.EButtonsMode.ConfirmAndExtraAndCancel: {
					m_buttonExtraText.Localize(_config.ExtraButtonTid);
				} break;
			}
		}
	}
}
