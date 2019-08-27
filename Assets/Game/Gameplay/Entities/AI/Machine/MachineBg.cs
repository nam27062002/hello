using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineBg : IMachine {		
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] private MC_MotionAir m_motion = new MC_MotionAir();

		private Pilot m_pilot = null;

		private Group m_group; // this will be a reference

		override public Quaternion orientation 	{ get { return transform.rotation; } set { transform.rotation = value; } }
		override public Vector3 position { 	get { if (m_enableMotion && m_motion != null) return m_motion.position; else return transform.position; } 
									set { if (m_enableMotion && m_motion != null) m_motion.position = value; else transform.position = value; } 
		}

		override public Vector3 eye 				{ get { return transform.position; } }
		override public Vector3 target			{ get { return m_pilot.target; } }
		override public Vector3 direction 		{ get { if (m_enableMotion && m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		override public Vector3 groundDirection 	{ get { return Vector3.zero; } }
		override public Vector3 upVector  		{ get { if (m_enableMotion && m_motion != null) return m_motion.upVector; else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }
		override public Vector3 velocity			{ get { if (m_enableMotion && m_motion != null) return m_motion.velocity; else return Vector3.zero;} }
		override public Vector3 angularVelocity	{ get { if (m_enableMotion && m_motion != null) return m_motion.angularVelocity; else return Vector3.zero;} }

		override public float lastFallDistance { get { return 0; } }
		public bool isKinematic{ get { return false; } set { } }
		override public Transform enemy { get { return null; } }
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

		override public void Spawn(ISpawner _spawner) {
			m_motion.Init();
		}

		override public void Activate() {}
		override public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {}

		override public void OnTrigger(int _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger);
			}
		}

		// Physics Collisions and Triggers

		void OnCollisionEnter(Collision _collision) {
			OnTrigger(SignalTriggers.onCollisionEnter);
		}


		void OnTriggerEnter(Collider _other) {
			SetSignal(Signals.Type.Trigger, true);
		}

		void OnTriggerExit(Collider _other) {
			SetSignal(Signals.Type.Trigger, false);
		}
		//

		// Update is called once per frame
		override public void CustomUpdate() {
			if (m_enableMotion) m_motion.Update();
		}

		override public void CustomFixedUpdate() {
			if (m_enableMotion) m_motion.FixedUpdate();
		}

		override public void SetSignal(Signals.Type _signal, bool _activated) {

		}

		override public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			
		}

		override public bool GetSignal(Signals.Type _signal) {
			return false;
		}

		override public object[] GetSignalParams(Signals.Type _signal) {
			return null;
		}

		override public void DisableSensor(float _seconds) {}

		override public void UseGravity(bool _value) {}

		override public void CheckCollisions(bool _value) {}

		override public void FaceDirection(bool _value) {
			if (m_motion != null) {
				m_motion.faceDirection = _value;
			}
		}

		override public bool IsFacingDirection() {
			if (m_motion != null) {
				return m_motion.faceDirection;
			}
			return false;
		}

		override public bool IsInFreeFall() { 
			return false; 
		}

		override public bool HasCorpse() {
			return false;
		}

		// Group membership -> for collective behaviours
		override public void EnterGroup(ref Group _group) {
			if (m_group != _group) {
				if (m_group != null) {
					LeaveGroup();
				}

				m_group = _group;
				m_group.Enter(this);
			}
		}

		override public Group GetGroup() {
			return m_group;
		}

		override public void LeaveGroup() {
			if (m_group != null) {
				m_group.Leave(this);
				m_group = null;
			}
		}

		private void PlaySound(string _clip) {
			AudioController.Play(_clip, transform.position);
		}

		// External interactions
		override public void EnterDevice(bool _isCage) {}
		override public void LeaveDevice(bool _isCage) {}

		override public void ReceiveDamage(float _damage) {}

		override public bool IsDead() { return false; }

		override public bool IsDying() { return false; }

		public bool IsFreezing() { return false; }
        override public bool IsStunned() { return false; }
        override public bool IsInLove() { return false; }
        override public bool IsBubbled() { return false; }

        override public void Drown() { }

		override public bool CanBeBitten() { return false; }

		override public float biteResistance { get { return 0; }}

		override public void Bite() { }

		override public void BeginSwallowed(Transform _transform, bool _rewardPlayer, IEntity.Type _source) { }

		override public void EndSwallowed(Transform _transform) { }

		override public HoldPreyPoint[] holdPreyPoints { get { return null; } }

		override public void BiteAndHold() { }

		override public void ReleaseHold() { }

		override public Quaternion GetDyingFixRot() {
			return Quaternion.identity;
		}

		override public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			return false;
		}

		override public bool Smash( IEntity.Type _source ){
			return false;
		}

		override public void SetVelocity(Vector3 _v) {
			if (m_motion != null) {
				m_motion.SetVelocity(_v);
			}
		}

		override public void AddExternalForce(Vector3 _f) {}

		// Debug
		void OnDrawGizmosSelected() {}
	}
}