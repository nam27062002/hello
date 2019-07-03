using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AnniversaryCakePanel : MonoBehaviour, IBroadcastListener {

	public enum State {
		Init = 0,
		EatCake,
        LaunchAnimation,
		DigestCake
	}

	[SerializeField] private Image m_cakeImage;
    [Tooltip("The speed factor when animating the cake counter")]
    [SerializeField] private float m_radialSpeed;

    // The real value of the cake (the value of cakeImage is being animated, so its not immediate)
    private float m_cakeValue;

    private DragonSuperSize m_DragonSuperSize;
	private DragonPlayer m_DragonPlayer;

	private int m_cakeSlicesEaten;
	private int m_cakeSliceCount;
	private float m_timePerSlice;
	private float m_sizeUpMultPerSlice;
	private State m_state;
	public State state { get { return m_state; } }

	private bool m_startHugeModeAtLastSlice;


	// Use this for initialization
	private void Start () {
		ChangeState(State.Init);


	}
	
	protected void OnEnable() {
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.AddListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnCakeSliceEaten);
		Messenger.AddListener(MessengerEvents.START_ALL_HUNGRY_LETTERS_COLLECTED, OnStartingLetters);
		Messenger.AddListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);		
	}

	protected void OnDisable() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.RemoveListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnCakeSliceEaten);	
		Messenger.RemoveListener(MessengerEvents.START_ALL_HUNGRY_LETTERS_COLLECTED, OnStartingLetters);
		Messenger.RemoveListener<bool, DragonSuperSize.Source>(MessengerEvents.SUPER_SIZE_TOGGLE, OnSuperSizeToggle);	
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_LEVEL_LOADED: {
				m_DragonPlayer = InstanceManager.player;
				m_DragonSuperSize = m_DragonPlayer.GetComponent<DragonSuperSize>();

				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
				m_cakeSliceCount = def.GetAsInt("anniversaryCakeSlices", 6);

				m_timePerSlice = m_DragonSuperSize.modeDuration / m_cakeSliceCount;
				m_sizeUpMultPerSlice = ((m_DragonSuperSize.sizeUpMultiplier - 1f) * 0.5f) / m_cakeSliceCount;

				ChangeState(State.EatCake);
			} break;
		}
	}

	// Update is called once per frame
	private void Update () {
		if (m_state == State.DigestCake) {
			m_cakeValue = m_DragonSuperSize.time / m_DragonSuperSize.modeDuration;			
		}

        // Animate the cake counter
        if (m_cakeImage.fillAmount != m_cakeValue)
        {
            float delta = m_cakeValue - m_cakeImage.fillAmount;
            m_cakeImage.fillAmount += Time.deltaTime * m_radialSpeed * delta; 
        }

    }

	private void ChangeState(State _newState) {
		switch(_newState) {
            case State.Init:
                // Reset the counter
                m_cakeValue = 0;
                m_cakeImage.fillAmount = 0;
                break;

            case State.EatCake:
			    m_cakeSlicesEaten = 0;
                m_cakeValue = 0;
			    m_startHugeModeAtLastSlice = true;
			    break;

            case State.LaunchAnimation:
                // Trigger the happy bday animated title and confetti before the bday mode
                Messenger.Broadcast(MessengerEvents.ANNIVERSARY_LAUNCH_ANIMATION);

                // Start bday mode after 2 secs delay
                UbiBCN.CoroutineManager.DelayedCall(
                    () => { ChangeState(State.DigestCake); }, 2f);

                break;

            case State.DigestCake:
                // Start the birthday mode
                Debug.Log("Start birthday mode");
                Messenger.Broadcast(MessengerEvents.ANNIVERSARY_START_BDAY_MODE);
			    break;
		}
		m_state = _newState;
	}
	
	//--------------------------------------------------
	//-- Callbacks
	//--------------------------------------------------

	private void OnStartingLetters() {
		m_startHugeModeAtLastSlice = false;
	}
	

	private void OnCakeSliceEaten(Vector3 _pos) {
		if (m_state == State.EatCake) {
			if (m_cakeSlicesEaten < m_cakeSliceCount) {
				m_cakeSlicesEaten++;
                m_cakeValue = Mathf.Clamp01 ( (float)m_cakeSlicesEaten / m_cakeSliceCount );
				
				if (m_startHugeModeAtLastSlice) {

                    // The dragon size grows with every piece eaten
					m_DragonPlayer.SetSuperSize(1f + m_sizeUpMultPerSlice * m_cakeSlicesEaten);
					
					if (m_cakeSlicesEaten == m_cakeSliceCount) {

                        // All the cake pieces eaten. Launch the happy bday animation to start bday mode
						ChangeState(State.LaunchAnimation);
					}
				}
			}
		} else if (m_state == State.DigestCake) {
			
			m_DragonSuperSize.AddTime(m_timePerSlice);

		}

        // Launch cake animation
        GetComponent<Animator>().SetTrigger("cakePieceEaten");
	}


	private void OnSuperSizeToggle(bool _activated, DragonSuperSize.Source _source) {
		if (_source == DragonSuperSize.Source.LETTERS) {
			if (_activated) {
                
                // We are in hungry mode, dont let the dragon grow again when eating the cake
				m_startHugeModeAtLastSlice = false;

			} else {

				if (m_cakeSlicesEaten == m_cakeSliceCount) {
                    // Hungry mode is over and bday mode is ready to start
					ChangeState(State.LaunchAnimation);

				} else {
                    // Return the dragon to the proper size according to the eaten pieces of cake
					m_DragonPlayer.SetSuperSize(1f + m_sizeUpMultPerSlice * m_cakeSlicesEaten);
				}

				m_startHugeModeAtLastSlice = true;
			}
		} else if (_source == DragonSuperSize.Source.CAKE)
        { 
            if (!_activated) {
				ChangeState(State.EatCake);
			}
			m_startHugeModeAtLastSlice = true;
		}
	}
}
