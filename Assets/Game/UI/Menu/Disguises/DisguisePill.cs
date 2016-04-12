using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

public class DisguisePillEvent : UnityEvent<DisguisePill>{}

public class DisguisePill : MonoBehaviour, IPointerClickHandler {

	//------------------------------------------//

	public DisguisePillEvent OnPillClicked = new DisguisePillEvent();

	//------------------------------------------//

	private static Color m_defaultColor = new Color(176f / 255f, 164f / 255f, 153f / 255f);
	private static Color m_commonColor = new Color(242f / 255f, 196f / 255f, 156f / 255f);
	private static Color m_rareColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
	private static Color m_epicColor = new Color(255f / 255f, 168f / 255f, 0f / 255f);

	private static Dictionary<string, Color> m_colors;

	private DefinitionNode m_def;

	private int m_level;

	public int level { get { return m_level; } }
	public bool isDefault { get { return (m_def == null); } }
	public string sku { get { if (m_def != null) return m_def.sku; else return "default"; } }
	public string powerUpSet { get { if (m_def != null) return m_def.GetAsString("powerupSet"); else return ""; } }
	public string nameLocalized { get { if (m_def != null) return m_def.GetLocalized("tidName"); else return "default"; } }

	//------------------------------------------//

	private Image m_disguiseIcon;
	private GameObject m_lockIcon;
	private GameObject m_selection;
	private GameObject m_equipedIcon;
	private GameObject m_upgrades;
	private GameObject[] m_upgradeIcons;

	private Image m_bgDisguise;
	private Image m_bgFrame;
	private Image m_bgIcon;


	//------------------------------------------//

	void Awake() {
		m_colors = new Dictionary<string, Color>();
		m_colors.Add("common", m_commonColor);
		m_colors.Add("rare", m_rareColor);
		m_colors.Add("epic", m_epicColor);

		m_disguiseIcon = transform.FindChild("DragonSkinIcon").GetComponent<Image>();
		m_bgDisguise = transform.FindChild("BgDisguise").GetComponent<Image>();
		m_bgFrame = transform.FindChild("BgFrame").GetComponent<Image>();
		m_bgIcon = transform.FindChild("IconBg").GetComponent<Image>();

		m_lockIcon = transform.FindChild("IconLock").gameObject;
		m_lockIcon.SetActive(true);

		m_selection = transform.FindChild("SelectionEffect").gameObject;
		m_selection.SetActive(false);

		m_equipedIcon = m_bgIcon.transform.FindChild("IconTick").gameObject;
		m_equipedIcon.SetActive(false);

		m_upgrades = transform.FindChild("Upgrades").gameObject;

		m_upgradeIcons = new GameObject[Wardrobe.MAX_LEVEL];
		for (int i = 0; i < m_upgradeIcons.Length; i++) {
			Transform slot = m_upgrades.transform.FindChild("Slot" + (i + 1));
			m_upgradeIcons[i] = slot.FindChild("IconUpgrade").gameObject;
			m_upgradeIcons[i].SetActive(false);
		}
	}

	public void LoadAsDefault(Sprite _spr) {
		m_def = null;
		m_level = 1;

		m_upgrades.SetActive(false);
		m_lockIcon.SetActive(false);
		m_selection.SetActive(false);
		m_equipedIcon.SetActive(false);

		m_bgDisguise.color = m_defaultColor;
		m_bgFrame.color = m_defaultColor;
		m_bgIcon.color = m_defaultColor;

		m_disguiseIcon.sprite = _spr;
	}

	public void Load(DefinitionNode _def, int _level, Sprite _spr) {
		m_def = _def;
		m_level = _level;

		if (_level > 0) {
			m_disguiseIcon.color = Color.white;
			m_lockIcon.SetActive(false);
			m_bgIcon.gameObject.SetActive(false);
			m_upgrades.SetActive(true);
			for (int i = 0; i < m_upgradeIcons.Length; i++) {
				m_upgradeIcons[i].SetActive(i < _level);
			}
		} else {
			m_disguiseIcon.color = Color.gray;
			m_lockIcon.SetActive(true);
			m_bgIcon.gameObject.SetActive(true);
			m_upgrades.SetActive(false);
		}

		Color color = m_colors[m_def.GetAsString("rarity")];
		m_bgDisguise.color = color;
		m_bgFrame.color = color;
		m_bgIcon.color = color;

		m_disguiseIcon.sprite = _spr;
	}

	public void Use(bool _value) {
		if (m_level > 0) {
			m_equipedIcon.SetActive(_value);
			m_bgIcon.gameObject.SetActive(_value);
		}
	}

	public void Select(bool _value) {
		m_selection.SetActive(_value);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// This object has been clicked.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnPointerClick(PointerEventData _eventData) {
		// [AOC] Quick'n'dirty: find a parent snapping scroll list and move it to this item
		/*SnappingScrollRect scrollList = GetComponentInParent<SnappingScrollRect>();
		if(scrollList != null) {
			scrollList.SelectPoint(this, true);
		}*/
		OnPillClicked.Invoke(this);
	}
}
