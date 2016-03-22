using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class DisguisePillEvent : UnityEvent<DisguisePill>{}

public class DisguisePill : MonoBehaviour, IPointerClickHandler {

	//------------------------------------------//

	public DisguisePillEvent OnPillClicked = new DisguisePillEvent();

	//------------------------------------------//

	private Color m_commonColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
	private Color m_rareColor = new Color(152f / 255f, 217f / 255f, 255f / 255f);
	private Color m_epicColor = new Color(186f / 255f, 129f / 255f, 234f / 255f);

	private DefinitionNode m_def;
	public DefinitionNode def { get { return m_def; } }

	private int m_level;
	public int level { get { return m_level; } }

	//------------------------------------------//

	private Image m_disguiseIcon;
	private GameObject m_lockIcon;
	private GameObject m_equipedIcon;
	private GameObject[] m_upgradeIcons;

	private Image m_bgDisguise;
	private Image m_bgFrame;

	//------------------------------------------//


	void Awake() {
		m_disguiseIcon = transform.FindChild("DragonSkinIcon").GetComponent<Image>();
		m_bgDisguise = transform.FindChild("BgDisguise").GetComponent<Image>();
		m_bgFrame = transform.FindChild("BgFrame").GetComponent<Image>();

		m_lockIcon = transform.FindChild("IconLock").gameObject;
		m_lockIcon.SetActive(true);

		m_equipedIcon = transform.FindChild("IconBg").gameObject;
		m_equipedIcon.SetActive(false);

		m_upgradeIcons = new GameObject[Wardrobe.MAX_LEVEL];

		for (int i = 0; i < m_upgradeIcons.Length; i++) {
			Transform slot = transform.FindTransformRecursive("Slot" + (i + 1));
			m_upgradeIcons[i] = slot.FindChild("IconUpgrade").gameObject;
			m_upgradeIcons[i].SetActive(false);
		}
	}

	public void Load(DefinitionNode _def, int _level, Sprite _spr) {
		m_def = _def;
		m_level = _level;

		m_lockIcon.SetActive(_level == 0);
		for (int i = 0; i < m_upgradeIcons.Length; i++) {
			m_upgradeIcons[i].SetActive(i < _level);
		}

		if (m_def.GetAsString("rarity") == "common") {
			m_bgDisguise.color = m_commonColor;
			m_bgFrame.color = m_commonColor;
		} else if (m_def.GetAsString("rarity") == "rare") {
			m_bgDisguise.color = m_rareColor;
			m_bgFrame.color = m_rareColor;
		} else if (m_def.GetAsString("rarity") == "epic") {
			m_bgDisguise.color = m_epicColor;
			m_bgFrame.color = m_epicColor;
		}

		m_disguiseIcon.sprite = _spr;
	}

	public void Use(bool _value) {
		m_equipedIcon.SetActive(_value);
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
