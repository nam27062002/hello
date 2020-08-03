using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FontTest : MonoBehaviour {

	public GameObject m_prefab = null;

	public string[] m_sampleTexts = new string[] { 
		"This is a latin 311 text",
		"한국어 키보드",
        "止待子和483文山球探負。球無",
        "覚ツチ文65均ラぐこ引城欧増",
        "Эа вим новум 460 оффекйяж"
	};

	public Color[] m_colors = new Color[] {
		Color.red, Color.green, Color.cyan, Color.yellow, Color.magenta
	};

	public int[] m_sizes = new int[] {
		14, 18, 22, 26, 30
	};

	private RectTransform m_rectTransform = null;

	void Awake() {
		m_rectTransform = this.transform as RectTransform;
	}

	void Start() {
	
	}
	
	void Update() {
	
	}

	public void OnGenerateNewSample() {
		GameObject newInstance = Instantiate<GameObject>(m_prefab);
		Text newTxt = newInstance.GetComponent<Text>();
		newTxt.text = m_sampleTexts[Random.Range(0, m_sampleTexts.Length)];
		newTxt.color = m_colors[Random.Range(0, m_colors.Length)];
		newTxt.fontSize = m_sizes[Random.Range(0, m_sizes.Length)];
		newInstance.transform.SetParent(this.transform, false);
		(newInstance.transform as RectTransform).anchoredPosition = new Vector2(
			Random.Range(m_rectTransform.rect.xMin, m_rectTransform.rect.xMax), 
		    Random.Range(m_rectTransform.rect.yMin, m_rectTransform.rect.yMax)
		);
		Debug.Log(System.String.Format("New Sample generated with text {0}, color {1} and size {2}", newTxt.text, newTxt.color, newTxt.fontSize));
	}
}
