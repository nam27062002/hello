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

	private DefinitionNode m_def;
	public DefinitionNode def { get { return m_def; } }

	private int m_level;
	public int level { get { return m_level; } }

	//------------------------------------------//

	private Image m_disguiseIcon;
	private GameObject m_lockIcon;
	private GameObject m_equipedIcon;
	private GameObject[] m_upgradeIcons;

	//------------------------------------------//


	void Awake() {
		m_disguiseIcon = transform.FindChild("DragonSkinIcon").GetComponent<Image>();

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

	public void Load(DefinitionNode _def, int _level) {
		m_def = _def;
		m_level = _level;

		m_lockIcon.SetActive(_level == 0);
		for (int i = 0; i < m_upgradeIcons.Length; i++) {
			m_upgradeIcons[i].SetActive(i < _level);
		}
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
