using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof(WaterController))]
public class WaterControllerEditor : Editor {

	public float m_startSpeed = 14;
	public float m_waterConstant = 7;
	public float m_waterIncreaseForce = 1;


	public override void OnInspectorGUI()
    {
		GUI.changed = false;

		serializedObject.Update ();
		DrawDefaultInspector();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField ("Movement Test Values", EditorStyles.boldLabel);


		m_startSpeed =  EditorGUILayout.FloatField ("Speed", m_startSpeed );
		m_waterConstant =  EditorGUILayout.FloatField ("Constant", m_waterConstant );
		m_waterIncreaseForce =  EditorGUILayout.FloatField ( "Add", m_waterIncreaseForce );

		if ( GUI.changed )
			EditorUtility.SetDirty (target);
    }


	void OnSceneGUI()
	{
		if ( target == null ) return;
		Transform _tr = ((WaterController)target).transform;
		Handles.color = Color.yellow;

		float squareSpeed = m_startSpeed * m_startSpeed;

		float parabolicConstant = (squareSpeed / 2.0f) - squareSpeed;

		float insideSquareRoot = (m_waterConstant * m_waterConstant) - 4 * m_waterIncreaseForce * parabolicConstant;

		float displacement = (-m_waterConstant + Mathf.Sqrt( insideSquareRoot ) ) / (2 * m_waterIncreaseForce);

		Vector3 deepPos = _tr.position + Vector3.down * displacement;
		Handles.DrawLine( _tr.position, deepPos);
		Handles.DrawLine( deepPos + Vector3.left * 100, deepPos + Vector3.right * 100);
	}
}
