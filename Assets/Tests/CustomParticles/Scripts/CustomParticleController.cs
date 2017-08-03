using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticleController : MonoBehaviour {


    private ParticleControl[] m_particleControlArray;
    private bool m_status = true;
	// Use this for initialization
	void Start () {
        m_particleControlArray = FindObjectsOfType<ParticleControl>();
	}
	

    void SetParticles(bool value)
    {
        foreach (ParticleControl pc in m_particleControlArray)
        {
            if (value)
            {
                pc.Play();
            }
            else
            {
                pc.Stop();
            }
        }
        m_status = value;
    }


	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("CustomParticleController: " + m_status);
            SetParticles(m_status ? false : true);
        }
	}
}
