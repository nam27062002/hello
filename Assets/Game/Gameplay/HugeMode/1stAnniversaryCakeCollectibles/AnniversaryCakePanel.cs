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


	// Use this for initialization
	private void Start () {
		ChangeState(State.Init);
	}
	
	protected void OnEnable() {
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.AddListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnCakeSliceEaten);		
	}

	protected void OnDisable() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.RemoveListener<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, OnCakeSliceEaten);		
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_LEVEL_LOADED: {
				m_DragonPlayer = InstanceManager.player;
				m_DragonSuperSize = m_DragonPlayer.GetComponent<DragonSuperSize>();

				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
				m_cakeSliceCount = def.GetAsInt("anniversaryCakesToHuge", 6);

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
			if (m_DragonSuperSize.time <= 0f) {
				ChangeState(State.EatCake);
			}
		}
	}

	private void ChangeState(State _newState) {
		switch(_newState) {
			case State.EatCake:
			m_cakeSlicesEaten = 0;
			m_cakeImage.fillAmount = 0;
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
	
	private void OnCakeSliceEaten(Vector3 _pos) {
		if (m_state == State.EatCake) {
			m_cakeSlicesEaten++;
			m_cakeImage.fillAmount = (float)m_cakeSlicesEaten / m_cakeSliceCount;
			m_DragonPlayer.SetSuperSize(1f + m_sizeUpMultPerSlice * m_cakeSlicesEaten);

			if (m_cakeSlicesEaten >= m_cakeSliceCount) {
				ChangeState(State.DigestCake);
			}
		} else if (m_state == State.DigestCake) {
			// add time
			m_DragonSuperSize.AddTime(m_timePerSlice);
		}
	}
}
