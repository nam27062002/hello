using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMapButton : MonoBehaviour {

	[SerializeField] private GameObject m_mapButton;
	[SerializeField] private GameObject m_mapButtonGodRays;
	[SerializeField] private RectTransform m_targetTransform;
	[SerializeField] private ParticleSystem m_godRays;
	[SerializeField] private float m_delay = 5f;

	private enum State {		
		Delay = 0,
		Idle,
		Intro,
		GodRays,
		Move,
		Disable
	}


	private DeltaTimer m_timer;
	private RectTransform m_rTransform;

	private State m_state;

	private float m_rotation;
	private Vector3 m_posO;
	private Color m_color;


	void Awake() {
		m_state = State.Idle;
	}

	// Use this for initialization
	void Start() {
		m_rTransform = transform as RectTransform;

		if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			m_mapButtonGodRays.SetActive(false);
			GameObject.Destroy(m_rTransform.parent.gameObject);
		} else {
			m_mapButton.SetActive(false);
			m_mapButtonGodRays.SetActive(false);

			if (UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN)) {				
				m_state = State.Idle;

				Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
			} else {
				GameObject.Destroy(m_rTransform.parent.gameObject);
			}
		}
	}

	private void OnDestroy() {
		Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEat);
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
	}

	private void OnGameStarted() {
		m_timer = new DeltaTimer();
		m_timer.Stop();

		m_rTransform.localScale = GameConstants.Vector3.zero;

		m_posO = m_rTransform.position;

		m_godRays.gameObject.SetActive(false);

		m_rotation = 0f;

		m_timer.Start(m_delay * 1000f);
		m_state = State.Delay;
	}

    private void OnEat (Transform _t, IEntity _e, Reward _reward, KillType _type)
    {
        if (_type == KillType.EATEN)
        {
            StartAnim();
        }
    }

	private void StartAnim() {
		m_timer.Start(500f);
		m_state = State.Intro;

		Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEat);
	}

	private void Update() {
		float dt = 0; 

		switch (m_state) {
		case State.Delay:
			dt = m_timer.GetDelta(CustomEase.EaseType.cubicIn_01);

			if (m_timer.IsFinished()) {
				m_state = State.Idle;
				Messenger.AddListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEat);
			}
			break;

		case State.Intro:
			dt = m_timer.GetDelta(CustomEase.EaseType.cubicInOut_01);

			m_rTransform.localScale = Vector3.Lerp(GameConstants.Vector3.zero, GameConstants.Vector3.one * 2f, dt);
			if (m_timer.IsFinished()) {
				m_timer.Start(1500f);

				m_godRays.gameObject.SetActive(true);

				m_state = State.GodRays;
			}
			break;

		case State.GodRays: 
			dt = m_timer.GetDelta(CustomEase.EaseType.expoOut_01);

			m_rotation = 6f * Mathf.Sin((float)((Mathf.PI * m_timer.GetTime()) / 500f));
			m_rTransform.localRotation = Quaternion.AngleAxis(m_rotation, GameConstants.Vector3.forward);

			m_color.a = Mathf.Lerp(0f, 1f, dt);
			if (m_timer.IsFinished()) {
				m_timer.Start(800f);

				ParticleSystem.EmissionModule em = m_godRays.emission;
				em.enabled = false;

				m_state = State.Move;
			}
			break;

		case State.Move: 
			dt = m_timer.GetDelta(CustomEase.EaseType.cubicInOut_01);

			m_rotation = Mathf.Lerp(m_rotation, 0f, dt * 2f);
			m_rTransform.localRotation = Quaternion.AngleAxis(m_rotation, GameConstants.Vector3.forward);

			m_color.a = Mathf.Lerp(1f, 0f, dt * 10f);
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

	public void OnTweenEnd() {
		m_mapButton.SetActive(true);
		m_mapButtonGodRays.SetActive(true);
		GameObject.Destroy(m_rTransform.parent.gameObject);
	}
}
