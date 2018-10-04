using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentBuildScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	private enum Mode {
		Build = 0,
		EditDragon,
		EditPets
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SeparatorAttribute("Dragon")]
	[SerializeField] private MenuDragonLoader 	m_dragonLoader;
	[SerializeField] private Localizer 			m_dragonName;
	[SerializeField] private Localizer 			m_dragonSkin;
	[SerializeField] private PowerIcon 			m_dragonPower;

	[SeparatorAttribute("Pets")]
	[SerializeField] private PetSlot[] 			m_petSlots;
	[SerializeField] private GameObject			m_petEditRoot;
	[SerializeField] private Transform[]		m_petEditSlots;

	[SeparatorAttribute("Tournament Info")]
	[SerializeField] private TextMeshProUGUI	m_goalText;
	[SerializeField] private ModifierIcon[] 	m_modifier;

	[SeparatorAttribute("Enter button")]
	[SerializeField] private Button 			m_enterCurrencyBtn;
	[SerializeField] private Button 			m_enterFreeBtn;
	[SerializeField] private Button 			m_enterAdBtn;
	[SerializeField] private GameObject			m_nextFreeTimerGroup;
	[SerializeField] private TextMeshProUGUI 	m_nextFreeTimer;
	[SerializeField] private Slider				m_nextFreeSlider;


	//------------------------------------------------------------------------//
	private HDTournamentManager 	m_tournament;
	private HDTournamentDefinition 	m_definition;
	private HDTournamentData 		m_data;
	private ResourcesFlow 			m_purchaseFlow;

	private Transform[] 			m_petEquipSlots;

	private bool m_waitingRewardsData = false;
	private bool m_hasFreeEntrance;

	private Mode m_mode;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	private void OnEnable() {
		Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes> (MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewDefinition);
	}

	private void OnDisable() {
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes> (MessengerEvents.LIVE_EVENT_NEW_DEFINITION, OnNewDefinition);
	}
		

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public void Refresh() {
		m_mode = Mode.Build;

		m_tournament = HDLiveEventsManager.instance.m_tournament;
		m_data = m_tournament.data as HDTournamentData;
		m_definition = m_data.definition as HDTournamentDefinition;


		//-- Dragon ---------------------------------------------------//
		string sku = m_tournament.GetToUseDragon();
		IDragonData dragonData = DragonManager.GetDragonData(sku);
		m_dragonName.Localize(dragonData.def.Get("tidName"));

		string disguiseSku = m_tournament.GetToUseSkin();
		m_dragonLoader.LoadDragon(sku, disguiseSku);
		DefinitionNode disguise = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguiseSku);
		if (disguise.GetAsInt("shopOrder") > 0) { // skins
			m_dragonSkin.Localize(disguise.Get("tidName"));
		} else { // default skin
			m_dragonSkin.gameObject.SetActive(false);
		}

		string powerupSku = disguise.Get("powerup");
		DefinitionNode powerup = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerupSku);

		m_dragonPower.InitFromDefinition(powerup, false);

		//-- Pets -----------------------------------------------------//
		DragonEquip dragonEquip = m_dragonLoader.FindComponentRecursive<DragonEquip>();
		m_petEquipSlots = new Transform[m_petSlots.Length];

		List<string> pets = m_tournament.GetToUsePets();
		for (int i = 0; i < m_petSlots.Length; ++i) {
			if (i < pets.Count) {				
				AttachPoint ap = dragonEquip.GetAttachPoint(Equipable.AttachPoint.Pet_1 + i);
				m_petEquipSlots[i] = ap.transform;

				m_petSlots[i].Refresh(pets[i], true);
				m_petSlots[i].gameObject.SetActive(true);

				m_petSlots[i].petLoader.transform.position = m_petEquipSlots[i].position;				
			} else {
				m_petEquipSlots[i] = null;
				m_petSlots[i].gameObject.SetActive(false);
				m_petSlots[i].powerIcon.gameObject.SetActive(false);
			}
		}


		//-- Tournament Info ------------------------------------------//
		//GOALS
		m_goalText.text = m_tournament.GetDescription();

		//MODIFIERS
		List<IModifierDefinition> mods = new List<IModifierDefinition>();
		for (int i = 0; i < m_definition.m_dragonMods.Count; ++i) {
			mods.Add(m_definition.m_dragonMods[i]);
		}

		for (int i = 0; i < m_definition.m_otherMods.Count; ++i) {
			mods.Add(m_definition.m_otherMods[i]);
		}

		for (int i = 0; i < m_modifier.Length; ++i) {
			if (i < mods.Count) {
				m_modifier[i].InitFromDefinition(mods[i]);
			} else {
				m_modifier[i].gameObject.SetActive(false);
			}
		}


		//-- Entrance Button ------------------------------------------//
		if (m_tournament.CanIUseFree()) {
			ShowEntranceButton(m_enterFreeBtn);
			m_nextFreeTimerGroup.SetActive(false);
		} else {
			ShowEntranceButton(m_enterCurrencyBtn);
			m_nextFreeTimerGroup.SetActive(true);
		}

		m_hasFreeEntrance = m_tournament.CanIUseFree();

		//TIMER
		m_nextFreeSlider.minValue = 0f;
		m_nextFreeSlider.maxValue = m_definition.m_entrance.m_dailyFree;
		UpdatePeriodic();
	}

	void Update() {
		if (!m_hasFreeEntrance) {
			//smooth slider animation
			m_nextFreeSlider.value += Time.deltaTime; 
		}
	}

	// Update timers periodically
	void UpdatePeriodic() {
		if (!m_hasFreeEntrance) {	
			double seconds = m_tournament.TimeToNextFree();
			m_nextFreeSlider.value = m_definition.m_entrance.m_dailyFree - (float)seconds;

			m_nextFreeTimer.text = TimeUtils.FormatTime(seconds, TimeUtils.EFormat.DIGITS, 3, TimeUtils.EPrecision.HOURS, true);	// [AOC] HARDCODED!!
			if (seconds <= 0) {
				m_hasFreeEntrance = true;
				ShowEntranceButton(m_enterFreeBtn);
				m_nextFreeTimerGroup.SetActive(false);
			}
		}

		if (!m_definition.m_refund) {
			if (m_definition.timeToEnd.TotalSeconds <= 0f) {
				if (!m_waitingRewardsData) {
					Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);

					// Request rewards data and wait for it to be loaded
					m_tournament.RequestRewards();

					// Show busy screen
					BusyScreen.Setup(true, LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_LOADING"));
					BusyScreen.Show(this);

					m_waitingRewardsData = true;
					CancelInvoke();
				}
			}
		}
	}

	private void ShowEntranceButton(Button _activeButton) {
		m_enterAdBtn.gameObject.SetActive(m_enterAdBtn == _activeButton);
		m_enterFreeBtn.gameObject.SetActive(m_enterFreeBtn == _activeButton);
		m_enterCurrencyBtn.gameObject.SetActive(m_enterCurrencyBtn == _activeButton);

		Localizer loc = _activeButton.FindComponentRecursive<Localizer>();
		if (m_enterAdBtn == _activeButton) {
			loc.Localize("TID_TOURNAMENT_PLAY_AD");
		} else if (m_enterFreeBtn == _activeButton) {
			loc.Localize("TID_GEN_FREE");
		} else if (m_enterCurrencyBtn == _activeButton) {
			if (m_definition.m_entrance.m_type == "hc" || m_definition.m_entrance.m_type == "pc") {
				loc.Localize("TID_TOURNAMENT_PLAY_PC", StringUtils.FormatNumber(m_definition.m_entrance.m_amount));
			} else{ 
				loc.Localize("TID_TOURNAMENT_PLAY_SC", StringUtils.FormatNumber(m_definition.m_entrance.m_amount));
			}
		}
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		Refresh();

		m_waitingRewardsData = false;

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);
	}

	public void OnHidePreAnimation() {
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	public void OnEditPetsToogle() {
		if (m_mode != Mode.EditPets) {
			m_petEditRoot.SetActive(true);

			for (int i = 0; i < m_petEquipSlots.Length; ++i) {
				if (m_petEquipSlots[i] != null) {
					m_petSlots[i].petLoader.transform.position = m_petEditSlots[i].position;
				}
			}

			m_mode = Mode.EditPets;
		} else {
			m_petEditRoot.SetActive(false);

			for (int i = 0; i < m_petEquipSlots.Length; ++i) {
				if (m_petEquipSlots[i] != null) {
					m_petSlots[i].petLoader.transform.position = m_petEquipSlots[i].position;
				}
			}

			m_mode = Mode.Build;
		}
	}

	public void OnStartPaying() {
		if (Application.internetReachability == NetworkReachability.NotReachable || !GameServerManager.SharedInstance.IsLoggedIn()) {
			SendFeedback("TID_GEN_NO_CONNECTION");
		} 
		/*
		else if (!GameServerManager.SharedInstance.IsLoggedIn()) 
		{
			// Check log in!
			SendFeedback("TID_NEED_TO_LOG_IN");
		} 
		*/
		else 
		{
			// Check paying
			if (m_hasFreeEntrance) {
				// Move to Loading Screen
				BusyScreen.Show(this);

				// Prepare to wait for the callback
				Messenger.AddListener<HDLiveEventsManager.ComunicationErrorCodes, string, long>(MessengerEvents.TOURNAMENT_ENTRANCE, OnTournamentEntrance);

				// Send Entrance
				m_tournament.SendEntrance("free", 0);
			} else {
				// Check if I have enough currency
				m_purchaseFlow = new ResourcesFlow("TOURNAMENT_ENTRANCE");
				m_purchaseFlow.OnSuccess.AddListener( OnEntrancePayAccepted );
				long amount = m_definition.m_entrance.m_amount;
				UserProfile.Currency currency = UserProfile.SkuToCurrency(m_definition.m_entrance.m_type);
				m_purchaseFlow.Begin(amount, currency, HDTrackingManager.EEconomyGroup.TOURNAMENT_ENTRY, null, false);
			}
		}
	}

	void OnEntrancePayAccepted(ResourcesFlow _flow) {
		// Move to Loading Screen
		BusyScreen.Show(this);

		// Prepare to wait for the callback
		Messenger.AddListener<HDLiveEventsManager.ComunicationErrorCodes, string, long>(MessengerEvents.TOURNAMENT_ENTRANCE, OnTournamentEntrance);

		// Send Entrance
		m_tournament.SendEntrance( m_definition.m_entrance.m_type, m_definition.m_entrance.m_amount );
	}

	void OnTournamentEntrance(HDLiveEventsManager.ComunicationErrorCodes err, string type, long amount) {
		BusyScreen.Hide(this);

		switch (err)  {
			case HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR: {
				// Pay and go to play
				if ( type != "free" )
				{
					m_purchaseFlow.OnSuccess.RemoveListener( OnEntrancePayAccepted );
					m_purchaseFlow.OnSuccess.AddListener( OnPayAndPlay );
					m_purchaseFlow.DoTransaction(false);
				}
				else
				{
					// Tracking
					HDTrackingManager.Instance.Notify_TournamentClickOnEnter(m_definition.m_name, UserProfile.Currency.NONE);

					// Go to play!
					InstanceManager.menuSceneController.OnPlayButton();
				}
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.NET_ERROR: {
				SendFeedback("TID_NET_ERROR");
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.RESPONSE_NOT_VALID:
			case HDLiveEventsManager.ComunicationErrorCodes.NO_RESPONSE: 
			{
				SendFeedback("TID_NO_RESPONSE");
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_AMOUNT_NOT_VALID: 
			case HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_TYPE_NOT_VALID:
			{
				SendFeedback("TID_FAIL_TO_PAY_ENTRANCE");
				// Ask for the definition?
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.ENTRANCE_FREE_INVALID:
			{
				SendFeedback("TID_TOURNAMENT_FAIL_TO_PAY_ENTRANCE");
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.TOURNAMENT_IS_OVER:
			{
				SendFeedback("TID_TOURNAMENT_OVER");
				m_tournament.RequestDefinition(true);

			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.OTHER_ERROR: 
			default:
			{
				// How to know if free was not valid??
				SendFeedback("TID_EVENT_RESULTS_UNKNOWN_ERROR");
			}break;
		}

		Messenger.RemoveListener<HDLiveEventsManager.ComunicationErrorCodes, string, long>(MessengerEvents.TOURNAMENT_ENTRANCE, OnTournamentEntrance);
	}

	private void SendFeedback(string tid) {
		UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize(tid),
			new Vector2(0.5f, 0.25f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
		text.text.color = Color.red;
	}

	void OnPayAndPlay(ResourcesFlow _flow) {
		// Tracking
		HDTrackingManager.Instance.Notify_TournamentClickOnEnter(m_definition.m_name, _flow.currency);

		// Go to play!
		InstanceManager.menuSceneController.OnPlayButton();
	}

	private void OnNewDefinition(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _err) {
		if (m_definition.m_refund) { // maybe we'll need some feedback
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.PLAY, true);
		}
	}


	/// <summary>
	/// We got a response on the rewards request.
	/// </summary>
	private void OnRewardsResponse(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Ignore if we weren't waiting for rewards!
		if(!m_waitingRewardsData) return;
		m_waitingRewardsData = false;

		// Hide busy screen
		BusyScreen.Hide(this);

		// Success?
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// Go to tournament rewards screen!
			TournamentRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).ui.GetComponent<TournamentRewardScreen>();
			scr.StartFlow();
            InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_REWARD, true);
		} else {
			// Show error message
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_ERROR"),
				new Vector2(0.5f, 0.33f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			text.text.color = UIConstants.ERROR_MESSAGE_COLOR;
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.PLAY, true);

             // Finish tournament if 607 / 608 / 622
            if ( (_errorCode == HDLiveEventsManager.ComunicationErrorCodes.EVENT_NOT_FOUND ||
                _errorCode == HDLiveEventsManager.ComunicationErrorCodes.EVENT_IS_NOT_VALID ||
                _errorCode == HDLiveEventsManager.ComunicationErrorCodes.EVENT_TTL_EXPIRED ) &&
                m_tournament.data.m_eventId == _eventId
                )
                {
                    m_tournament.ForceFinishByError();
                }
		}
	}
}
