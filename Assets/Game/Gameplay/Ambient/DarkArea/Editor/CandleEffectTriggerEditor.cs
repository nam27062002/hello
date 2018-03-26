using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CandleEffectTrigger))]	// True to be used by heir classes as well
public class CandleEffectTriggerEditor : Editor {

    CandleEffectTrigger m_target = null;

	private void OnEnable() {
        m_target = target as CandleEffectTrigger;

        m_target.m_tDirection = m_target.transform.Find("Direction").GetComponent<Transform>();
    }

    private void OnDisable() {
	}

    protected virtual void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Vector3 targetPosition = Handles.PositionHandle(m_target.m_tDirection.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_target, "Change Dark Area Trigger Direction");
            m_target.m_tDirection.position = targetPosition;
        }
    }

}
