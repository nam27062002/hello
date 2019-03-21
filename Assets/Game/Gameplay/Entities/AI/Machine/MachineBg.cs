﻿using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineBg : MonoBehaviour, IMachine, ISpawnable {		
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] private MC_MotionAir m_motion = new MC_MotionAir();

		private Pilot m_pilot = null;

		private Group m_group; // this will be a reference

		public virtual Quaternion orientation 	{ get { return transform.rotation; } set { transform.rotation = value; } }
		public Vector3 position { 	get { if (m_enableMotion && m_motion != null) return m_motion.position; else return transform.position; } 
									set { if (m_enableMotion && m_motion != null) m_motion.position = value; else transform.position = value; } 
		}

		public Vector3 eye 				{ get { return transform.position; } }
		public Vector3 target			{ get { return m_pilot.target; } }
		public Vector3 direction 		{ get { if (m_enableMotion && m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Vector3 groundDirection 	{ get { return Vector3.zero; } }
		public Vector3 upVector  		{ get { if (m_enableMotion && m_motion != null) return m_motion.upVector; else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }
		public Vector3 velocity			{ get { if (m_enableMotion && m_motion != null) return m_motion.velocity; else return Vector3.zero;} }
		public Vector3 angularVelocity	{ get { if (m_enableMotion && m_motion != null) return m_motion.angularVelocity; else return Vector3.zero;} }

		public float lastFallDistance { get { return 0; } }
		public bool isKinematic{ get { return false; } set { } }
		public Transform enemy { get { return null; } set { } }
		public bool isPetTarget{ get { return false;} set { } }

		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_pilot = GetComponent<Pilot>();
			m_motion.Attach(this, null, m_pilot);
		}

		void OnDisable() {
			LeaveGroup();
		}

		public void Spawn(ISpawner _spawner) {
			m_motion.Init();
		}

		public void Activate() {}
		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {}

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger);
			}
		}

		// Physics Collisions and Triggers

		void OnCollisionEnter(Collision _collision) {
			OnTrigger(SignalTriggers.OnCollisionEnter);
		}


		void OnTriggerEnter(Collider _other) {
			SetSignal(Signals.Type.Trigger, true);
		}

		void OnTriggerExit(Collider _other) {
			SetSignal(Signals.Type.Trigger, false);
		}
		//

		// Update is called once per frame
		public virtual void CustomUpdate() {
			if (m_enableMotion) m_motion.Update();
		}

		public virtual void CustomFixedUpdate() {
			if (m_enableMotion) m_motion.FixedUpdate();
		}

		public void SetSignal(Signals.Type _signal, bool _activated) {

		}

		public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			
		}

		public bool GetSignal(Signals.Type _signal) {
			return false;
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return null;
		}

		public void DisableSensor(float _seconds) {}

		public void UseGravity(bool _value) {}

		public void CheckCollisions(bool _value) {}

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

		public bool IsInFreeFall() { 
			return false; 
		}

		public bool HasCorpse() {
			return false;
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
			AudioController.Play(_clip, transform.position);
		}

		// External interactions
		public void EnterDevice(bool _isCage) {}
		public void LeaveDevice(bool _isCage) {}

		public void ReceiveDamage(float _damage) {}

		public bool IsDead() { return false; }

		public bool IsDying() { return false; }

		public bool IsFreezing() { return false; }
        public bool IsStunned() { return false; }
        public bool IsInLove() { return false; }

		public void Drown() { }

		public bool CanBeBitten() { return false; }

		public float biteResistance { get { return 0; }}

		public void Bite() { }

		public void BeginSwallowed(Transform _transform, bool _rewardPlayer, IEntity.Type _source) { }

		public void EndSwallowed(Transform _transform) { }

		public HoldPreyPoint[] holdPreyPoints { get { return null; } }

		public void BiteAndHold() { }

		public void ReleaseHold() { }

		public Quaternion GetDyingFixRot() {
			return Quaternion.identity;
		}

		public virtual bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			return false;
		}

		public bool Smash( IEntity.Type _source ){
			return false;
		}

		public void SetVelocity(Vector3 _v) {
			if (m_motion != null) {
				m_motion.SetVelocity(_v);
			}
		}

		public void AddExternalForce(Vector3 _f) {}

		// Debug
		void OnDrawGizmosSelected() {}
	}
}