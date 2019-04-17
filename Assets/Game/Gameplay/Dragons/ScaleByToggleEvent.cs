using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleByToggleEvent : MonoBehaviour, IBroadcastListener {

    public BroadcastEventType m_eventType;
    public Range m_range;
    Coroutine m_routine;
    Transform m_transform;
    
    public void Start()
    {
        m_transform = transform;
        Broadcaster.AddListener(m_eventType, this);
    }
    
    void OnDestroy()
    {
        if ( Application.isPlaying )
        {
            Broadcaster.RemoveListener(m_eventType, this);
        }
    }
    
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        ToggleParam toggleParam = broadcastEventInfo as ToggleParam;
        if (m_routine != null)
        { 
            StopCoroutine(m_routine);
        }
        if ( toggleParam.value )
            m_routine = StartCoroutine( ScaleTo( m_range.max ) );
        else
            m_routine = StartCoroutine( ScaleTo( m_range.min ) );
    }
    
    IEnumerator ScaleTo( float target )
    {
        float timer = 0;
        float start = m_transform.localScale.y;
        while( timer < 0.5f )
        {
            float delta = timer / 0.5f;
            m_transform.localScale = GameConstants.Vector3.one * Mathf.Lerp(start, target, delta);
            timer += Time.deltaTime;
            yield return null;
        }
        m_transform.localScale = GameConstants.Vector3.one * target;
        yield return null;
    }
	
}
