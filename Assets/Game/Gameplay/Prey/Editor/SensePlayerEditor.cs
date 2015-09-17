using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(SensePlayer))]
public class SensePlayerEditor : Editor {

	private SensePlayer m_target;
	private bool m_isTargetDirty;

	public void Awake() {
		m_target = (SensePlayer)target;
		m_isTargetDirty = false;
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
		float angle = m_target.sensorAngle;

		Vector3 from;
		Vector3 to;
		/*if (m_target.prey.direction.x < 0) {
			from = Vector3.right.RotateXYDegrees(-m_target.sensorAngle * 0.5f + 180f);
			to = Vector3.right.Rot*ateXYDegrees(m_target.sensorAngle * 0.5f + 180f);
		} else */
		{
			from = Vector3.right.RotateXYDegrees(-m_target.sensorAngle * 0.5f);
			to = Vector3.right.RotateXYDegrees(m_target.sensorAngle * 0.5f);
		}

		// Outter area
		Handles.color =  new Color(1f, 1f, 224f/255f, 0.125f * alphaFactor);
		Handles.DrawSolidDisc(m_target.transform.position, Vector3.forward, m_target.sensorMaxRadius);

		Handles.color =  new Color(1f, 1f, 224f/255f, 1f * alphaFactor);
		Handles.DrawWireDisc(m_target.transform.position, Vector3.forward, m_target.sensorMaxRadius);

		// inner area
		Handles.color =  new Color(220f/255f, 20f/255f, 60f/255f, 0.0625f * alphaFactor);
		Handles.DrawSolidArc(m_target.transform.position, Vector3.forward, from, m_target.sensorAngle, m_target.sensorMinRadius);
				
		Handles.color =  new Color(220f/255f, 20f/255f, 60f/255f, 1f * alphaFactor);
		Handles.DrawWireArc(m_target.transform.position, Vector3.forward, from, m_target.sensorAngle, m_target.sensorMinRadius);
		Handles.DrawLine(m_target.transform.position, m_target.transform.position + to * m_target.sensorMinRadius);
		Handles.DrawLine(m_target.transform.position, m_target.transform.position + from * m_target.sensorMinRadius);
	}
}
