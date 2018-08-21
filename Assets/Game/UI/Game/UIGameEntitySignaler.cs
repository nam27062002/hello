using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameEntitySignaler : MonoBehaviour {

    public Transform m_following;
    Camera m_gameCamera;
    Camera m_uiCamera;
    public Transform m_signalPivot;

    private void Start()
    {
        Canvas c = GetComponentInParent<Canvas>();
        if (c)
            m_uiCamera = c.worldCamera;

        m_gameCamera = InstanceManager.gameCamera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate () 
    {
        if ( m_uiCamera != null )
        {
            Vector3 posScreen = m_gameCamera.WorldToViewportPoint(m_following.position);
            posScreen.z = 0;
           // check is inside the camrea
            if ( posScreen.x >= 0 && posScreen.x <= 1 && posScreen.y >= 0 && posScreen.y <= 1)
            {
                m_signalPivot.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            }
            else
            {
                // if entity outside of the screen then clamp the position to the screen and then show  
                Vector3 clampedPosition = Vector3.zero;
                clampedPosition.x = Mathf.Clamp01(posScreen.x);
                clampedPosition.y = Mathf.Clamp01(posScreen.y);
                
                Vector3 diff = clampedPosition - posScreen;
                float rads = Mathf.Atan2(diff.y, diff.x);
                float angle = Mathf.Rad2Deg * rads;
                angle -= 90;
                // Apply rotation
                m_signalPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                posScreen = clampedPosition;
            }
            transform.position = m_uiCamera.ViewportToWorldPoint(posScreen); 
        }
	}
}
