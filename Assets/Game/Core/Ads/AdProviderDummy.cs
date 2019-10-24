// AdProviderDummy.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Dummy ad provider implementation to be used in the editor.
/// </summary>
public class AdProviderDummy : AdProvider {
	//------------------------------------------------------------------------//
	// SUBCLASSES															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Monobehaviour simulating ad logic.
	/// </summary>
	public class DummyAdEngine : MonoBehaviour {
		//--------------------------------------------------------------------//
		// CONSTANTS														  //
		//--------------------------------------------------------------------//
		public const float WAIT_DURATION = 1f;
		public const float AD_DURATION = 4f;

		public enum State {
			IDLE,
			WAITING,
			PLAYING,
			FINISHED
		}

		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		public State state {
			get;
			private set;
		}

		public float timer {
			get;
			private set;
		}

		public AdProviderDummy adProvider {
			get;
			set;
		}

		/// <summary>
		/// Initialization.
		/// </summary>
		public void Awake() {
			state = State.IDLE;
			timer = 0f;
		}

		/// <summary>
		/// Update loop.
		/// </summary>
		public void Update() {
			// Update timer
			if(timer > 0f) {
				timer -= Time.unscaledDeltaTime;
				if(timer < 0f) timer = 0f;
			}

			// Update state machine
			switch(state) {
				case State.IDLE: {
					// Nothing to do
				} break;

				case State.WAITING: {
					if(timer <= 0f) {
						ChangeState(State.PLAYING);
					}
				} break;

				case State.PLAYING: {
					if(timer <= 0f) {
						adProvider.OnAdPlayed(adProvider.ad, true, null);
						ChangeState(State.FINISHED);
					}
				} break;

				case State.FINISHED: {
					// Should never be in this state for more than one frame! Go to IDLE
					ChangeState(State.IDLE);
				} break;
			}
		}

		/// <summary>
		/// Change state of the ad engine.
		/// </summary>
		/// <param name="_newState"></param>
		public void ChangeState(State _newState) {
			// Stuff to do when leaving current state
			switch(state) {
				default: {
					// Nothing to do
				} break;
			}

			// Store new state
			State oldState = state;
			state = _newState;

			// Stuff to do when entering new state
			switch(state) {
				case State.WAITING: {
					// Reset timer
					timer = WAIT_DURATION;
				} break;

				case State.PLAYING: {
					// Reset timer
					timer = AD_DURATION;

					// Notify provider
					if(adProvider.onVideoAdOpen != null) {
						adProvider.onVideoAdOpen();
					}
				} break;

				case State.FINISHED: {
					// Notify provider
					if(adProvider.onVideoAdClosed != null) {
						adProvider.onVideoAdClosed();
					}
				} break;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public Ad ad {
		get { return m_ad; }
	}

	private DummyAdEngine m_adEngine = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override string GetId() {
        return "Dummy";
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_useAgeProtection"></param>
	/// <param name="_consentRestriction"></param>
	protected override void ExtendedInit(bool _useAgeProtection, bool _consentRestriction) {
		// Create a GameObject in order to have an Update loop
		// Destroy if one already exists
		GameObject go = GameObject.Find("DummyAdEngine");
		if(go != null) {
			Object.Destroy(go);
		}
		go = new GameObject("DummyAdEngine");
		Object.DontDestroyOnLoad(go);

		// Add and initialize engine
		m_adEngine = go.AddComponent<DummyAdEngine>();
		m_adEngine.adProvider = this;
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void ExtendedShowInterstitial() {
		m_adEngine.ChangeState(DummyAdEngine.State.WAITING);
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void ExtendedShowRewarded() {
		m_adEngine.ChangeState(DummyAdEngine.State.WAITING);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public override bool IsWaitingToPlayAnAnd() {
		return m_adEngine.state == DummyAdEngine.State.WAITING;
	}

	/// <summary>
	/// 
	/// </summary>
	public override void StopWaitingToPlayAnAd() {
		// The video can be cancelled only if it hasn't started playing yet
		if(m_adEngine.state == DummyAdEngine.State.WAITING) {
			Log("CANCELLING AD");
			OnAdPlayed(m_ad, false, "(IS) Cancelled by the user");
			m_adEngine.ChangeState(DummyAdEngine.State.FINISHED);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public override void ShowDebugInfo() {
		if(m_adEngine == null) {
			Log("Dummy Ad Provider: Engine is null");
		} else {
			Log("Ad Engine State: " + m_adEngine.state + "\nTimer: " + m_adEngine.timer);
		}
	}
}
