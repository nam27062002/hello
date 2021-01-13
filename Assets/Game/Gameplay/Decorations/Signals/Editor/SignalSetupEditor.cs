using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SignalSetup))]
[CanEditMultipleObjects]
public class SignalSetupEditor : Editor {

	private SignalSetup m_component;


	public void Awake() {
		m_component = target as SignalSetup;
	}

	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		UpdateSignal();
	}

	void UpdateSignal() {
		m_component.UpdateSticker();
		m_component.UpdateArrowRotation();
		m_component.UpdateArrowVisibility();
	}
}
