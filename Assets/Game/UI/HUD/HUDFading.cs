using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HUDFading : MonoBehaviour {

	public Image m_blackImage;
	enum State
	{
		NONE,
		FADE_OUT,
		WAITING_FADE_IN,
		FADE_IN
	};
	private State m_state = State.NONE;

	const float FADE_DURATION = 1.0f;
    float m_startTime = 0.0f;
	Color m_color = Color.black;

	bool m_skipFrame = false;
	float m_waitingTime = 0;

	public GameObject m_text;

	// Use this for initialization
	void Awake () 
	{
		m_blackImage =  GetComponent<Image>();
		m_blackImage.enabled = false;
		m_text.SetActive(false);
		m_color.a = 0;
		enabled = false;

		Messenger.AddListener(GameEvents.PLAYER_LEAVING_AREA, PlayerLeavingArea);
		Messenger.AddListener(GameEvents.GAME_AREA_ENTER, OnAreaStart);
	}
	
	// Update is called once per frame
	void OnDestroy () 
	{
		Messenger.RemoveListener(GameEvents.PLAYER_LEAVING_AREA, PlayerLeavingArea);
		Messenger.RemoveListener(GameEvents.GAME_AREA_ENTER, OnAreaStart);
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
        m_blackImage.enabled = false;
        enabled = false;
    }


    void PlayerLeavingArea()
	{
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
