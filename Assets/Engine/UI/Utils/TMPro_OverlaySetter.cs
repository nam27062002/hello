using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPro_OverlaySetter : MonoBehaviour {

    public bool m_Overlay = false;
    private bool m_lastOverlay = false;
    private CanvasRenderer[] tmp = null;

    public void setOverlay(bool value)
    {
        for (int c = 0; c < tmp.Length; c++)
        {
            Material mat = tmp[c].GetMaterial();
            if (mat != null)
            {
                mat.renderQueue = m_Overlay ? 3500 : -1;
            }
        }
        m_lastOverlay = value;
    }

    // Use this for initialization
    void Start () {
        tmp = GetComponentsInChildren<CanvasRenderer>();

        setOverlay(m_Overlay);
	}

    void Update()
    {
        if (m_Overlay != m_lastOverlay)
        {
            setOverlay(m_Overlay);
        }
    }


}
