using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRandomOnEnable : MonoBehaviour {

    public List<GameObject> m_objectsList = new List<GameObject>();
    private void OnEnable()
    {
        int max = m_objectsList.Count;
        int r = Random.Range(0, max);
        for (int i = 0; i < max; i++)
        {
            m_objectsList[i].SetActive( i == r );
        }

    }
}
