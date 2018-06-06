using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentBuildScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SeparatorAttribute("Dragon")]
	[SerializeField] private MenuDragonLoader 	m_dragonLoader;
	[SerializeField] private Localizer 			m_dragonName;
	[SerializeField] private Localizer 			m_dragonSkin;
	[SerializeField] private Image 				m_dragonTierIcon;
	[SerializeField] private PowerIcon 			m_dragonPower;

	[SeparatorAttribute("Pets")]
	[SerializeField] private PetSlot[] 			m_petSlots;

	[SeparatorAttribute("Tournament Info")]
	[SerializeField] private Localizer 			m_titleText;
	[SerializeField] private Localizer			m_goalText;
	[SerializeField] private Image 				m_goalIcon;
	[SerializeField] private TextMeshProUGUI	m_bestScore;
	[SerializeField] private ModifierIcon[] 	m_modifier;

	[SeparatorAttribute("Enter button")]
	[SerializeField] private Localizer 			m_enterBtn;
	[SerializeField] private GameObject			m_nextFreeTimerGroup;
	[SerializeField] private TextMeshProUGUI 	m_nextFreeTimer;


	//------------------------------------------------------------------------//
	private HDTournamentManager 	m_tournament;
	private HDTournamentDefinition 	m_definition;
	private HDTournamentData 		m_data;
	private ResourcesFlow 			m_purchaseFlow;

	private bool m_hasFreeEntrance;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}



	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public void Refresh() {
		m_tournament = HDLiveEventsManager.instance.m_tournament;
		m_data = m_tournament.data as HDTournamentData;
		m_definition = m_data.definition as HDTournamentDefinition;


		//-- Dragon ---------------------------------------------------//
		string sku = m_tournament.GetToUseDragon();
		DragonData dragonData = DragonManager.GetDragonData(sku);

		m_dragonLoader.LoadDragon(sku);
		m_dragonName.Localize(dragonData.def.Get("tidName"));
		m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));

		DefinitionNode disguise = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, dragonData.diguise);
		m_dragonSkin.Localize(disguise.Get("tidName"));

		string powerupSku = disguise.Get("powerup");
		DefinitionNode powerup = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerupSku);

		m_dragonPower.InitFromDefinition(powerup, true);


		//-- Pets -----------------------------------------------------//
		List<string> pets = m_tournament.GetToUsePets();
		for (int i = 0; i < m_petSlots.Length; ++i) {
			if (i < pets.Count) {
				m_petSlots[i].Refresh(pets[i], true);
				m_petSlots[i].gameObject.SetActive(true);
			} else {
				m_petSlots[i].gameObject.SetActive(false);
			}
		}


		//-- Tournament Info ------------------------------------------//
		//TITLE
		m_titleText.Localize(m_definition.m_name);

		//GOALS
		m_goalText.Localize(m_definition.m_goal.m_desc);
		m_goalIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + m_definition.m_goal.m_icon);

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

		m_bestScore.text = StringUtils.FormatNumber(m_data.m_score);



		//-- Entrance Button ------------------------------------------//
		if (m_tournament.CanIUseFree()) {
			m_enterBtn.Localize("Free");
			m_nextFreeTimerGroup.SetActive(false);
		} else {
			m_enterBtn.Localize("Enter<br>" + m_definition.m_entrance.m_amount);
			m_nextFreeTimerGroup.SetActive(true);
		}

		m_hasFreeEntrance = m_tournament.CanIUseFree();

		//TIMER
		UpdatePeriodic();
	}

	// Update timers periodically
	void UpdatePeriodic() {
		if (!m_hasFreeEntrance) {	
			m_nextFreeTimer.text = TimeUtils.FormatTime(m_tournament.TimeToNextFree(), TimeUtils.EFormat.DIGITS, 4, TimeUtils.EPrecision.DAYS, true);	// [AOC] HARDCODED!!
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

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);
	}

	public void OnStartPaying() {
		if (Application.internetReachability == NetworkReachability.NotReachable) {
			SendFeedback("TID_NEED_CONNECTION");
		} else if (!GameServerManager.SharedInstance.IsLoggedIn()) {
			// Check log in!
			SendFeedback("TID_NEED_TO_LOG_IN");
		} else {
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
				m_purchaseFlow.Begin(amount, currency, HDTrackingManager.EEconomyGroup.TOURNAMENT_ENTRANCE, null, false);
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
					InstanceManager.menuSceneController.OnPlayButton();
				}
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.NET_ERROR: {
				SendFeedback("TID_NET_ERROR");
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.RESPONSE_NOT_VALID:
			case HDLiveEventsManager.ComunicationErrorCodes.NO_RESPONSE: {
				SendFeedback("TID_NO_RESPONSE");
			}break;
			case HDLiveEventsManager.ComunicationErrorCodes.OTHER_ERROR: {
				// How to know if free was not valid??
				// SendFeedback("TID_NO_RESPONSE");
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
		// Go to play!
		InstanceManager.menuSceneController.OnPlayButton();
	}


}
