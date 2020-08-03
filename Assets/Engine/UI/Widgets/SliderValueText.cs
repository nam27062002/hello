using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Text;

[ExecuteInEditMode]
[RequireComponent(typeof(Slider))]
public class SliderValueText : MonoBehaviour {

	[SerializeField] private Slider m_slider = null;
	[SerializeField] private TextMeshProUGUI m_text = null;
	[Space]
	[SerializeField] private bool m_showMax = true;
	[SerializeField] private bool m_showDelta = false;
	[Space]
	[SerializeField] private int m_zeroPadding = 0;
	[SerializeField] private int m_decimalPlaces = 2;

	private StringBuilder m_sb = new StringBuilder();

	private void OnEnable () {
		// Subscribe to slider changed event
		if(m_slider != null) m_slider.onValueChanged.AddListener(OnSliderValueChanged);

		// Initialize text
		RefreshText();
	}

	private void OnDisable() {
		// Unsubscribe from slider changed event
		if(m_slider != null) m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
	}

	private void OnValidate() {
		RefreshText();
	}
	
	void OnSliderValueChanged(float _value) {
		RefreshText();
	}

	void RefreshText() {
		if(m_slider == null) return;
		if(m_text == null) return;

		// Clear
		m_sb.Length = 0;

		// Value
		m_sb.Append(StringUtils.FormatNumber(m_slider.value, m_decimalPlaces, m_zeroPadding));

		// Max
		if(m_showMax) {
			m_sb.Append("/");
			m_sb.Append(StringUtils.FormatNumber(m_slider.maxValue, m_decimalPlaces, m_zeroPadding));
		}

		// Delta
		if(m_showDelta) {
			m_sb.AppendLine();
			m_sb.Append("(");
			m_sb.Append(StringUtils.FormatNumber(m_slider.normalizedValue, 2));
			m_sb.Append(")");
		}

		m_text.text = m_sb.ToString();
	}
}
