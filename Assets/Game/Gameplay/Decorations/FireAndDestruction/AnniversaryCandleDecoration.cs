﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnniversaryCandleDecoration : MonoBehaviour, IFireNode {
	private enum State {
		IDLE = 0,
		LIGHT_UP
	}
	
	
	[SerializeField] private Rect m_rect;
	[SerializeField] private float m_lightUpTime = 60f;
	[SerializeField] private ViewParticleSpawner m_effect;
	
	private Transform m_transform;
	public Rect boundingRect { get { return m_rect; } }

	private BoundingSphere m_boundingSphere;
	public BoundingSphere boundingSphere { get { return m_boundingSphere; } }

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }


	private float m_timer;
	private State m_state;



	//-------------|
	//-- Generic --|
	//-------------|
	private void Awake() {
		m_transform = transform;
		
		m_timer = 0f;
		m_state = State.IDLE;
	}

	// Use this for initialization
	private void Start () {
		m_rect.center += (Vector2)m_transform.position;
		m_area = new CircleAreaBounds(m_transform.position, m_rect.size.magnitude);
		m_boundingSphere = new BoundingSphere(m_transform.position, 8f * m_transform.localScale.x);

		FirePropagationManager.Insert(this);
	}
	
	// Update is called once per frame
	private void Update () {
		if (m_state == State.LIGHT_UP) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_effect.Stop();
				m_state = State.IDLE;
			}
		}
	}
	//--------------


	//-------------------------|
	//-- Fire node interface --|
	//-------------------------|
	public void UpdateLogic() {/* nothing to do here */}
	public void SetEffectVisibility(bool _visible) {
		if (m_state == State.LIGHT_UP) {
			if (_visible) 	m_effect.Spawn();
			else 			m_effect.Stop();
		}
	}
	public void Burn(Vector2 _direction, bool _dragonBreath, DragonTier _tier, DragonBreathBehaviour.Type _breathType, IEntity.Type _source, FireColorSetupManager.FireColorType _fireColorType) {
		if (m_state == State.IDLE) {
			//light up candle
			m_effect.Spawn();
			m_timer = m_lightUpTime;
			m_state = State.LIGHT_UP;
		}
	}
	//--------------------------


	//-----------|
	//-- Debug --|
	//-----------|
	public void OnDrawGizmosSelected() {
		Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.5f);
		Gizmos.DrawSphere(transform.position + (Vector3)m_rect.center, 0.5f);

		Gizmos.color = Colors.fuchsia;
		Gizmos.DrawWireCube(transform.position + (Vector3)m_rect.center, m_rect.size);

		m_boundingSphere = new BoundingSphere(transform.position, 8f * transform.localScale.x);
		Gizmos.color = Colors.paleYellow;
		Gizmos.DrawWireSphere(m_boundingSphere.position, m_boundingSphere.radius);
	}
	//-----------
}
