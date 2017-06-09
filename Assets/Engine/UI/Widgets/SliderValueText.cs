using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text;

[RequireComponent(typeof(Slider))]
public class SliderValueText : MonoBehaviour {

	public TextMeshProUGUI text = null;
	public bool showMax = true;
	public bool showDelta = false;

	private Slider slider = null;
	private StringBuilder m_sb = new StringBuilder();

	// Use this for initialization
	void Start () {
		// Get required components
		slider = GetComponent<Slider>();
		DebugUtils.Assert(text != null, "Required Component!!");
		DebugUtils.Assert(slider != null, "Required Component!!");

		// Subscribe to slider changed event
		slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });

		// Initialize text
		RefreshText();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnSliderValueChanged() {
		RefreshText();
	}

	void RefreshText() {
		m_sb.Length = 0;
		if(showMax) {
			m_sb.AppendLine(string.Format("{0:0.00}/{1}", slider.value, slider.maxValue));
		} else {
			m_sb.AppendLine(string.Format("{0:0.00}", slider.value));
		}

		if(showDelta) {
			m_sb.AppendLine(string.Format("{0:0.00}", slider.normalizedValue));
		}

		text.text = m_sb.ToString();
	}
}
