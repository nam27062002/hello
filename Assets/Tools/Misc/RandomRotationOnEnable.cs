using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotationOnEnable : MonoBehaviour {

    public Vector3 m_fromAngles;
    public Vector3 m_toAngles;
    public bool m_local = true;

    public void OnEnable()
    {
        Vector3 rot = m_fromAngles;
        rot.x = Random.Range(m_fromAngles.x, m_toAngles.x);
        rot.x = Random.Range(m_fromAngles.x, m_toAngles.x);
        rot.x = Random.Range(m_fromAngles.x, m_toAngles.x);
        if ( m_local )
        {
            transform.localRotation = Quaternion.Euler(rot);
        }
        else
        {
            transform.rotation = Quaternion.Euler(rot);
        }
        
    }

}
