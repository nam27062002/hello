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

	[SeparatorAttribute]
	private Transform m_dragonWorldPos;
	public RectTransform m_dragonUIPos;
	public float m_depth = 25f;

	[HideInInspector] public string m_previewDisguise;

	private DisguisePill[] m_disguises;

	private DisguisePill m_using; 
	private DisguisePill m_preview;

	private string m_dragonSku;


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

		m_dragonSku = "";
		m_previewDisguise = "";
	}

	void OnEnable() {
		// find the 3D dragon position
		GameObject disguiseScene = GameObject.Find("PF_MenuDisguisesScene");
		if (disguiseScene != null) {
			m_dragonWorldPos = disguiseScene.transform.FindChild("CurrentDragon");
		}

		// get disguises levels of the current dragon
		m_dragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		List<DefinitionNode> defList = DefinitionsManager.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonSku);

		DefinitionsManager.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		string currentDisguise = Wardrobe.GetEquipedDisguise(m_dragonSku);

		if (m_previewDisguise != "") {
			currentDisguise = m_previewDisguise;
			m_previewDisguise = "";
		}

		Sprite[] icons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/" + m_dragonSku);

		// we don't have any disguise equiped
		m_using = null;
		m_preview = null;

		// hide all the info
		m_powerList.SetActive(false);
		m_disguiseTitle.SetActive(false);
		for (int i = 0; i < m_powers.Length; i++) {
			m_powers[i].SetActive(false);
		}


		int defaultSelection = -1;
		bool usingIt = false;

		// load data and persistence
		for (int i = 0; i < m_disguises.Length; i++) {
			if (i < defList.Count) {
				DefinitionNode def = defList[i];

				Sprite spr = null;
				for (int s = 0; s < icons.Length; s++) {
					if (icons[i].name == def.GetAsString("icon")) {
						spr = icons[i];
					}
				}

				int level = Wardrobe.GetDisguiseLevel(def.sku);
				m_disguises[i].Load(def, level, spr);

				if (def.sku == currentDisguise) {
					defaultSelection = i;
					usingIt = true;
				} else if (defaultSelection < 0 && level > 0) {
					defaultSelection = i;
				}

				m_disguises[i].Use(false);
				m_disguises[i].Select(false);
				m_disguises[i].gameObject.SetActive(true);
			} else {
				m_disguises[i].gameObject.SetActive(false);
			}
		}

		if (defaultSelection >= 0) {
			OnPillClicked(m_disguises[defaultSelection]);
			if (usingIt) OnUse();
		} else {
			// hide the buttons
			ShowButtons(true, false);
		}
	}

	void OnDisable() {
		if (m_using != null) {
			Wardrobe.Equip(m_dragonSku, m_using.def.sku);
		} else {
			Wardrobe.Equip(m_dragonSku, "");
		}

		PersistenceManager.Save();
	}

	void Update() {

		Canvas canvas = GetComponentInParent<Canvas>();
		Vector3 viewportPos = canvas.worldCamera.WorldToViewportPoint(m_dragonUIPos.position);

		Camera camera = InstanceManager.GetSceneController<MenuSceneController>().screensController.camera;
		viewportPos.z = m_depth;
		m_dragonWorldPos.position = camera.ViewportToWorldPoint(viewportPos);

	}

	void OnPillClicked(DisguisePill _pill) {
		if (m_preview != _pill) {
			if (m_preview == null) {
				m_powerList.SetActive(true);
				m_disguiseTitle.SetActive(true);
			} else {
				m_preview.Select(false);
			}

			for (int i = 0; i < m_powers.Length; i++) {
				m_powers[i].SetActive(true);
				m_powers[i].transform.FindChild("IconLock").gameObject.SetActive(i >= _pill.level);
			}

			// update name
			m_name.text = _pill.def.GetAsString("tidName"); // we have to change this

			// update level
			for (int i = 0; i < m_upgrades.Length; i++) {
				m_upgrades[i].SetActive(i < _pill.level);
			}

			m_preview = _pill;
			m_preview.Select(true);

			Wardrobe.Equip(m_dragonSku, m_preview.def.sku);

			if (m_preview.level == 0) {
				ShowButtons(true, false);
			} else {
				ShowButtons(false, true);
			}
		}
	}

	public void OnUse() {
		if (m_using != m_preview) {
			if (m_using != null) {
				m_using.Use(false);
			} 
			m_preview.Use(true);
			m_using = m_preview;

			Wardrobe.Equip(m_dragonSku, m_using.def.sku);
		} else {
			m_using.Use(false);
			m_using = null;
		}

		ShowButtons(false, true);
	}

	public void OnBuy() {
		PopupController popup = PopupManager.OpenPopupInstant(PopupEggShop.PATH);
		popup.GetComponent<PopupEggShop>().initialEgg = m_dragonSku;
	}

	private void ShowButtons(bool _buy, bool _use) {
		m_buyButton.SetActive(_buy);

		m_useButton.SetActive(_use);

		if (_use) {
			Text text = m_useButton.GetComponentInChildren<Text>();
			if (m_preview == m_using) {
				// unuse
				text.text = "UNEQUIP";
			} else {
				text.text = "USE";
			}
		}
	}
}
