using UnityEngine;
using System.Collections;
using System;

namespace AI {
	public abstract class Pilot : ISpawnable {
		
		[Flags]
		public enum Action {
			None 				= (1 << 0),
			Boost 				= (1 << 1),
			Stop 				= (1 << 2),
			Attack 				= (1 << 3),
			Aim 				= (1 << 4),
			Bite 				= (1 << 5),
			Fire 				= (1 << 6),
			Jump 				= (1 << 7),
			Avoid 				= (1 << 8),
			Pursuit 			= (1 << 9),
			Scared 				= (1 << 10),
			Latching 			= (1 << 11),
			Button_A 			= (1 << 12), // "buttons" to manage
			Button_B 			= (1 << 13), // the specila actions 
			Button_C 			= (1 << 14), // of the current machine
			ExclamationMark 	= (1 << 15)
		}

		//----------------------------------------------------------------------------------------------------------------

		[SerializeField] private float m_blendSpeedFactor = 1f;
		[SerializeField] private float m_energy = 10f;
		[SerializeField] private float m_energyDrainSec = 1f;
		[SerializeField] private float m_energyRecoverSec = 1f;

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

		public IMachine m_machine;

		private Action m_actions;

		protected virtual float railSeparation { get { return 1f; } }

		// speed and leerping between values, trying to achieve smooth speed changes
		public virtual float speedFactor { get { return 1f; } set { } }

		private float m_freezeFactor = 1;
		protected virtual float freezeFactor{ get {return m_freezeFactor;} }

		protected bool m_stunned = false;

		private float m_moveSpeed;
		public float moveSpeed { 
			get {
				float ret = 0; 
				ret = m_moveSpeed * speedFactor * freezeFactor;
				return  ret;
			} 
		}
		
		private float m_boostSpeed;
		public float boostSpeed { 
			get { 
				float ret = 0;
				ret = m_boostSpeed * speedFactor * freezeFactor;
				return  ret;
			} 
		}

		private float m_currentSpeed;
		public float speed { 
			get { 
				float ret = 0;
				ret = m_currentSpeed * speedFactor * freezeFactor; 
				return ret;
			} 
		}

		private float m_zOffset;
		protected float zOffset { get { return m_zOffset; } }

		//
		protected bool m_boostAvailable;
		private float m_currentEnergy;

		protected Vector3 m_externalImpulse;
		protected Vector3 m_impulse;
		public Vector3 impulse { get { return m_impulse; } }

		protected Vector3 m_direction;
		protected bool m_directionForced;
		public Vector3 direction { get { return m_direction; } }

		protected Quaternion m_targetRotation;
		public Quaternion targetRotation{ get { return m_targetRotation; } }

		public virtual Vector3 target { get { return m_transform.position; } }

        protected Transform m_transform;

		//----------------------------------------------------------------------------------------------------------------

		protected virtual void Awake() {
            m_transform = transform;

            m_moveSpeed = 0;
			m_boostSpeed = 0;

			m_currentSpeed = 0;

			m_boostAvailable = true;
			m_currentEnergy = m_energy;

			m_zOffset = 0;

			m_impulse = Vector3.zero;
			m_direction = Vector3.right;
			m_directionForced = false;

			m_actions = Pilot.Action.None;
			m_machine = GetComponent<IMachine>();
		}

		public bool IsActionPressed(Pilot.Action _action) {
			return (m_actions & _action) != 0;
		}

		public void PressAction(Pilot.Action _action) {
			m_actions |= _action;
		}

		public void ReleaseAction(Pilot.Action _action) {
			m_actions &= ~_action;
		}

		public virtual void BrainExit() {}

		public virtual void OnTrigger(string _trigger, object[] _param = null) {}

		// all the movement will be offset to follow a specific rail
		public void SetRail(int _rail, int _total) {
			if (_total > 1) {
				m_zOffset = (_rail - (_total / 2)) * railSeparation;
			} else {
				m_zOffset = 0;
			}
		}

		public void SetMoveSpeed(float _speed, bool _blend = true) {
			m_moveSpeed = _speed;
			if (!_blend) {
				m_currentSpeed = m_moveSpeed;
			}
			ReleaseAction(Action.Stop);
		}

		public void SetBoostSpeed(float _boostSpeed, bool _blend = true) {
			m_boostSpeed = _boostSpeed;
			if (!_blend) {
				m_currentSpeed = m_boostSpeed;
			}
			ReleaseAction(Action.Stop);
		}

		public void SetFreezeFactor(float _factor) {
			m_freezeFactor = _factor;
		}

		public void SetStunned( bool _stunned ){
			m_stunned = _stunned;
		}

		public void Stop() {
			m_moveSpeed = 0f;
			m_currentSpeed = 0f;
			m_boostSpeed = 0f;
			m_impulse = Vector3.zero;
			m_externalImpulse = Vector3.zero;

            PressAction(Action.Stop);
		}

		public void SetDirection(Vector3 _dir, bool _force = false) {
			m_direction = _dir;
			m_directionForced = _force;
		}
					
		public void Scared(bool _enable) {
			if (_enable) PressAction(Action.Scared);
			else 		 ReleaseAction(Action.Scared);
		}

		public void Avoid(bool _enable) {
			if (_enable) PressAction(Action.Avoid);
			else 		 ReleaseAction(Action.Avoid);
		}

		public void Pursuit(bool _enable) {
			if (_enable) PressAction(Action.Pursuit);
			else 		 ReleaseAction(Action.Pursuit);
		}

		public void AddImpulse(Vector3 _externalImpulse) {
			m_externalImpulse += _externalImpulse;
		}

		override public void Spawn(ISpawner _spawner) {}

		override public void CustomUpdate() {
        
			if (m_boostAvailable && IsActionPressed(Action.Boost)) {
				m_currentSpeed = Mathf.Lerp(m_currentSpeed, m_boostSpeed, Time.deltaTime * m_blendSpeedFactor);
				m_currentEnergy = Mathf.Lerp(m_currentEnergy, 0f, Time.deltaTime * m_energyDrainSec);
				m_boostAvailable = m_currentEnergy > 0.1f;
			} else {
				m_currentSpeed = Mathf.Lerp(m_currentSpeed, m_moveSpeed, Time.deltaTime /* m_blendSpeedFactor*/);
				m_currentEnergy = Mathf.Lerp(m_currentEnergy, m_energy, Time.deltaTime * m_energyRecoverSec);
				m_boostAvailable = m_currentEnergy > (m_energy * 0.75f);
			}
		}
	}
}