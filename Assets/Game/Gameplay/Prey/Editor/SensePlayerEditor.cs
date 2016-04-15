using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(SensePlayer))]
public class SensePlayerEditor : Editor {

	private SensePlayer m_target;

	public void Awake() {
		m_target = (SensePlayer)target;
	}

	void OnSceneGUI() {

		DrawHandle();
	}

	public override void OnInspectorGUI() {

		SerializedProperty property = serializedObject.GetIterator();
		
		GUI.changed = false;
		if (property.NextVisible(true)) {
			do {
				if (property.name == "m_sensorMinRadius") {					
					property.floatValue = EditorGUILayout.FloatField("Sensor Min Radius", property.floatValue);

					SerializedProperty maxRadius = serializedObject.FindProperty("m_sensorMaxRadius");
					if (property.floatValue > maxRadius.floatValue) {
						maxRadius.floatValue = property.floatValue;
					}
				} else if (property.name == "m_sensorMaxRadius") {	
					property.floatValue = EditorGUILayout.FloatField("Sensor Max Radius", property.floatValue);

					SerializedProperty minRadius = serializedObject.FindProperty("m_sensorMinRadius");
					if (property.floatValue < minRadius.floatValue) {
						minRadius.floatValue = property.floatValue;
					}
				} else {
					EditorGUILayout.PropertyField(serializedObject.FindProperty(property.name), true);
				}
			} while (property.NextVisible(false));
		}

		serializedObject.ApplyModifiedProperties();

		if (GUI.changed) {
			EditorUtility.SetDirty(m_target);
		}
	}
	
	private void DrawHandle() {

		// Rotation
		float alphaFactor = 1f;//Mathf.Min(1f, m_target.fleeForce);
		float fromAngle = (-m_target.sensorAngle * 0.5f) + m_target.sensorAngleOffset;
		float toAngle = (m_target.sensorAngle * 0.5f) + m_target.sensorAngleOffset;

		Vector3 from = Vector3.right.RotateXYDegrees(fromAngle);
		Vector3 to = Vector3.right.RotateXYDegrees(toAngle);

		Vector3 pos = m_target.transform.position + m_target.sensorPosition;

		// Outter area
		Handles.color =  new Color(1f, 1f, 224f/255f, 0.125f * alphaFactor);
		Handles.DrawSolidDisc(pos, Vector3.forward, m_target.sensorMaxRadius);

		Handles.color =  new Color(1f, 1f, 224f/255f, 1f * alphaFactor);
		Handles.DrawWireDisc(pos, Vector3.forward, m_target.sensorMaxRadius);

		// inner area
		Handles.color =  new Color(220f/255f, 20f/255f, 60f/255f, 0.0625f * alphaFactor);
		Handles.DrawSolidArc(pos, Vector3.forward, from, m_target.sensorAngle, m_target.sensorMinRadius);
				
		Handles.color =  new Color(220f/255f, 20f/255f, 60f/255f, 1f * alphaFactor);
		Handles.DrawWireArc(pos, Vector3.forward, from, m_target.sensorAngle, m_target.sensorMinRadius);
		Handles.DrawLine(pos, pos + to * m_target.sensorMinRadius);
		Handles.DrawLine(pos, pos + from * m_target.sensorMinRadius);
	}
}
