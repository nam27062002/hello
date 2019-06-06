using UnityEngine;
using UnityEngine.UI;

public class UIIconFireColors : MonoBehaviour {

	[SerializeField] private Image m_baseImage = null;
	[SerializeField] private Image m_insideImage = null;
	[Space]
	[SerializeField] private Color m_baseColor = new Color(243f / 255f, 68f / 255f, 5f / 255f);
	[SerializeField] private Color m_insideColor = new Color(247f / 255f, 180f / 255f, 8f / 255f);

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
		if(m_baseImage != null) m_baseImage.color = m_baseColor;
		if(m_insideImage != null) m_insideImage.color = m_insideColor;
	}
}
