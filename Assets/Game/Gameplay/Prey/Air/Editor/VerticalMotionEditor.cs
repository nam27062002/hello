using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(VerticalMotion))]
public class VerticalMotionEditor : Editor {

	private VerticalMotion m_target;
	private Vector3 m_initialPosition;

	// Use this for initialization
	void Awake() {
		m_target = (VerticalMotion)target;	
		m_initialPosition = m_target.transform.position;
	}
	
	// Update is called once per frame
	void OnSceneGUI() {
		m_initialPosition = m_target.transform.position;

		DrawHandle();
				
		bool isTargetDirty = false;

		isTargetDirty = isTargetDirty || DrawFrequencyHandle();
		isTargetDirty = isTargetDirty || DrawAmplitudeHandle();

		if (isTargetDirty) {
			EditorUtility.SetDirty(m_target);
		}
	}

	public override void OnInspectorGUI() {
		m_initialPosition = m_target.transform.position;

		GUI.changed = false;

		m_target.amplitude = EditorGUILayout.FloatField("Amplitude", m_target.amplitude);
		m_target.frequency = EditorGUILayout.FloatField("Frequency", m_target.frequency);
		
		if (GUI.changed) {
			EditorUtility.SetDirty(m_target);
		}
	}

	private bool DrawFrequencyHandle() {

		EditorGUI.BeginChangeCheck();
		Vector3 actualPosition = m_initialPosition + GetOffset((3 * Mathf.PI * 0.5f), m_target.amplitude, m_target.frequency);

		actualPosition = MoveHandle(actualPosition);

		if (EditorGUI.EndChangeCheck()) {

			float deltaX = actualPosition.x - m_initialPosition.x;

			if (deltaX > 0) {
				m_target.frequency = (deltaX / (3f * VerticalMotion.DEFAULT_AMPLITUDE * 0.5f));
			} else {
				m_target.frequency = 0;
			}

			return true;
		}

		return false;
	}

	private bool DrawAmplitudeHandle() {

		EditorGUI.BeginChangeCheck();
		Vector3 firstPosition = m_initialPosition + Vector3.up * m_target.amplitude;
		Vector3 secondPosition = m_initialPosition + GetOffset( Mathf.PI, m_target.amplitude, m_target.frequency);
		
		firstPosition = MoveHandle(firstPosition);	

		if (EditorGUI.EndChangeCheck()) {
			m_target.amplitude = Mathf.Max(0, firstPosition.y - m_initialPosition.y);
			return true;
		}

		EditorGUI.BeginChangeCheck();
		secondPosition = MoveHandle(secondPosition);
		
		if (EditorGUI.EndChangeCheck()) {
			m_target.amplitude = Mathf.Max(0, m_initialPosition.y - secondPosition.y);
			return true;
		}
		return false;
	}

	private void DrawHandle() {

		Handles.color =  new Color(220f/255f, 20f/255f, 60f/255f, 1f);

		Vector3 lastPos = m_initialPosition + GetOffset(0, m_target.amplitude, m_target.frequency);

		for (int i = 1; i <= 40; i++) {
			float t = (3 * Mathf.PI * 0.5f) * (i / 40f);
			Vector3 pos = m_initialPosition + GetOffset(t, m_target.amplitude, m_target.frequency);
			Handles.DrawLine(lastPos, pos);

			lastPos = pos;
		}

		Handles.color = Color.gray;
		Handles.DrawLine(m_initialPosition + Vector3.left * 25f, lastPos + Vector3.right * 25f);
	}

	private Vector3 MoveHandle(Vector3 _pos) {
		
		Handles.color = new Color(0.76f, 0.23f, 0.13f, 1f);
		float size = HandleUtility.GetHandleSize(Vector3.zero) * 0.05f;
		return Handles.FreeMoveHandle(_pos, Quaternion.identity, size, Vector3.zero, Handles.DotCap);
	}

	private Vector3 GetOffset(float _t, float _a, float _f) {
		Vector3 offset = Vector2.zero;

		offset.x = (_t / (2 * Mathf.PI)) * (VerticalMotion.DEFAULT_AMPLITUDE * 2 * _f);
		offset.y = (Mathf.Cos(_t) * _a);

		return offset;
	}
}
