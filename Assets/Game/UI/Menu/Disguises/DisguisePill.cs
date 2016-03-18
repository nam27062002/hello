using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisguisePill : MonoBehaviour {

	//------------------------------------------//

	private string m_sku;
	private int m_level;

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

	public void Load(string _disguise, int _level) {
		m_sku = _disguise;
		m_level = _level;

		m_lockIcon.SetActive(_level == 0);
		for (int i = 0; i < m_upgradeIcons.Length; i++) {
			m_upgradeIcons[i].SetActive(i < _level);
		}
	}

	public void Use(bool _value) {
		m_equipedIcon.SetActive(_value);
	}
}
