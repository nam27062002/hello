// CustomizerPopup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/04/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup controller for popups coming from the customizer.
/// </summary>
public class PopupCustomizer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static class FieldType {
		public static readonly string TEXT 		= "text";
		public static readonly string TEXTAREA 	= "textarea";
		public static readonly string IMAGE 	= "image";
		public static readonly string BUTTON 	= "button";
	}

	public static class CaletyKey {
		public static readonly string TITLE 	= "title";
		public static readonly string TEXT 		= "text";
		public static readonly string PICTURES	= "pictures";
		public static readonly string BUTTONS	= "buttons";
	}


	//------------------------------------------------------------------------//
	// NESTED CLASSES														  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public abstract class Field<T> {
		private string m_fieldType;
		public string fieldType { get { return m_fieldType; } }

		private string m_caletyKey;
		public string caletyKey { get { return m_caletyKey; } }

		public T element;

		//------------------------------------------------------//
		public Field() {}
		public Field(string _type, string _key) {
			m_fieldType = _type;
			m_caletyKey = _key;
		}
	}

	[System.Serializable]
	public class TitleField : Field<TextMeshProUGUI> {
		public TitleField() : base(FieldType.TEXT, CaletyKey.TITLE) {}
	}

	[System.Serializable]
	public class MessageField : Field<TextMeshProUGUI> {
		public MessageField() : base(FieldType.TEXTAREA, CaletyKey.TEXT) {}
	}

	[System.Serializable]
	public class TextField : Field<TextMeshProUGUI> {
		public TextField() : base(FieldType.TEXT, CaletyKey.TEXT) {}
	}

	[System.Serializable]
	public class ImageField : Field<RawImage> {
		public ImageField() : base(FieldType.IMAGE, CaletyKey.PICTURES) {}
	}

	[System.Serializable]
	public class ButtonField : Field<Button> {
		public bool optional = false;
		//------------------------------------------------------//
		public ButtonField() : base(FieldType.BUTTON, CaletyKey.BUTTONS) {}
	}

	[System.Serializable]
	public class CloseButtonField : Field<GameObject> {
		public bool optional = false;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// [AOC] TODO!! Support as many fields as desired. Not supported by Calety right now :(
	//[SerializeField] private FieldsDictionary m_dynamicFields = new FieldsDictionary();

	// Exposed
	[InfoBox("All fields optional")]
	[SerializeField] private TitleField m_titleField = null;
	[SeparatorAttribute]
	[SerializeField] private MessageField m_messageField = null;
	[SerializeField] private List<TextField> m_textFields = new List<TextField>();
	[SeparatorAttribute]
	[SerializeField] private ImageField m_imageField = null;
	[SeparatorAttribute]
	[SerializeField] private CloseButtonField m_closeButtonField = null;
	[SerializeField] private List<Button> m_buttons = new List<Button>();

	// Internal
	private CustomizerManager.CustomiserPopupConfig m_config = null;
	private CaletyConstants.PopupConfig m_localizedConfig = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Connect the buttons
		for(int i = 0; i < m_buttons.Count; ++i) {
			int btnIdx = i;	// Delta expressions and iterators -_-
			m_buttons[i].onClick.AddListener(() => { OnButton(btnIdx); });
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	public void OnDestroy() {
		
	}

	/// <summary>
	/// Initialize the popup given a configuration from the customizer.
	/// </summary>
	/// <param name="_config">Config.</param>
	public void InitFromConfig(CustomizerManager.CustomiserPopupConfig _config) {
		// Store config
		m_config = _config;

		// Find out localized config based on current selected language
		m_localizedConfig = m_config.m_kPopupConfigByLanguage[m_config.m_kPrepareState.m_strPreparedLanguage];

		// Close button
		if(m_closeButtonRoot != null) {
			m_closeButtonRoot.SetActive(m_config.m_bHasCloseButton);
		}

		// Title
		if(m_titleText != null) {
			m_titleText.text = m_localizedConfig.m_strTitle;
		}

		// Texts
		// Since Calety's customizer doesn't support multiple textfields, use the message as a texts dictionary
		JSONNode textsJson = null;
		try {
			textsJson = JSON.Parse(m_localizedConfig.m_strMessage);
		} catch {}

		//TODO: Parse again the full list of texts from Calety
		/*
		if(textsJson == null) {
			// If json couldn't be parsed from the given string, assume it's a simple message and put it in the main textfield
			if(m_messageText != null) {
				m_messageText.text = m_localizedConfig.m_strMessage;
			}
			if(m_otherTexts.dict.ContainsKey("message")) {
				m_otherTexts.dict["message"].text = m_localizedConfig.m_strMessage;
			}
		} else {
			// Iterate through all textfields in the popup and try to find a text for them in the json
			foreach(KeyValuePair<string, TextMeshProUGUI> kvp in m_otherTexts.dict) {
				if(kvp.Value == null) continue;	// Just in case

				// Set the text from the json, or empty string if the json doesn't have a text for this textfield
				if(textsJson.ContainsKey(kvp.Key)) {
					kvp.Value.text = textsJson[kvp.Key];
				} else {
					kvp.Value.text = string.Empty;
				}
			}
		}
		*/

		// Image
		if(m_image != null) {
			if(m_config.m_kUnityImageTexture != null) {
				m_image.gameObject.SetActive(true);
				m_image.texture = m_config.m_kUnityImageTexture;
			} else {
				m_image.gameObject.SetActive(false);
			}
		}

		// Buttons
		for(int i = 0; i < m_buttons.Count; ++i) {
			// Button defined?
			if(i >= m_localizedConfig.m_kPopupButtons.Count) {
				m_buttons[i].gameObject.SetActive(false);
			} else {
				m_buttons[i].gameObject.SetActive(true);

				// Set text
				TextMeshProUGUI txt = m_buttons[i].GetComponentInChildren<TextMeshProUGUI>();
				if(txt != null) {
					txt.text = m_localizedConfig.m_kPopupButtons[i].m_strText;
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Close this popup.
	/// </summary>
	public void ClosePopup() {
		this.GetComponent<PopupController>().Close(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A button has been hit.
	/// </summary>
	/// <param name="_btnIdx">Button index.</param>
	public void OnButton(int _btnIdx) {
		// Check params
		if(m_localizedConfig == null) return;
		if(_btnIdx >= m_localizedConfig.m_kPopupButtons.Count) return;

		// Different stuff based on the action assigned to the pressed button
		CustomizerManager.CustomiserPopupButton button = m_localizedConfig.m_kPopupButtons[_btnIdx] as CustomizerManager.CustomiserPopupButton;
		switch(button.m_eButtonAction) {
			case CustomizerManager.ePopupButtonAction.CLOSE: {
				ClosePopup();
			} break;

			case CustomizerManager.ePopupButtonAction.GAME_LINK: {
				// Since we need extra parameters not available in Calety's customizer popup implementation, we'll concatenate them to the target screen
				string[] tokens = button.m_strParam.Split(':');
				if(tokens.Length == 0) break;

				// Parse known links
				switch(tokens[0]) {
					case "DRAGON_SELECTION": {
						// Navigate to a specific dragon?
						if(tokens.Length > 1) {
							InstanceManager.menuSceneController.SetSelectedDragon(tokens[1]);
						}

						// Go to dragon selection screen and close the popup!
						InstanceManager.menuSceneController.GoToScreen(MenuScreen.DRAGON_SELECTION);
						ClosePopup();
					} break;

					// [AOC] TODO!! More cases, as defined in the US https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+In-Game+News
				}
			} break;

			case CustomizerManager.ePopupButtonAction.OPEN_URL: {
				// [AOC] TODO!!
			} break;

			case CustomizerManager.ePopupButtonAction.BUY_PRODUCT: {
				// [AOC] TODO!!
			} break;

			case CustomizerManager.ePopupButtonAction.REWARD: {
				// [AOC] TODO!!
			} break;

			case CustomizerManager.ePopupButtonAction.SHOP: {
				// [AOC] TODO!!
			} break;

			default:
			case CustomizerManager.ePopupButtonAction.NONE: {
				return;
			} break;
		}
	}
}