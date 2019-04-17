using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCollectible : MonoBehaviour, IMachine, ISpawnable {
		UnityEngine.Events.UnityAction m_deactivateCallback;

        [SerializeField] private bool m_useSpawnerRotation = false;

        private CollectibleViewControl m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;

		private bool m_isCollected;

        private bool m_dieOutsideFrustumRestoreValue;


		public Vector3 eye						{ get { return Vector3.zero; } }
		public Vector3 target					{ get { return Vector3.zero; } }
		public virtual Vector3 upVector 		{ get { return Vector3.up; } set {} }
		public Transform enemy 					{ get { return null; } }
		public bool isPetTarget 				{ get { return false; } set {} }
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

            m_dieOutsideFrustumRestoreValue = (m_entity as CollectibleEntity).dieOutsideFrustum;
		}

		protected virtual void OnTriggerEnter(Collider _other) {
			if (_other.CompareTag("Player")) {
				if (!m_isCollected) {
					Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.EATEN);

					// Initialize some death info
					m_entity.onDieStatus.source = IEntity.Type.PLAYER;
					m_entity.onDieStatus.reason = IEntity.DyingReason.EATEN;

					// Dispatch global event
					Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, m_transform, m_entity, reward);

					m_viewControl.Collect();

                    (m_entity as CollectibleEntity).dieOutsideFrustum = false;
					m_isCollected = true;
				}
			}
		}

		public void Spawn(ISpawner _spawner) {
			m_isCollected = false;

            if (m_useSpawnerRotation) {
                Quaternion rot = GameConstants.Quaternion.identity;
                if (_spawner != null) {
                    rot = _spawner.rotation;
                }
                m_transform.rotation = rot;
            }

            (m_entity as CollectibleEntity).dieOutsideFrustum = m_dieOutsideFrustumRestoreValue;
		}

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

		public void CustomUpdate() {
			if (m_isCollected) {
				if (m_viewControl.HasCollectAnimationFinished()) {
					m_entity.Disable(true);
				}
			}
		}


		//----------------------------------------------------------------------------------------------------------------------//
		public void OnTrigger(string _trigger, object[] _param = null) {}
		public void DisableSensor(float _seconds) {}
		public virtual void CheckCollisions(bool _value) {}
		public virtual void FaceDirection(bool _value) {}
		public bool HasCorpse() { return false; }
		public void ReceiveDamage(float _damage) {}

		public bool GetSignal(Signals.Type _signal) { return false;}
		public void SetSignal(Signals.Type _signal, bool _activated) {}
		public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {}
		public object[] GetSignalParams(Signals.Type _signal) { return null;}

		public virtual void UseGravity(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }
		public virtual bool IsInFreeFall() { return false; }
		public bool IsDead(){ return false; }
		public bool IsDying(){ return false; }
		public bool IsFreezing(){ return false; }
        public bool IsStunned() { return false; }
        public bool IsInLove() { return false; }
        public bool IsBubbled() { return false; }
        public void CustomFixedUpdate(){}

		public virtual bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) { return false; }
		public bool Smash(IEntity.Type _source) { return false; }
		public void AddExternalForce(Vector3 force) {}
		public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		public void SetVelocity(Vector3 _v) {}
		public void BiteAndHold() {}
		public void ReleaseHold() {}
		public void EndSwallowed(Transform _transform){}
		public void Bite() {}
		public void Drown() {}
		public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, IEntity.Type _source) {}


		public void	EnterGroup(ref Group _group) {}
		public Group GetGroup() {return null;}
		public void LeaveGroup() {}


		public void EnterDevice(bool _isCage) {}
		public void LeaveDevice(bool _isCage) {}
		public virtual bool CanBeBitten() {return false;}
		//----------------------------------------------------------------------------------------------------------------------//
	}
}
