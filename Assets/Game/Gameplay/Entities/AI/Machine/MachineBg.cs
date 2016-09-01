﻿using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineBg : MonoBehaviour, IMachine, ISpawnable {		
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private Range m_railSeparation = new Range(0.5f, 1f);

		private Pilot m_pilot = null;

		private Group m_group; // this will be a reference


		public Vector3 position { 	get { if (m_enableMotion && m_motion != null) return m_motion.position; else return transform.position; } 
									set { if (m_enableMotion && m_motion != null) m_motion.position = value; else transform.position = value; } 
		}

		public Vector3 target	{ get { return m_pilot.target; } }
		public Vector3 direction { get { if (m_enableMotion && m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Vector3 upVector  { get { if (m_enableMotion && m_motion != null) return m_motion.upVector;  else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }
		public Vector3 velocity	{ get{ if (m_enableMotion && m_motion != null) return m_motion.velocity; else return Vector3.zero;} }
		public Vector3 angularVelocity	{ get{ if (m_enableMotion && m_motion != null) return m_motion.angularVelocity; else return Vector3.zero;} }

		public Transform enemy { 
			get {
				return null;
			}
		}

		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_pilot = GetComponent<Pilot>();
			m_motion.Attach(this, null, m_pilot);
		}

		void OnDisable() {
			LeaveGroup();
		}

		public void Spawn(ISpawner _spawner) 
		{
			m_motion.Init();
		}

		public void OnTrigger(string _trigger) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger);
			}
		}

		// Physics Collisions and Triggers

		void OnCollisionEnter(Collision _collision) {
			OnTrigger(SignalTriggers.OnCollisionEnter);
		}


		void OnTriggerEnter(Collider _other) {
			SetSignal(Signals.Type.Trigger, true, null);
		}

		void OnTriggerExit(Collider _other) {
			SetSignal(Signals.Type.Trigger, false, null);
		}
		//

		// Update is called once per frame
		void Update() {
			if (m_enableMotion) m_motion.Update();
		}

		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params) {
			
		}

		public bool GetSignal(Signals.Type _signal) {
			return false;
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return null;
		}

		public void UseGravity(bool _value) {
			if (m_motion != null) {
				m_motion.useGravity = _value;
			}
		}

		public void CheckCollisions(bool _value) {
			if (m_motion != null) {
				m_motion.checkCollisions = _value;
			}
		}

		public void FaceDirection(bool _value) {
			if (m_motion != null) {
				m_motion.faceDirection = _value;
			}
		}

		public bool IsFacingDirection() {
			if (m_motion != null) {
				return m_motion.faceDirection;
			}
			return false;
		}

		public void SetRail(uint _rail, uint _total) {
			if (m_motion != null) {
				if (_total > 1) {
					float railSeparation = m_railSeparation.GetRandom();
					m_motion.zOffset = (_rail * railSeparation) - (railSeparation * (_total / 2));
				} else {
					m_motion.zOffset = 0f;
				}
			}
		}

		// Group membership -> for collective behaviours
		public void	EnterGroup(ref Group _group) {
			if (m_group != _group) {
				if (m_group != null) {
					LeaveGroup();
				}

				m_group = _group;
				m_group.Enter(this);
			}
		}

		public Group GetGroup() {
			return m_group;
		}

		public void LeaveGroup() {
			if (m_group != null) {
				m_group.Leave(this);
				m_group = null;
			}
		}

		private void PlaySound(string _clip) {
			AudioManager.instance.PlayClip(_clip);
		}

		// External interactions
		public void LockInCage() {}
		public void UnlockFromCage() {}

		public void ReceiveDamage(float _damage) 
		{
			
		}

		public bool IsDead() {
			return false;
		}

		public float biteResistance { get { return 0; }}

		public void Bite() {
			
		}

		public void BeingSwallowed(Transform _transform, bool _rewardPlayer) {			
			
		}

		public List<Transform> holdPreyPoints { get{ return null; } }

		public void BiteAndHold() {
			
		}

		public void ReleaseHold() {
			
		}

		public virtual bool Burn(float _damage, Transform _transform) {
			
			return false;
		}

		public void SetVelocity(Vector3 _v) {
			if (m_motion != null) {
				m_motion.SetVelocity(_v);
			}
		}

		// Debug
		void OnDrawGizmosSelected() {
			
		}
	}
}