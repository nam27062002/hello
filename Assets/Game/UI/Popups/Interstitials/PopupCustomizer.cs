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

using TMPro;
using SimpleJSON;
using DG.Tweening;

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;


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
	public const string PATH = "UI/Popups/Interstitial/";
	private const string JSON_SAVE_PATH = "Assets/Art/UI/Popups/Interstitial/";

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
	public abstract class Field<T> where T : Component {
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

		public void SaveJSON(SimpleJSON.JSONNode _root) {
			SaveJSON(_root, -1);
		}

		public void SaveJSON(SimpleJSON.JSONNode _root, int _index) {
			if (element != null) {
				SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
				{
					data.Add("key", element.name);
					data.Add("type", m_fieldType);
					data.Add("caletyKey", m_caletyKey);

					SimpleJSON.JSONClass param = new SimpleJSON.JSONClass();
					{
						if (_index >= 0) param.Add("index", _index);
						AddJSONParams(param);
					}
					if (param.Count > 0) {
						data.Add("params", param);
					}
				}
				_root.Add(data);
			}
		}

		protected virtual void AddJSONParams(SimpleJSON.JSONClass _param) {}
	}

	[System.Serializable]
	public class TitleField : Field<Text> {
		public TitleField() : base(FieldType.TEXT, CaletyKey.TITLE) {}
	}

	[System.Serializable]
	public class MessageField : Field<Text> {
		public MessageField() : base(FieldType.TEXTAREA, CaletyKey.TEXT) {}
	}

	[System.Serializable]
	public class TextField : Field<Text> {
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

		protected override void AddJSONParams(SimpleJSON.JSONClass _param) {
			if (optional) _param.Add("optional", true);
		}
	}



	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// [AOC] TODO!! Support as many fields as desired. Not supported by Calety right now :(
	//[SerializeField] private FieldsDictionary m_dynamicFields = new FieldsDictionary();

	// Exposed
	[InfoBox("All fields optional")]
	[SerializeField] private TitleField m_titleField;
	[SeparatorAttribute]
	[SerializeField] private MessageField m_messageField;
	[SerializeField] private List<TextField> m_textFields = new List<TextField>();
	[SeparatorAttribute]
	[SerializeField] private ImageField m_imageField;
	[SeparatorAttribute]
	[SerializeField] private List<ButtonField> m_buttonFields = new List<ButtonField>();
	[SerializeField] private ButtonField m_closeButtonField;


	// Internal
	private Calety.Customiser.CustomiserPopupConfig m_config = null;
	private CaletyConstants.PopupConfig m_localizedConfig = null;

	private CanvasGroup m_menuCanvasGroup = null;

    private bool m_isPopupSurvey = false;
    private HDTrackingManager.EPopupSurveyAction m_popupSurveyAction;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Connect the buttons
		for(int i = 0; i < m_buttonFields.Count; ++i) {
			int btnIdx = i;	// Delta expressions and iterators -_-
			m_buttonFields[i].element.onClick.AddListener(() => { OnButton(btnIdx); });
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
	public void InitFromConfig(Calety.Customiser.CustomiserPopupConfig _config) {
		// Store config
		m_config = _config;

		// Find out localized config based on current selected language
		m_localizedConfig = m_config.m_kPopupConfigByLanguage[m_config.m_kPrepareState.m_strPreparedLanguage];

		// Close button
		if (m_closeButtonField.element != null) {
			m_closeButtonField.element.gameObject.SetActive(m_config.m_bHasCloseButton);
		}

		// Title
		if (m_titleField.element != null) {
			m_titleField.element.text = m_localizedConfig.m_strTitle;
		}

		// Texts
		// Since Calety's customizer doesn't support multiple textfields, use the message as a texts dictionary
		JSONNode textsJson = null;
		try {
			textsJson = JSON.Parse(m_localizedConfig.m_strMessage);
		} catch {}


		if (textsJson == null || textsJson is JSONString) {
			// If json couldn't be parsed from the given string, assume it's a simple message and put it in the main textfield
			if (m_messageField.element != null) {
				m_messageField.element.text = m_localizedConfig.m_strMessage;
			}
		} else {
			// Find the main text
			if (m_messageField.element != null) {
				m_messageField.element.text = textsJson[m_messageField.element.name];
			}

			// Iterate through all textfields in the popup and try to find a text for them in the json
			for (int i = 0; i < m_textFields.Count; ++i) {
				Text textField = m_textFields[i].element;

				if (textField != null) {
					// Set the text from the json, or empty string if the json doesn't have a text for this textfield
					if(textsJson.ContainsKey(textField.name)) {
						textField.text = textsJson[textField.name];
					} else {
						textField.text = string.Empty;
					}
				}
			}
		}

		// Image
		if(m_imageField.element != null) {
			if(m_config.m_kUnityImageTexture != null) {
				m_imageField.element.gameObject.SetActive(true);
				m_imageField.element.texture = m_config.m_kUnityImageTexture;
			} else {
				m_imageField.element.gameObject.SetActive(false);
			}
		}

		// Buttons
		for (int i = 0; i < m_buttonFields.Count; ++i) {
			// Button defined?
			if (i >= m_localizedConfig.m_kPopupButtons.Count) {
				m_buttonFields[i].element.gameObject.SetActive(false);
			} else {
				m_buttonFields[i].element.gameObject.SetActive(true);

				// Set text
				Text txt = m_buttonFields[i].element.GetComponentInChildren<Text>();
				if (txt != null) {
					txt.text = m_localizedConfig.m_kPopupButtons[i].m_strText;
				}
			}
		}

        // Survey popup is implemented with a CRM popup. We need to know if this is a survey popup because some stuff besides just opening the url needs to be done.
        // A CRM popup is considered the survey popup when the url contains a specific domain ("typeform.com")
        m_isPopupSurvey = false;
        m_popupSurveyAction = HDTrackingManager.EPopupSurveyAction.No;
        if (m_localizedConfig != null && m_localizedConfig.m_kPopupButtons != null)
        {
            int count = m_localizedConfig.m_kPopupButtons.Count;
            CaletyConstants.PopupButton button;
            string[] tokens;
            for (int i = 0; i < count && !m_isPopupSurvey; i++)
            {
                button = m_localizedConfig.m_kPopupButtons[i];
                tokens = string.IsNullOrEmpty(button.m_strParam) ? new string[0] : button.m_strParam.Split(';');
                if (tokens.Length > 0 && !string.IsNullOrEmpty(tokens[0]) && tokens[0].Contains("typeform.com"))
                {
                    m_isPopupSurvey = true;
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
        if (m_isPopupSurvey)
        {
            HDTrackingManager.Instance.Notify_PopupSurveyShown(m_popupSurveyAction);
        }

		// Close the popup!
		this.GetComponent<PopupController>().Close(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide menu UI!
		// This popup doesn't have a dark curtain (art request), so we need to hide
		// the rest of the UI to prevent players from thinking it's usable

		// Make sure we're on the menu
		if(InstanceManager.menuSceneController == null) return;

		// Find out root canvas and add a canvas group to set global alpha
		Canvas menuCanvas = InstanceManager.menuSceneController.hud.GetComponentInParent<Canvas>();
		m_menuCanvasGroup = menuCanvas.ForceGetComponent<CanvasGroup>();

		// Fade it out!
		m_menuCanvasGroup.DOKill(true);
		m_menuCanvasGroup.DOFade(0f, 0.25f);
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
        HDCustomizerManager.instance.NotifyPopupViewed(m_config);

		// Fade canvas in!
		if(m_menuCanvasGroup != null) {
			m_menuCanvasGroup.DOKill(true);
			m_menuCanvasGroup.DOFade(1f, 0.25f);
		}
	}    

	/// <summary>
	/// A button has been hit.
	/// </summary>
	/// <param name="_btnIdx">Button index.</param>
	public void OnButton(int _btnIdx) {
		// Check params
		if(m_localizedConfig == null) return;
		if(_btnIdx >= m_localizedConfig.m_kPopupButtons.Count) return;

		// Close popup afterwards unless some action explicitely asks not to
		bool closePopup = true;

		// Different stuff based on the action assigned to the pressed button
		// Since we need extra parameters not available in Calety's customizer popup implementation, we'll concatenate them to the target screen
		Calety.Customiser.CustomiserPopupButton button = m_localizedConfig.m_kPopupButtons[_btnIdx] as Calety.Customiser.CustomiserPopupButton;
		string[] tokens = string.IsNullOrEmpty(button.m_strParam) ? new string[0] : button.m_strParam.Split(';');
		switch(button.m_eButtonAction) {
			case Calety.Customiser.ePopupButtonAction.CLOSE: {
				// Will be done at the end of the method
			} break;

			case Calety.Customiser.ePopupButtonAction.GAME_LINK: {
				// At least one token! (screen id)
				if(tokens.Length == 0) break;

				// Parse known links
				// As defined in the US https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+In-Game+News
				MenuScreen nextScreen = MenuScreen.NONE;
				switch(tokens[0]) {
					case "dragon_selection": {
						// Navigate to a specific dragon?
						if(tokens.Length > 1) {
							InstanceManager.menuSceneController.SetSelectedDragon(tokens[1]);
						}

						// Go to dragon selection screen
						nextScreen = MenuScreen.DRAGON_SELECTION;
					} break;

					case "shop": {
						// Scroll to a specific item / category?
						if(tokens.Length > 1) {
							// Is it a category sku?
							string targetSku = tokens[1];
							if(DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.SHOP_CATEGORIES).Contains(targetSku)) {
								// Yes! Tell the shop to scroll to that category
								InstanceManager.menuSceneController.shopScreenController.shopController.SetInitialCategory(targetSku);
							} else {
								// No! Assume it's a shop item sku and tell the shop to scroll to that item
								InstanceManager.menuSceneController.shopScreenController.shopController.SetInitialItem(targetSku);
							}
						}

						// Go to the shop screen
						InstanceManager.menuSceneController.shopScreenController.mode = ShopController.Mode.DEFAULT;
						nextScreen = MenuScreen.SHOP;
					} break;

					case "pets": {
						// Make sure selected dragon is owned (requirement for opening the pets screen)
						InstanceManager.menuSceneController.SetSelectedDragon(DragonManager.CurrentDragon.def.sku);	// Current dragon is the last owned selected dragon

						// Go to the pets screen
						nextScreen = MenuScreen.PETS;

						// Navigate to a specific pet?
						if(tokens.Length > 1) {
							PetsScreenController petScreen = InstanceManager.menuSceneController.transitionManager.GetScreenData(nextScreen).ui.GetComponent<PetsScreenController>();
							petScreen.Initialize(tokens[1]);
						}
					} break;

					case "global_event": {
						// Just do it!
						nextScreen = MenuScreen.GLOBAL_EVENTS;
					} break;

					case "tournament": {
						// Do we have a valid tournament?
						HDTournamentManager tournamentManager = HDLiveDataManager.tournament;

						// Do we actually have a tournament?
						if(!tournamentManager.EventExists()) break;

						// If the tournament requires a game update, show popup and break
						if(tournamentManager.RequiresUpdate()) {
							// Show update popup!
							PopupManager.OpenPopupInstant(PopupUpdateEvents.PATH);
							break;
						}

						// Is the tournament running?
						if(!tournamentManager.IsRunning()) break;

						// Is OTA ready for this tournament?
						// If we dont have downloaded the content yet, dont go to the tournament screen
						Downloadables.Handle tournamentHandle = HDAddressablesManager.Instance.GetHandleForTournament(tournamentManager);
						if(tournamentHandle.IsAvailable()) {
							// All good! go to the tournament screen
							// Change game mode first
    						SceneController.SetMode(SceneController.Mode.TOURNAMENT);
        					HDLiveDataManager.instance.SwitchToTournament();

							// Now we're good to go!
							nextScreen = MenuScreen.TOURNAMENT_INFO;
						} else {
							// Show popup with download progress
							PopupAssetsDownloadFlow downloadPopup = PopupAssetsDownloadFlow.OpenPopupByState(
								tournamentHandle,
								PopupAssetsDownloadFlow.PopupType.ANY,
								AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_TOURNAMENT
							);

							// Allow the player to hit the button again once the OTA popup is closed
							// [AOC] TODO!! This flow should be reviewed
							closePopup = false;
						}
					} break;

					case "skins": {
						// Is a specific skin sku defined?
						DefinitionNode targetSkinDef = null;
						if(tokens.Length > 1) {
							targetSkinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, tokens[1]);
						}

						// Find out target dragon
						string targetDragonSku = DragonManager.CurrentDragon.def.sku;  // Current dragon by default
						if(targetSkinDef != null) {
							targetDragonSku = targetSkinDef.GetAsString("dragonSku", targetDragonSku);
						}

						// Scroll to target dragon
						InstanceManager.menuSceneController.SetSelectedDragon(targetDragonSku);

						// Is target dragon owned?
						bool dragonOwned = DragonManager.IsDragonOwned(targetDragonSku);

						// Are all assets downloaded for this dragon?
						Downloadables.Handle dragonHandle = HDAddressablesManager.Instance.GetHandleForClassicDragon(targetDragonSku);
						bool assetsDownloaded = dragonHandle.IsAvailable();

						// Is dragon owned and assets downloaded?
						if(dragonOwned && assetsDownloaded) {
							// Yes! Go to the skins screen
							nextScreen = MenuScreen.SKINS;

							// If defined, select a specific skin
							if(targetSkinDef != null) {
								DisguisesScreenController skinsScreen = InstanceManager.menuSceneController.GetScreenData(MenuScreen.SKINS).ui.GetComponent<DisguisesScreenController>();
								skinsScreen.initialSkin = targetSkinDef.sku;
							}
						} else {
							// No! Go to the dragon selection screen instead
							nextScreen = MenuScreen.DRAGON_SELECTION;
						}
					} break;

					case "offer_popup": {
						// Is a specific offer sku defined?
						OfferPack targetOffer = null;
						if(tokens.Length > 1) {
							targetOffer = OffersManager.GetOfferPack(tokens[1]);
						}

						// If not defined, the target offer will be the featured one (if any)
						else {
							targetOffer = OffersManager.featuredOffer;
						}

						// Valid and active offer?
						if(targetOffer != null && targetOffer.isActive) {
							// Yes! Open offer popup
							PopupController popup = PopupShopOfferPack.LoadPopupForOfferPack(targetOffer);
							popup.Open();
						}
					} break;
				}

				// Trigger screen transition
				if(nextScreen != MenuScreen.NONE) {
					// Go to target screens
					InstanceManager.menuSceneController.GoToScreen(nextScreen);

					// Clear any queued popups
					PopupManager.ClearQueue();
				}
			} break;

			case Calety.Customiser.ePopupButtonAction.OPEN_URL: {
				// Mandatory parameter, just close popup if not defined
				if(tokens.Length > 0) {

                    // This stuff has be done here instead of inside the callback called by DelayedCall because the popup is closed immediately and
                    // m_popupSurveyAction is used by ClosePopup() function.
                    string url = tokens[0];
                    if (!string.IsNullOrEmpty(url))
                    {
                        // If the link leads to a survey then we need to add the user's dna profile id to the url so BI can cross survey results with information such as age or country retrieved from dna
                        // This stuff is hardcoded because it was the fastest way to carry it out, otherwise it would've involved server and Calety
                        if (m_isPopupSurvey)
                        {
                            string dnaProfileId = HDTrackingManager.Instance.GetDNAProfileID();
                            if (string.IsNullOrEmpty(dnaProfileId))
                            {
                                dnaProfileId = "Not_Available";
                            }
                            url += "?UID=" + dnaProfileId;

                            // Tracking event will be sent when the popup is closed. We need to store that the user has clicked on the button that leads her to the survey
                            m_popupSurveyAction = HDTrackingManager.EPopupSurveyAction.Yes;
                        }
                    }

                    // Add some delay to give enough time for SFX to be played and popup to be closed before losing focus
                    UbiBCN.CoroutineManager.DelayedCall(
				    () => {                           
                        if (!string.IsNullOrEmpty(url))
                        {                           
                            Application.OpenURL(url);
                        }
					}, 0.25f
					);
				}
			} break;

			case Calety.Customiser.ePopupButtonAction.BUY_PRODUCT: {
				// [AOC] TODO!!
				LogError("Customizer Popup: Action not yet implemented! (" + button.m_eButtonAction.ToString() + " | " + button.m_strParam + ")", true);
			} break;

			case Calety.Customiser.ePopupButtonAction.REWARD: {
				// [AOC] TODO!!
				LogError("Customizer Popup: Action not yet implemented! (" + button.m_eButtonAction.ToString() + " | " + button.m_strParam + ")", true);
			} break;
			
			case Calety.Customiser.ePopupButtonAction.NONE: {
				// [AOC] Custom actions to be implemented if needed
				LogError("Customizer Popup: Action not yet implemented! (" + button.m_eButtonAction.ToString() + " | " + button.m_strParam + ")", true);
				return;
			} break;

			default: {
				// Unrecognized action
				LogError("Customizer Popup: Unknown action! (" + button.m_eButtonAction.ToString() + ")", true);
				return;
			}
		}

		// Close popup unless some action explicitely asks not to
		if(closePopup) ClosePopup();
	}
		
	//------------------------------------------------------------------------//
	// UTILS																  //
	//------------------------------------------------------------------------//
	public void SaveJSON() {
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
		{
			SimpleJSON.JSONArray components = new SimpleJSON.JSONArray();
			{
				m_titleField.SaveJSON(components);
				m_messageField.SaveJSON(components);
				for (int i = 0; i < m_textFields.Count; ++i) {
					m_textFields[i].SaveJSON(components);
				}
				m_imageField.SaveJSON(components);
				for (int i = 0; i < m_buttonFields.Count; ++i) {
					m_buttonFields[i].SaveJSON(components, i);
				}
			}
			data.Add("components", components);

			SimpleJSON.JSONClass settings = new SimpleJSON.JSONClass();
			{
				int layoutIndex = 0;
				string layoutName = name;

				int i = layoutName.LastIndexOf("_");
				layoutName = layoutName.Substring(i + 1);
				layoutIndex = int.Parse(layoutName, NumberStyles.Any, CultureInfo.InvariantCulture);

				settings.Add("layoutIndex", layoutIndex);
				settings.Add("hasCloseButton", m_closeButtonField.element != null);
				if (m_closeButtonField.element != null) {
					settings.Add("closeButtonIsOptional", m_closeButtonField.optional);
				}
			}
			data.Add("settings", settings);
		}

		string filePath = JSON_SAVE_PATH + name + ".json";
		using (StreamWriter sw = new StreamWriter(filePath, false)) {
			sw.WriteLine(data.ToString());
			sw.Close();
		}
	}

	/// <summary>
	/// Internal error logger.
	/// </summary>
	/// <param name="_message">Message to be displayed.</param>
	/// <param name="_onScreen">Show on-screen message?</param>
	private void LogError(string _message, bool _onScreen) {
		// Console message
		ControlPanel.LogError(Colors.red.Tag(_message), ControlPanel.ELogChannel.Customizer);

		// On-screen message
		if(_onScreen) {
			UIFeedbackText feedback = UIFeedbackText.CreateAndLaunch(
				_message,
				GameConstants.Vector2.one * 0.5f,
				InstanceManager.menuSceneController.hud.GetComponentInParent<Canvas>().transform as RectTransform
			);
			feedback.text.color = Color.red;
		}
	}
}