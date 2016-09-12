using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class Pilot : MonoBehaviour {
		
		public enum Action {
			Boost = 0,
			Stop,
			Attack,
			Aim,
			Bite,
			Fire,
			Avoid,
			Pursuit,
			Scared,
			Latching,
			Button_A, // "buttons" to manage
			Button_B, // the specila actions 
			Button_C, // of the current machine

			Count
		}

		//----------------------------------------------------------------------------------------------------------------

		protected AreaBounds m_area;
		public AreaBounds area { get { return m_area; } set { m_area = value; } }

		protected IGuideFunction m_guideFunction;
		public IGuideFunction guideFunction { 
			get { return m_guideFunction; } 
			set { 
				m_guideFunction = value; 	
				if (m_guideFunction != null) {
					m_area = m_guideFunction.GetBounds();
				}
			} 
		}

		protected IMachine m_machine;

		protected bool[] m_actions;

		// speed and leerping between values, trying to achieve smooth speed changes
		protected virtual float speedFactor { get { return 1f; } }

		private float m_moveSpeed;
		public float moveSpeed { get { return m_moveSpeed * speedFactor; } }
		
		private float m_boostSpeed;
		public float boostSpeed { get { return m_boostSpeed * speedFactor; } }

		private float m_currentSpeed;
		public float speed { get { return m_currentSpeed; } }


		//
		protected Vector3 m_externalImpulse;
		protected Vector3 m_impulse;
		public Vector3 impulse { get { return m_impulse; } }

		protected Vector3 m_direction;
		protected bool m_directionForced;
		public Vector3 direction { get { return m_direction; } }

		public virtual Vector3 target { get { return transform.position; } }

		//----------------------------------------------------------------------------------------------------------------

		protected virtual void Awake() {
			m_moveSpeed = 0;
			m_boostSpeed = 0;

			m_currentSpeed = 0;

			m_impulse = Vector3.zero;
			m_direction = Vector3.right;
			m_directionForced = false;

			m_actions = new bool[(int)Action.Count];
			m_machine = GetComponent<IMachine>();
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

		public virtual void OnTrigger(string _trigger, object[] _param = null) {}

		public void SetMoveSpeed(float _speed, bool _blend = true) {
			m_moveSpeed = _speed;
			if (!_blend) {
				m_currentSpeed = m_moveSpeed;
			}
			m_actions[(int)Action.Stop] = false;
		}

		public void SetBoostSpeed(float _boostSpeed) {
			m_boostSpeed = _boostSpeed;
			m_actions[(int)Action.Stop] = false;
		}

		public void Stop() {
			m_moveSpeed = 0f;
			m_currentSpeed = 0f;
			m_boostSpeed = 0f;
			m_actions[(int)Action.Stop] = true;
		}

		public void SetDirection(Vector3 _dir, bool _force = false) {
			m_direction = _dir;
			m_directionForced = _force;
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

		protected virtual void Update() {
			if (IsActionPressed(Action.Boost)) {
				m_currentSpeed = Mathf.Lerp(m_currentSpeed, m_boostSpeed, Time.deltaTime * 2f);
			} else {
				m_currentSpeed = Mathf.Lerp(m_currentSpeed, m_moveSpeed, Time.deltaTime * 2f);
			}
		}
	}
}