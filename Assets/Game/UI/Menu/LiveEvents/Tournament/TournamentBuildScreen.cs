using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentBuildScreen : MonoBehaviour {
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


	//----------------------------------------------------------------//
	private HDTournamentManager 	m_tournament;
	private HDTournamentDefinition 	m_definition;
	private HDTournamentData 		m_data;



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
	}



	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {

		Refresh();

	}
}
