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

	private DefinitionNode m_def;

	private int m_level;

	public int level { get { return m_level; } }
	public bool isDefault { get { return (m_def == null); } }
	public string sku { get { if (m_def != null) return m_def.sku; else return "default"; } }
	public string powerUpSet { get { if (m_def != null) return m_def.GetAsString("powerupSet"); else return ""; } }
	public string tidName { get { if (m_def != null) return m_def.Get("tidName"); else return "TID_DISGUISE_DEFAULT_NAME"; } }

	//------------------------------------------//

	private Image m_disguiseIcon;
	private GameObject m_iconBg;
	private GameObject m_lockIcon;
	private GameObject m_selection;
	private GameObject m_equippedIcon;


	//------------------------------------------//

	void Awake() {
		m_disguiseIcon = transform.FindComponentRecursive<Image>("DragonSkinIcon");

		m_iconBg = transform.FindObjectRecursive("IconBg");
		m_lockIcon = transform.FindObjectRecursive("IconLock");
		m_selection = transform.FindObjectRecursive("SelectionEffect");
		m_equippedIcon = transform.FindObjectRecursive("IconTick");
	}

	public void LoadAsDefault(Sprite _spr) {
		m_def = null;
		m_level = 1;

		m_iconBg.SetActive(false);
		m_lockIcon.SetActive(false);
		m_selection.SetActive(false);
		m_equippedIcon.SetActive(false);

		m_disguiseIcon.sprite = _spr;
	}

	public void Load(DefinitionNode _def, int _level, Sprite _spr) {
		m_def = _def;
		m_level = _level;

		if (_level > 0) {
			// Unlocked
			m_disguiseIcon.color = Color.white;
			m_iconBg.SetActive(false);
			m_lockIcon.SetActive(false);
		} else {
			// Locked
			m_disguiseIcon.color = Color.gray;
			m_iconBg.SetActive(true);
			m_lockIcon.SetActive(true);
		}

		m_equippedIcon.SetActive(false);
		m_selection.SetActive(false);

		m_disguiseIcon.sprite = _spr;
	}

	public void Use(bool _value) {
		if (m_level > 0) {
			m_iconBg.SetActive(_value);
			m_equippedIcon.SetActive(_value);
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
