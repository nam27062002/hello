using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

public class DisguisePillEvent : UnityEvent<DisguisePill>{}

[RequireComponent(typeof(ScrollRectSnapPoint))]
public class DisguisePill : MonoBehaviour, IPointerClickHandler {

	[SerializeField] private Color m_equippedTextColor = Color.white;
	[SerializeField] private Color m_getNowTextColor = Colors.gray;
	[SerializeField] private Color m_lockedTextColor = Color.red;

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
	// Internal references
	private Animator m_lockIconAnim;
	private ShowHideAnimator m_equippedFrame;
	private DOTweenAnimation m_equippedFX;
	private Localizer m_infoText;

	//------------------------------------------//

	void Awake() {
		m_icon = transform.FindComponentRecursive<Image>("DragonSkinIcon");
		m_lockIconAnim = transform.FindComponentRecursive<Animator>("PF_UILock");
		m_equippedFrame = transform.FindComponentRecursive<ShowHideAnimator>("EquippedFrame");
		m_equippedFX = transform.FindComponentRecursive<DOTweenAnimation>("EquippedFX");
		m_infoText = transform.FindComponentRecursive<Localizer>("InfoText");
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
			m_lockIconAnim.gameObject.SetActive(false);
		} else {
			// Locked
			m_icon.color = Color.gray;
			m_lockIconAnim.gameObject.SetActive(true);
		}

		RefreshText(false, m_owned, m_locked);

		m_equippedFrame.ForceHide(false);
		m_equippedFX.gameObject.SetActive(false);

		m_icon.sprite = _spr;
	}

	public void Equip(bool _equip, bool _animate = true) {
		if(_equip && m_owned) {
			m_equippedFrame.Show(_animate);
			m_equippedFX.gameObject.SetActive(true);
			m_equippedFX.DORestart();
		} else {
			m_equippedFrame.Hide(_animate);
			m_equippedFX.gameObject.SetActive(false);
		}

		RefreshText(_equip, m_owned, m_locked);
	}

	/// <summary>
	/// Properly set pill's text and color based on its state.
	/// </summary>
	/// <param name="_equipped">Is the disguise equipped?.</param>
	/// <param name="_owned">Is the disguise owned?</param>
	/// <param name="_locked">Is the disguise locked?</param>
	private void RefreshText(bool _equipped, bool _owned, bool _locked) {
		// Check by priority
		if(_locked) {
			// Locked, can't be neither owned nor equipped
			m_infoText.Localize("TID_LEVEL", (m_def.GetAsInt("unlockLevel") + 1).ToString());
			m_infoText.text.color = m_lockedTextColor;
		} else if(_owned) {
			// Can't be equipped if it's not owned!
			if(_equipped) {
				// Equipped
				m_infoText.Localize("TID_DISGUISES_EQUIPPED");
				m_infoText.text.color = m_equippedTextColor;
			} else {
				// Owned but not equipped
				m_infoText.Localize("");
				m_infoText.text.color = Color.white;
			}
		} else {
			// Not owned
			m_infoText.Localize("TID_DRAGON_GET_NOW");
			m_infoText.text.color = m_getNowTextColor;
		}
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

		// Small animation on the lock icon
		if(m_locked) {
			m_lockIconAnim.SetTrigger("bounce");
		}
	}
}
