using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HUDFading : MonoBehaviour, IBroadcastListener {

	private Image m_blackImage;
	enum State
	{
		NONE,
		FADE_OUT,
		WAITING_FADE_IN,
		FADE_IN
	};
	private State m_state = State.NONE;

	float FADE_DURATION = 1.0f;
    float m_startTime = 0.0f;
	Color m_color = Color.black;

	bool m_skipFrame = false;
	float m_waitingTime = 0;

	public GameObject m_text;


    private Material m_originalCurtain;
    private Material m_oldMaterial;

	// Use this for initialization
	void Awake () 
	{
		m_blackImage =  GetComponent<Image>();
		m_blackImage.enabled = false;
		m_text.SetActive(false);
		TextMeshProUGUI label = m_text.GetComponent<TextMeshProUGUI>();
		if ( label != null )
		{
			label.fontMaterial.renderQueue = 4000;
		}
		m_color.a = 0;
		enabled = false;

        m_oldMaterial = m_originalCurtain = m_blackImage.material;

		Messenger.AddListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, PlayerLeavingArea);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}
	
	// Update is called once per frame
	void OnDestroy () 
	{
		Messenger.RemoveListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, PlayerLeavingArea);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                OnAreaStart();
            }break;
        }
    }
    
	void Update()
	{
		if ( m_skipFrame )
		{
			m_skipFrame = false;
			return;
		}

		switch( m_state )
		{
			case State.FADE_OUT:
			{
				float tim = Time.time - m_startTime;

				float alpha = tim / FADE_DURATION;
				if ( alpha > 1.0f )
					alpha = 1.0f;
				m_color.a = alpha;
				m_blackImage.color = m_color;

				if ( tim >= FADE_DURATION )
				{
					m_waitingTime = 0;
					m_state = State.WAITING_FADE_IN;
				}
			}break;

			case State.WAITING_FADE_IN:
			{
				m_waitingTime += Time.deltaTime;
				if (m_waitingTime > 1.0f )
				{
					// Enable Loading time
					m_text.SetActive(true);
				}
			}break;

			case State.FADE_IN:
			{
				float tim = FADE_DURATION - (Time.time - m_startTime);
				float alpha = tim / FADE_DURATION;
				if ( alpha <= 0f )
					alpha = 0.0f;
				m_color.a = alpha;
				m_blackImage.color = m_color;
				if ( tim <= 0 )
				{
					m_state = State.NONE;
                    StartCoroutine(DisableInTime());
				}

			}break;
		}
	}


    IEnumerator DisableInTime()
    {
        yield return new WaitForSeconds(1.0f);
		if( m_state == State.NONE )
		{
			m_blackImage.enabled = false;
        	enabled = false;
		}
        
    }


    void PlayerLeavingArea(float estimatedLeavingTime)
	{
		FADE_DURATION = 0.5f * estimatedLeavingTime;
		StartFadeOut();
	}

	void OnAreaStart()
	{
		StartFadeIn();
	}

	// Screen to black
	void StartFadeOut()
	{
		enabled = true;
        m_blackImage.enabled = true;
        m_startTime = Time.time;
        m_state = State.FADE_OUT;
	}

	// Screen to transparent
	void StartFadeIn()
	{
		m_text.SetActive(false);
        m_blackImage.enabled = true;
        m_skipFrame = true;
		enabled = true;
        m_startTime = Time.time;
        m_state = State.FADE_IN;

	}

}
