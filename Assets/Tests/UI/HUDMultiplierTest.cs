using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDMultiplierTest : MonoBehaviour {

	[System.Serializable]
	public class MultiplierText {
		public TextMeshProUGUI baseText = null;
		public TextMeshProUGUI fillText = null;
	}

	public string text = "5";
	[Range(0f, 1f)] public float fillAmount = 0.5f;
	public MultiplierText[] targetTexts = new MultiplierText[0];

	private Vector2 m_fillTextureOffset = Vector2.zero;

	[Space]
	[Header("Debug display values, do not fill in the inspector")]
	public int m_fontMaterialInstanceID = -1;
	public int m_fontMaterialInstanceID_CACHED = -1;
	public int m_sharedFontMaterialInstanceID = -1;
	public int m_sharedFontMaterialInstanceID_CACHED = -1;
	public Material m_fontMaterial = null;
	public Material m_sharedFontMaterial = null;

	// Use this for initialization
	void Start () {
		Apply();
	}
	
	// Update is called once per frame
	void Update () {
		Apply();	
	}

	void Apply() {
		// The fill texture is setup in a way that the top half is transparent and the bottom half is tinted
		// When empty (value 0), map the top half to the text mesh
		// When full (value 1), map the bottom half to the text mesh
		// In between, interpolate fill texture offsetY between 0 and -0.5
		m_fillTextureOffset.y = Mathf.Lerp(0, -0.5f, fillAmount);

		for(int i = 0; i < targetTexts.Length; ++i) {
			if(targetTexts[i] != null) {
				// Set text and fill offset
				MultiplierText target = targetTexts[i];
				if(target.baseText != null) target.baseText.text = text;
				if(target.fillText != null) {
					target.fillText.text = text;
					if(i == 2) {
						if(m_fontMaterial == null) m_fontMaterial = target.fillText.fontMaterial;
						if(m_fontMaterial != null) {
							//m_fontMaterial.SetTextureOffset(ShaderUtilities.ID_FaceTex, m_fillTextureOffset);
							m_fontMaterialInstanceID_CACHED = m_fontMaterial.GetInstanceID();
						}

						if(m_sharedFontMaterial == null) m_sharedFontMaterial = target.fillText.fontMaterial;
						if(m_sharedFontMaterial != null) {
							m_sharedFontMaterial.SetTextureOffset(ShaderUtilities.ID_FaceTex, m_fillTextureOffset);
							m_sharedFontMaterialInstanceID_CACHED = m_sharedFontMaterial.GetInstanceID();
						}

						m_fontMaterialInstanceID = target.fillText.fontMaterial.GetInstanceID();
						m_sharedFontMaterialInstanceID = target.fillText.fontSharedMaterial.GetInstanceID();
					} else {
						target.fillText.fontMaterial.SetTextureOffset(ShaderUtilities.ID_FaceTex, m_fillTextureOffset);
					}
				}
			}
		}
	}
}
