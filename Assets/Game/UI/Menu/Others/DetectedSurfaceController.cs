using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectedSurfaceController : MonoBehaviour {
    [SerializeField] private PhotoScreenARFlow m_arFlow = null;
    [SerializeField] private DragControlRotation m_dragRotation = null;


	// Use this for initialization
	void Start () {
        if (m_arFlow.dragonLoader != null) {
            m_dragRotation.InitFromTarget(m_arFlow.dragonLoader.transform, true);
        }
	}
	
	// Update is called once per frame
	void OnEnable() {
        if (m_arFlow.dragonLoader != null) {
            m_dragRotation.InitFromTarget(m_arFlow.dragonLoader.transform, true);
        }
	}
}
