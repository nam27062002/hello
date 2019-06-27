using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AnniversaryCakePanel : MonoBehaviour, IBroadcastListener {

	public enum State {
		Init = 0,
		EatCake,
		DigestCake
	}

	[SerializeField] private Image m_cakeImage;

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
			m_cakeImage.fillAmount = m_DragonSuperSize.time / m_DragonSuperSize.modeDuration;			
		}
	}

	private void ChangeState(State _newState) {
		switch(_newState) {
			case State.EatCake:
			m_cakeSlicesEaten = 0;
			m_cakeImage.fillAmount = 0;
			m_startHugeModeAtLastSlice = true;
			break;

			case State.DigestCake:
			Messenger.Broadcast(MessengerEvents.ANNIVERSARY_CAKE_FULL_EATEN);
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
				m_cakeImage.fillAmount = (float)m_cakeSlicesEaten / m_cakeSliceCount;
				
				if (m_startHugeModeAtLastSlice) {
					m_DragonPlayer.SetSuperSize(1f + m_sizeUpMultPerSlice * m_cakeSlicesEaten);
					
					if (m_cakeSlicesEaten == m_cakeSliceCount) {
						ChangeState(State.DigestCake);
					}
				}
			}
		} else if (m_state == State.DigestCake) {
			// add time
			m_DragonSuperSize.AddTime(m_timePerSlice);
		}
	}

	private void OnSuperSizeToggle(bool _activated, DragonSuperSize.Source _source) {
		if (_source == DragonSuperSize.Source.LETTERS) {
			if (_activated) {
				m_startHugeModeAtLastSlice = false;
			} else {
				if (m_cakeSlicesEaten == m_cakeSliceCount) {
					ChangeState(State.DigestCake);
				} else {
					m_DragonPlayer.SetSuperSize(1f + m_sizeUpMultPerSlice * m_cakeSlicesEaten);
				}
				m_startHugeModeAtLastSlice = true;
			}
		} else {
			if (!_activated) {
				ChangeState(State.EatCake);
			}
			m_startHugeModeAtLastSlice = true;
		}
	}
}
