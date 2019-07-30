using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AnniversaryCakePanel : MonoBehaviour, IBroadcastListener {

    //------------------------------------------------------------------------//
    // ENUMS											                        //
    //------------------------------------------------------------------------//
    public enum State {
		Init = 0,
		EatCake,
        LaunchAnimation,
		DigestCake
	}

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    // Exposed
    [SerializeField] private GameObject m_cakeGroup;

    [Space(10)]
    [SerializeField] private Image m_radialCakeCounter;
    [Tooltip("Determines how fast is the cake counter updating its value")]
    [SerializeField] private float m_radialSpeedFactor;

    [Space(10)]
    [Tooltip("Time to wait between eating the last piece of cake and entering birthday mode")]
    [SerializeField] private float m_birthdayModeDelayInSecs;
    [SerializeField] private GameObject m_birthdayModeEffects;

    // Cached values
    private Animator m_cakeAnimator;

    // Internal logic
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

    private float m_timer;

	private bool m_startHugeModeAtLastSlice;


	// Use this for initialization
	private void Start () {

		ChangeState(State.Init);

        m_cakeAnimator = m_cakeGroup.GetComponent<Animator>();

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

				m_timePerSlice = m_DragonPlayer.data.superModeDuration / m_cakeSliceCount;
				m_sizeUpMultPerSlice = ((m_DragonPlayer.data.superSizeUpMultiplier - 1f) * 0.5f) / m_cakeSliceCount;

				ChangeState(State.EatCake);
			} break;
		}
	}

	// Update is called once per frame
	private void Update () {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C)) {
            OnCakeSliceEaten(Vector3.zero);
        }
#endif

        if (m_state == State.DigestCake) {
            m_cakeValue = m_DragonSuperSize.time / m_DragonSuperSize.modeDuration;
        } else if (m_state == State.LaunchAnimation) {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0f) {
                ChangeState(State.DigestCake);
            }
        }

        // Animate the cake counter
        if (m_radialCakeCounter.fillAmount != m_cakeValue)
        {
            float delta = m_cakeValue - m_radialCakeCounter.fillAmount;
            m_radialCakeCounter.fillAmount += Time.deltaTime * m_radialSpeedFactor * delta; 
        }
    }

	private void ChangeState(State _newState) {
        // Deactivate bday mode FX
        m_birthdayModeEffects.SetActive(false);

        switch (_newState) {

            case State.Init:
                // Reset the counter
                m_cakeValue = 0;
                m_radialCakeCounter.fillAmount = 0;
                break;

            case State.EatCake:
			    m_cakeSlicesEaten = 0;
                m_cakeValue = 0;
			    m_startHugeModeAtLastSlice = true;

                break;

            case State.LaunchAnimation:
                // Launch cake animation
                m_cakeAnimator.SetTrigger("fullCakeEaten");

                // Start bday mode after delay to let the cake animation play
                m_timer = m_birthdayModeDelayInSecs;
                break;

            case State.DigestCake:
                // Start the birthday mode
                Debug.Log("Start birthday mode");

                // Activate supersize and the "Hungry Bday" message
                Messenger.Broadcast(MessengerEvents.ANNIVERSARY_START_BDAY_MODE);

                if (FeatureSettingsManager.instance.LevelsLOD > FeatureSettings.ELevel4Values.low) {
                    // Activate bday mode FX (confetti and pink frame)
                    m_birthdayModeEffects.SetActive(true);
                }
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
        m_cakeAnimator.SetTrigger("cakePieceEaten");
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
