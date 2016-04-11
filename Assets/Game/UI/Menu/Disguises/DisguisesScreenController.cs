using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisguisesScreenController : MonoBehaviour {

	[SerializeField] private GameObject m_disguiseTitle;
	[SerializeField] private Text m_name;
	[SerializeField] private GameObject m_upgradeList;
	[SerializeField] private GameObject[] m_upgrades;
	[SerializeField] private GameObject m_powerList;
	[SerializeField] private GameObject[] m_powers;
	[SerializeField] private RectTransform m_layout;

	[SeparatorAttribute]
	[SerializeField] private GameObject m_buyButton;
	[SerializeField] private GameObject m_useButton;

	[SeparatorAttribute]
	private Transform m_dragonWorldPos;
	private Transform m_dragonRotationArrowsPos;
	public RectTransform m_dragonUIPos;
	public float m_depth = 25f;

	[HideInInspector] public string m_previewDisguise;

	private DisguisePill[] m_disguises;

	private DisguisePill m_using; 
	private DisguisePill m_preview;

	private DefinitionNode[] m_powerDefs = new DefinitionNode[3];
	private Sprite[] m_powerIcons = new Sprite[3];
	private Sprite[] m_allPowerIcons = null;

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

		// Preload the powerup icon spritesheet
		m_allPowerIcons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/powers/icons_powers"); 

		m_dragonSku = "";
		m_previewDisguise = "";
	}

	void OnEnable() {
		// find the 3D dragon position
		GameObject disguiseScene = GameObject.Find("PF_MenuDisguisesScene");
		if (disguiseScene != null) {
			m_dragonWorldPos = disguiseScene.transform.FindChild("CurrentDragon");
			m_dragonRotationArrowsPos = disguiseScene.transform.FindChild("Arrows");
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

		int defaultSelection = 0;

		// load data and persistence
		for (int i = 0; i < m_disguises.Length; i++) {
			if (i <= defList.Count) {
				if (i == 0) {
					m_disguises[i].LoadAsDefault(GetFromCollection(ref icons, "default"));
				} else {
					DefinitionNode def = defList[i - 1];

					Sprite spr = GetFromCollection(ref icons, def.GetAsString("icon"));

					int level = Wardrobe.GetDisguiseLevel(def.sku);
					m_disguises[i].Load(def, level, spr);

					if (def.sku == currentDisguise) {
						defaultSelection = i;
					}
				}
				m_disguises[i].Use(false);
				m_disguises[i].Select(false);
				m_disguises[i].gameObject.SetActive(true);
			} else {
				m_disguises[i].gameObject.SetActive(false);
			}
		}

		OnPillClicked(m_disguises[defaultSelection]);
		OnUse();
	}

	private Sprite GetFromCollection(ref Sprite[] _array, string _name) {
		for (int i = 0; i < _array.Length; i++) {
			if (_array[i].name == _name) {
				return _array[i];
			}
		}

		return null;
	}

	void OnDisable() {
		if (m_using != null) {
			Wardrobe.Equip(m_dragonSku, m_using.sku);
		} else {
			Wardrobe.Equip(m_dragonSku, "default");
		}

		PersistenceManager.Save();
	}

	void Update() {
		Canvas canvas = GetComponentInParent<Canvas>();
		Vector3 viewportPos = canvas.worldCamera.WorldToViewportPoint(m_dragonUIPos.position);

		Camera camera = InstanceManager.GetSceneController<MenuSceneController>().screensController.camera;
		viewportPos.z = m_depth;
		m_dragonWorldPos.position = camera.ViewportToWorldPoint(viewportPos);
		m_dragonRotationArrowsPos.position = camera.ViewportToWorldPoint(viewportPos) + Vector3.down;
	}

	void OnPillClicked(DisguisePill _pill) {
		if (m_preview != _pill) {
			if (m_preview == null) {				
				m_disguiseTitle.SetActive(true);
			} else {
				m_preview.Select(false);
			}

			if (_pill.isDefault) {
				m_powerList.SetActive(false);
				m_upgradeList.SetActive(false);
			} else {
				m_powerList.SetActive(true);
				m_upgradeList.SetActive(true);

				// Update powers
				// Get defs
				DefinitionNode powerSetDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, _pill.powerUpSet);
				for (int i = 0; i < 3; i++) {
					string powerUpSku = powerSetDef.GetAsString("powerup"+(i+1).ToString());
					m_powerDefs[i] = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);
				}

				// Update icons
				for (int i = 0; i < m_powers.Length; i++) {
					// Show/hide
					m_powers[i].SetActive(true);

					// Lock
					m_powers[i].transform.FindChild("IconLock").gameObject.SetActive(i >= _pill.level);

					// Icons
					if (m_powerDefs[i] != null) {
						// Search icon within the spritesheet
						string iconName = m_powerDefs[i].GetAsString("icon");
						Image img = m_powers[i].FindComponentRecursive<Image>("PowerIcon");
						img.sprite = GetFromCollection(ref m_allPowerIcons, iconName);
						img.SetNativeSize();

						// Store for further use
						m_powerIcons[i] = img.sprite;
					}
				}

				// update name
				m_name.text = _pill.nameLocalized;

				// update level
				for (int i = 0; i < m_upgrades.Length; i++) {
					m_upgrades[i].SetActive(i < _pill.level);
				}
			}

			m_preview = _pill;
			m_preview.Select(true);

			Wardrobe.Equip(m_dragonSku, m_preview.sku);

			if (_pill.level == 0) {
				ShowButtons(true, false);
			} else {
				ShowButtons(false, true);
			}
		}
	}

	/// <summary>
	/// A tooltip is about to be opened.
	/// In this context, it means that a power button has been pressed.
	/// </summary>
	/// <param name="_tooltip">The tooltip about to be opened.</param>
	/// <param name="_trigger">The button which triggered the event.</param>
	public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Find out which power has been tapped (buttons have the trigger component)
		for(int i = 0; i < m_powers.Length; i++) {
			if(m_powers[i] == _trigger.gameObject) {
				// Found! Initialized tooltip with data from this power
				DefinitionNode def = m_powerDefs[i];

				// Name
				_tooltip.FindComponentRecursive<Text>("PowerupNameText").text = def.GetLocalized("tidName");

				// Desc
				_tooltip.FindComponentRecursive<Text>("PowerupDescText").text = DragonPowerUp.GetDescription(def.sku);	// Custom formatting depending on powerup type

				// Icon
				Image img = _tooltip.FindComponentRecursive<Image>("Icon");
				img.sprite = m_powerIcons[i];
				img.SetNativeSize();	// Icons already have the desired size

				// We're done!
				break;
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

			Wardrobe.Equip(m_dragonSku, m_using.sku);
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
			if (m_preview == m_using) {
				m_useButton.SetActive(false);
			} else {
				Text text = m_useButton.GetComponentInChildren<Text>();
				text.text = "USE";
			}
		}
	}
}
