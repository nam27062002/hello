using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisguisesScreenController : MonoBehaviour {

	[SerializeField] private GameObject m_disguiseTitle;
	[SerializeField] private Text m_name;
	[SerializeField] private GameObject[] m_upgrades;
	[SerializeField] private GameObject m_powerList;
	[SerializeField] private GameObject[] m_powers;
	[SerializeField] private RectTransform m_layout;

	[SeparatorAttribute]
	[SerializeField] private GameObject m_buyButton;
	[SerializeField] private GameObject m_useButton;
	[SerializeField] private GameObject m_unuseButton;

	private DisguisePill[] m_disguises;

	private DisguisePill m_using; 
	private DisguisePill m_preview;


	// Use this for initialization
	void Awake() {
		m_disguises = new DisguisePill[9];
		GameObject prefab = Resources.Load<GameObject>("UI/Popups/Disguises/PF_DisguisesPill");

		// Test
		for (int i = 0; i < 9; i++) {
			GameObject pill = GameObject.Instantiate<GameObject>(prefab);
			pill.transform.parent = m_layout;
			pill.transform.localScale = Vector3.one;

			m_disguises[i] = pill.GetComponent<DisguisePill>();

			m_disguises[i].OnPillClicked.AddListener(OnPillClicked);
		}
	}

	void OnEnable() {
		// get disguises levels of the current dragon
		string dragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		List<DefinitionNode> defList = Definitions.GetDefinitionsByVariable(Definitions.Category.DISGUISES, "dragonSku", dragonSku);

		Definitions.SortByProperty(ref defList, "sku", Definitions.SortType.ALPHANUMERIC);

		for (int i = 0; i < defList.Count; i++) {
			m_disguises[i].Load(defList[i], Wardrobe.GetDisguiseLevel(defList[i].sku));
			m_disguises[i].Use(false);
		}

		// we don't have any disguise equiped
		m_using = null;
		m_preview = null;

		// hide all the info
		m_powerList.SetActive(false);
		m_disguiseTitle.SetActive(false);
		for (int i = 0; i < m_powers.Length; i++) {
			m_powers[i].SetActive(false);
		}

		// hide the buttons
		ShowButtons(false, false, false);
	}

	void OnPillClicked(DisguisePill _pill) {
		if (m_preview != _pill) {
			if (m_preview == null) {
				m_powerList.SetActive(true);
				m_disguiseTitle.SetActive(true);
				for (int i = 0; i < m_powers.Length; i++) {
					m_powers[i].SetActive(true);
				}
			}

			// update name
			m_name.text = _pill.def.sku; // we have to change this

			// update level
			for (int i = 0; i < m_upgrades.Length; i++) {
				m_upgrades[i].SetActive(i < _pill.level);
			}

			m_preview = _pill;

			if (m_preview.level == 0) {
				ShowButtons(true, false, false);
			} else {
				if (m_preview == m_using) {
					ShowButtons(false, false, true);
				} else {
					ShowButtons(false, true, false);
				}				
			}
		}
	}

	public void OnUse() {
		if (m_using != null) {
			m_using.Use(false);
		} 
		m_preview.Use(true);
		m_using = m_preview;
		ShowButtons(false, false, true);
	}

	public void OnUnuse() {
		m_using.Use(false);
		m_using = null;
		ShowButtons(false, true, false);
	}

	private void ShowButtons(bool _buy, bool _use, bool _unuse) {
		m_buyButton.SetActive(_buy);
		m_useButton.SetActive(_use);
		m_unuseButton.SetActive(_unuse);
	}
}
