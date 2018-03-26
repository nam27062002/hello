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
				if(targetTexts[i].baseText != null) targetTexts[i].baseText.text = text;
				if(targetTexts[i].fillText != null) {
					targetTexts[i].fillText.text = text;
					targetTexts[i].fillText.fontMaterial.SetTextureOffset(ShaderUtilities.ID_FaceTex, m_fillTextureOffset);
				}
			}
		}
	}
}
