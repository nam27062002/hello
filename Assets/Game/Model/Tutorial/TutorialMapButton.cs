using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMapButton : MonoBehaviour {

	[SerializeField] private GameObject m_mapButton;
	[SerializeField] private RectTransform m_targetTransform;
	[SerializeField] private Image m_godRays;
	[SerializeField] private float m_delay = 5f;

	private enum State {
		Idle = 0,
		Delay,
		Intro,
		GodRays,
		Move,
		Disable
	}


	private DeltaTimer m_timer;
	private RectTransform m_rTransform;

	private State m_state;

	private Vector3 m_posO;
	private Color m_color;


	void Awake() {
		m_state = State.Idle;
	}

	// Use this for initialization
	void Start() {
		m_rTransform = transform as RectTransform;

		if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			GameObject.Destroy(m_rTransform.parent.gameObject);
		} else {
			m_mapButton.SetActive(false);

			if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {
				m_timer = new DeltaTimer();
				m_timer.Stop();

				m_rTransform.localScale = GameConstants.Vector3.zero;

				m_posO = m_rTransform.position;

				m_color = m_godRays.color;
				m_color.a = 0f;
				m_godRays.color = m_color;

				enabled = false;
				m_state = State.Idle;

				Messenger.AddListener(MessengerEvents.GAME_STARTED, StartAnim);
			} else {
				GameObject.Destroy(m_rTransform.parent.gameObject);
			}
		}
	}

	private void OnDestroy() {
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, StartAnim);
	}

	private void StartAnim() {
		m_timer.Start(m_delay * 1000f);
		m_state = State.Delay;
		enabled = true;
	}

	void Update() {
		if (m_state > State.Idle) {
			float dt = 0; 

			switch (m_state) {
			case State.Delay:
				dt = m_timer.GetDelta(CustomEase.EaseType.cubicIn_01);

				if (m_timer.IsFinished()) {
					m_timer.Start(500f);
					m_state = State.Intro;
				}
				break;

			case State.Intro:
				dt = m_timer.GetDelta(CustomEase.EaseType.cubicInOut_01);

				m_rTransform.localScale = Vector3.Lerp(GameConstants.Vector3.zero, GameConstants.Vector3.one * 2f, dt);
				if (m_timer.IsFinished()) {
					m_timer.Start(1500f);
					m_state = State.GodRays;
				}
				break;

			case State.GodRays: 
				dt = m_timer.GetDelta(CustomEase.EaseType.expoOut_01);

				m_color.a = Mathf.Lerp(0f, 1f, dt);
				m_godRays.color = m_color;
				if (m_timer.IsFinished()) {
					m_timer.Start(1250f);
					m_state = State.Move;
				}
				break;

			case State.Move: 
				dt = m_timer.GetDelta(CustomEase.EaseType.cubicInOut_01);

				m_color.a = Mathf.Lerp(1f, 0f, dt * 10f);
				m_godRays.color = m_color;
				m_rTransform.position = Vector3.Lerp(m_posO, m_targetTransform.position, dt);
				m_rTransform.localScale = Vector3.Lerp(GameConstants.Vector3.one * 2f, GameConstants.Vector3.one, dt);
				if (m_timer.IsFinished()) {
					m_state = State.Disable;
				}
				break;

			case State.Disable:
				OnTweenEnd();
				break;
			}
		}
	}

	public void OnTweenEnd() {
		m_mapButton.SetActive(true);
		GameObject.Destroy(m_rTransform.parent.gameObject);
	}
}
