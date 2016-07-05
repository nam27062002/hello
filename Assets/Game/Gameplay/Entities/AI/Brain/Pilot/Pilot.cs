﻿using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class Pilot : MonoBehaviour {
		
		//TODO: tornar a crear la interficie??
		public enum Action {
			Boost = 0,
			Attack,
			Aim,
			Bite,
			Fire,
			Avoid,
			Pursuit,
			Scared,

			Count
		}

		//----------------------------------------------------------------------------------------------------------------

		protected Bounds m_area;
		public Bounds area { get { return m_area; } }

		protected GuideFunction m_guideFunction;
		public GuideFunction guideFunction { 
			get { return m_guideFunction; } 
			set { 
				m_guideFunction = value; 	
				if (m_guideFunction != null) {
					m_area = m_guideFunction.GetBounds();
				}
			} 
		}

		protected Vector3 m_homePosition;
		public Vector3 homePosition { get { return m_homePosition; } }

		protected Machine m_machine;

		protected bool[] m_actions;

		protected Vector3 m_target;

		protected float m_speed;
		public float speed { get { return m_speed; } }

		protected Vector3 m_externalImpulse;
		protected Vector3 m_impulse;
		public Vector3 impulse { get { return m_impulse; } }

		protected Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }


		//----------------------------------------------------------------------------------------------------------------

		protected virtual void Awake() {
			m_speed = 0;
			m_impulse = Vector3.zero;
			m_direction = Vector3.right;

			m_target = transform.position;

			m_actions = new bool[(int)Action.Count];
			m_machine = GetComponent<Machine>();
		}

		public bool IsActionPressed(Pilot.Action _action) {
			return m_actions[(int)_action];
		}

		public void PressAction(Pilot.Action _action) {
			m_actions[(int)_action] = true;
		}

		public void ReleaseAction(Pilot.Action _action) {
			m_actions[(int)_action] = false;
		}

		public virtual void OnTrigger(string _trigger) {}

		public void SetSpeed(float _speed) {
			m_speed = _speed;
		}

		public void SetDirection(Vector3 _dir) {
			m_direction = _dir;
		}

		public void GoTo(Vector3 _target) {
			m_target = _target;
		}
			
		public void Scared(bool _enable) {
			m_actions[(int)Action.Scared] = _enable;
		}

		public void Avoid(bool _enable) {
			m_actions[(int)Action.Avoid] = _enable;
		}

		public void Pursuit(bool _enable) {
			m_actions[(int)Action.Pursuit] = _enable;
		}

		public void AddImpulse(Vector3 _externalImpulse) {
			m_externalImpulse += _externalImpulse;
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(m_target, 0.25f);
		}
	}
}