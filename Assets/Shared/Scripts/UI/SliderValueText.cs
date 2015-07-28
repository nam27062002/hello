using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValueText : MonoBehaviour {

	public Text text = null;
	public bool showMax = true;

	private Slider slider = null;

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
		if(showMax) {
			text.text = string.Format("{0:0.00}/{1}", slider.value, slider.maxValue);
		} else {
			text.text = string.Format("{0:0.00}", slider.value);
		}
	}
}
