using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class HuntEventCursorController : MonoBehaviour {
	
	public Image m_bar;
	public RectTransform m_arrow;
	public HuntEventSpawner m_spawner;

	private Transform m_player;
	private Transform m_target;

	private float m_radius;

	// Use this for initialization
	void Start() {
	
		m_player = GameObject.Find("Player").transform;
		Messenger.AddListener<Transform, bool>(GameEvents.HUNT_EVENT_TOGGLED, EventToggle);

		m_radius = ((RectTransform)transform).sizeDelta.x * ((RectTransform)transform).lossyScale.x * 0.45f;

		gameObject.SetActive(false);
	}

	void OnDestroy() {

		Messenger.RemoveListener<Transform, bool>(GameEvents.HUNT_EVENT_TOGGLED, EventToggle);
	}
	
	// Update is called once per frame
	void Update() {

		bool offScreen = false;

		// update position
		Vector3 screenPos = Camera.main.WorldToScreenPoint(m_target.position);
		screenPos.z = 0;

		if (screenPos.x < m_radius) {
			screenPos.x = m_radius;
			offScreen = true;
		} else if (screenPos.x > Screen.width - m_radius) {
			screenPos.x = Screen.width - m_radius;
			offScreen = true;
		}

		if (screenPos.y < m_radius) {
			screenPos.y = m_radius;
			offScreen = true;
		} else if (screenPos.y > Screen.height - m_radius) {
			screenPos.y = Screen.height - m_radius;
			offScreen = true;
		}

		transform.position = screenPos;
		
		// update rotation
		float angle = 270; //point down!
		if (offScreen) {
			Vector3 v = m_player.position - m_target.position;
			angle = Vector3.Angle(Vector3.left, v);
			Vector3 cross = Vector3.Cross(Vector2.left, v);
			if (cross.z < 0) angle = 360 - angle;
		}
		m_arrow.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
				
		// update bar
		m_bar.fillAmount = (m_spawner.huntTimer / m_spawner.huntTimeSecs);
	}

	void EventToggle(Transform _entityLocation, bool _activated) {

		if (_activated) {
			gameObject.SetActive(true);
			Transform cursor = _entityLocation.FindChild("HuntCursor");
			if (cursor == null) {
				m_target = _entityLocation;
			} else {
				m_target = cursor;
			}
			transform.position = Camera.main.WorldToScreenPoint(m_target.position);
		} else {
			gameObject.SetActive(false);
		}
	}
}
