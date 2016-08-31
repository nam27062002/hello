using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(CageBehaviour))]
public class CageBehaviourEditor : Editor 
{
	SerializedProperty m_hits;
	SerializedProperty m_onBreakParticle;
	SerializedProperty m_keyList;
	SerializedProperty m_valueList;

	void OnEnable()
    {
		m_onBreakParticle = serializedObject.FindProperty("m_onBreakParticle");
		m_hits = serializedObject.FindProperty("m_hits");
		m_keyList = m_hits.FindPropertyRelative("m_keyList");
		m_valueList = m_hits.FindPropertyRelative("m_valueList");
    }

	public override void OnInspectorGUI() 
	{
		serializedObject.Update();
		CageBehaviour myTarget = (CageBehaviour) target;
		for( DragonTier d = DragonTier.TIER_0; d < DragonTier.COUNT; d++ )
		{
			if ( !myTarget.m_hits.dict.ContainsKey(d) )
			{
				myTarget.m_hits.dict.Add( d, new CageBehaviour.ContainerHit() );
				EditorUtility.SetDirty( myTarget );
			}

			int properIndex = -1;
			for( int i = 0; i<m_keyList.arraySize; i++ )
			{
				if ( m_keyList.GetArrayElementAtIndex(i).intValue == (int)d )
				{
					properIndex = i;
					break;
				}
			}
			SerializedProperty hit = m_valueList.GetArrayElementAtIndex(properIndex);
			EditorGUILayout.PropertyField( hit, new GUIContent(  d.ToString()) , true);
		}

		EditorGUILayout.PropertyField(m_onBreakParticle);

		serializedObject.ApplyModifiedProperties();
	}
}
