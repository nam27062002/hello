using UnityEngine;
using UnityEngine.UI;

public class UIIconFireColors : MonoBehaviour {

	[SerializeField] private Color m_baseColor = new Color(243f / 255f, 68f / 255f, 5f / 255f);
	[SerializeField] private Color m_insideColor = new Color(247f / 255f, 180f / 255f, 8f / 255f);

	private Image m_iconBase = null;
	private Image m_iconInside = null;

	private void Awake() {
		UpdateColor();
	}

	public float alpha {
		get { return m_baseColor.a; }
		set {
			m_baseColor.a = value;
			m_insideColor.a = value;
			UpdateColor();
		}
	}

	public void UpdateColor() {
		if (m_iconBase == null) {
			m_iconBase = transform.FindChild("IconBase").GetComponent<Image>();
			m_iconInside = transform.FindChild("IconInside").GetComponent<Image>();
		}

		m_iconBase.color = m_baseColor;
		m_iconInside.color = m_insideColor;
	}
}
