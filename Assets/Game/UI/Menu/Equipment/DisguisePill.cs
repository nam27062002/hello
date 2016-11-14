using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

public class DisguisePillEvent : UnityEvent<DisguisePill>{}

[RequireComponent(typeof(ScrollRectSnapPoint))]
public class DisguisePill : MonoBehaviour, IPointerClickHandler {

	//------------------------------------------//

	public DisguisePillEvent OnPillClicked = new DisguisePillEvent();

	//------------------------------------------//
	private DefinitionNode m_def;
	public DefinitionNode def {
		get { return m_def; }
	}

	private ScrollRectSnapPoint m_snapPoint = null;
	public ScrollRectSnapPoint snapPoint {
		get {
			if(m_snapPoint == null) {
				m_snapPoint = GetComponent<ScrollRectSnapPoint>();
			}
			return m_snapPoint;
		}
	}

	private bool m_owned = false;
	public bool owned { get { return m_owned; } }

	private bool m_locked = true;
	public bool locked { get { return m_locked; } }

	private Image m_icon;
	public Image icon { get { return m_icon; } }

	//------------------------------------------//

	private GameObject m_iconBg;
	private GameObject m_lockIcon;
	private GameObject m_selection;
	private GameObject m_equippedIcon;


	//------------------------------------------//

	void Awake() {
		m_icon = transform.FindComponentRecursive<Image>("DragonSkinIcon");
		m_iconBg = transform.FindObjectRecursive("IconBg");
		m_lockIcon = transform.FindObjectRecursive("IconLock");
		m_selection = transform.FindObjectRecursive("SelectionEffect");
		m_equippedIcon = transform.FindObjectRecursive("IconTick");
	}

	public void LoadAsDefault(Sprite _spr) {
		m_def = null;

		m_iconBg.SetActive(false);
		m_lockIcon.SetActive(false);
		m_selection.SetActive(false);
		m_equippedIcon.SetActive(false);

		m_icon.sprite = _spr;
	}

	public void Load(DefinitionNode _def, bool _locked, bool _owned, Sprite _spr) {
		// Store data
		m_def = _def;
		m_locked = _locked;
		m_owned = _owned;

		// Locked?
		if(!_locked) {
			// Unlocked
			m_icon.color = Color.white;
			m_iconBg.SetActive(false);
			m_lockIcon.SetActive(false);
		} else {
			// Locked
			m_icon.color = Color.gray;
			m_iconBg.SetActive(true);
			m_lockIcon.SetActive(true);
		}

		m_equippedIcon.SetActive(false);
		m_selection.SetActive(false);

		m_icon.sprite = _spr;
	}

	public void Use(bool _value) {
		if(m_owned) {
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
		OnPillClicked.Invoke(this);
	}
}
