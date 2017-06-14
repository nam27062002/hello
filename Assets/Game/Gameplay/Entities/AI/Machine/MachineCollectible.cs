using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCollectible : MonoBehaviour, IMachine {		
		UnityEngine.Events.UnityAction m_deactivateCallback;


		private CollectibleViewControl m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;


		public Vector3 eye						{ get { return Vector3.zero; } }
		public Vector3 target					{ get { return Vector3.zero; } }
		public virtual Vector3 upVector 		{ get { return Vector3.up; } set {} }
		public Transform enemy 					{ get { return null;} }
		public bool isPetTarget 				{ get { return false;} set {} }
		public virtual float lastFallDistance 	{ get { return 0f; } }
		public virtual bool isKinematic 		{ get { return false; } set {} }

		public virtual Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		public virtual Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		public virtual Vector3 direction 		{ get { return Vector3.zero; } }
		public virtual Vector3 groundDirection	{ get { return Vector3.right; } }
		public virtual Vector3 velocity			{ get { return Vector3.zero; } }
		public virtual Vector3 angularVelocity	{ get { return Vector3.zero; } }

		public float biteResistance { get { return 0; } }
		public HoldPreyPoint[] holdPreyPoints { get{ return null; } }


		protected virtual void Awake() {
			m_transform = transform;
			m_viewControl = GetComponent<CollectibleViewControl>();
			m_entity = GetComponent<IEntity>();	
		}

		protected virtual void OnTriggerEnter(Collider _other) {
			if (_other.CompareTag("Player")) {				
				Reward reward = (m_entity as Entity).GetOnKillReward(false);

				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, m_transform, reward);

				m_viewControl.Collect();
				m_entity.Disable(true);
			} 
		}

		public void Spawn(ISpawner _spawner) {}

		public void Activate() {
			gameObject.SetActive(true);
			if (m_deactivateCallback != null)
				m_deactivateCallback();
		}

		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {
			gameObject.SetActive(false);
			m_deactivateCallback = _action;
			Invoke("Activate", duration);
		}

		public void OnTrigger(string _trigger, object[] _param = null) {}
		public void DisableSensor(float _seconds) {}
		public virtual void CheckCollisions(bool _value) { }
		public virtual void FaceDirection(bool _value) { }
		public bool HasCorpse() { return false; }
		public void ReceiveDamage(float _damage) {}

		public bool GetSignal(Signals.Type _signal) { return false;}
		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params = null) {}
		public object[] GetSignalParams(Signals.Type _signal) { return null;}

		public virtual void UseGravity(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }
		public bool IsDead(){ return false; }
		public bool IsDying(){ return false; }
		public bool IsFreezing(){ return false; }
		public void CustomFixedUpdate(){}

		public virtual bool Burn(Transform _transform) { return false; }
		public void AddExternalForce(Vector3 force) {}
		public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		public void SetVelocity(Vector3 _v) {}
		public void BiteAndHold() {}
		public void ReleaseHold() {}
		public void EndSwallowed(Transform _transform){}
		public void Bite() {}
		public void Drown() {}
		public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, bool _isPlayer) {}


		public void	EnterGroup(ref Group _group) {}
		public Group GetGroup() {return null;}
		public void LeaveGroup() {}


		public void EnterDevice(bool _isCage) {}
		public void LeaveDevice(bool _isCage) {}
		public virtual bool CanBeBitten() {return false;}
	}
}