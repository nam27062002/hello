using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCollectible : IMachine {
		UnityEngine.Events.UnityAction m_deactivateCallback;

        [SerializeField] private bool m_useSpawnerRotation = false;

        private CollectibleViewControl m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;

		private bool m_isCollected;

        private bool m_dieOutsideFrustumRestoreValue;


		override public Vector3 eye						{ get { return Vector3.zero; } }
		override public Vector3 target					{ get { return Vector3.zero; } }
		override public Vector3 upVector 		{ get { return Vector3.up; } set {} }
		override public Transform enemy 					{ get { return null; } }
		public bool isPetTarget 				{ get { return false; } set {} }
		override public float lastFallDistance 	{ get { return 0f; } }
		public virtual bool isKinematic 		{ get { return false; } set {} }

		override public Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		override public Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		override public Vector3 direction 		{ get { return Vector3.zero; } }
		override public Vector3 groundDirection	{ get { return Vector3.right; } }
		override public Vector3 velocity			{ get { return Vector3.zero; } }
		override public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		override public float biteResistance { get { return 0; } }
		override public HoldPreyPoint[] holdPreyPoints { get{ return null; } }



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

		override public void Spawn(ISpawner _spawner) {
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

		override public void Activate() {
			gameObject.SetActive(true);
			if (m_deactivateCallback != null)
				m_deactivateCallback();
		}

		override public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {
			gameObject.SetActive(false);
			m_deactivateCallback = _action;
			Invoke("Activate", duration);
		}

		override public void CustomUpdate() {
			if (m_isCollected) {
				if (m_viewControl.HasCollectAnimationFinished()) {
					m_entity.Disable(true);
				}
			}
		}


		//----------------------------------------------------------------------------------------------------------------------//
		override public void OnTrigger(int _triggerHash, object[] _param = null) {}
		override public void DisableSensor(float _seconds) {}
		override public  void CheckCollisions(bool _value) {}
		override public  void FaceDirection(bool _value) {}
		override public bool HasCorpse() { return false; }
		override public void ReceiveDamage(float _damage) {}

		override public bool GetSignal(Signals.Type _signal) { return false;}
		override public void SetSignal(Signals.Type _signal, bool _activated) {}
		override public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {}
		override public object[] GetSignalParams(Signals.Type _signal) { return null;}

		override public void UseGravity(bool _value) { }
		override public bool IsFacingDirection() { return false; }
		override public bool IsInFreeFall() { return false; }
		override public bool IsDead(){ return false; }
		override public bool IsDying(){ return false; }
		public bool IsFreezing(){ return false; }
        override public bool IsStunned() { return false; }
        override public bool IsInLove() { return false; }
        override public bool IsBubbled() { return false; }
        override public void CustomFixedUpdate(){}

		override public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) { return false; }
		override public bool Smash(IEntity.Type _source) { return false; }
		override public void AddExternalForce(Vector3 force) {}
		override public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		override public void SetVelocity(Vector3 _v) {}
		override public void BiteAndHold() {}
		override public void ReleaseHold() {}
		override public void EndSwallowed(Transform _transform){}
		override public void Bite() {}
		override public void Drown() {}
		override public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, IEntity.Type _source) {}


		override public void	EnterGroup(ref Group _group) {}
		override public Group GetGroup() {return null;}
		override public void LeaveGroup() {}


		override public void EnterDevice(bool _isCage) {}
		override public void LeaveDevice(bool _isCage) {}
		override public bool CanBeBitten() {return false;}
		//----------------------------------------------------------------------------------------------------------------------//
	}
}
