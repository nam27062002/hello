using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(CustomParticleSystem))]
public class CustomParticleSystemEditor : Editor {

    private SerializedObject m_CustomParticleSystem;
    private SerializedProperty m_radius;



    // Use this for initialization
    void Awake () {
        m_CustomParticleSystem = new SerializedObject(target);
        m_radius = m_CustomParticleSystem.FindProperty("m_radius");
    }




    void OnSceneGUI()
    {
        m_CustomParticleSystem.Update();


        CustomParticleSystem cps = (CustomParticleSystem)target;

        m_radius.floatValue = Handles.RadiusHandle(Quaternion.identity, cps.transform.position, m_radius.floatValue);


        m_CustomParticleSystem.ApplyModifiedProperties();
    }

}
