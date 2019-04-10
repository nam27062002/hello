using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCollectibleAir : MonoBehaviour, IMachine, ISpawnable {		
		UnityEngine.Events.UnityAction m_deactivateCallback;

		[SerializeField] private MC_MotionAir m_airMotion = new MC_MotionAir();
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		public MachineSensor sensor { get{ return m_sensor; } }

		private IEntity m_entity = null;
		private Pilot m_pilot = null;
		private Transform m_transform;


		public Vector3 eye				{ get { return m_sensor.sensorPosition; } }
		public Vector3 target			{ get { return m_pilot.target; } }
		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}
		public bool isPetTarget 		{ get { return false; } set {} }
		public float lastFallDistance 	{ get { return 0f; } }
		public bool isKinematic 		{ get { return false; } set {} }

		public Quaternion orientation 	{ get { return m_airMotion.orientation; } set { m_airMotion.orientation = value; } }
		public Vector3 position			{ get { return m_airMotion.position; } set { m_airMotion.position = value; } }
		public Vector3 direction 		{ get { return m_airMotion.direction; } }
		public Vector3 groundDirection	{ get { return Vector3.right; } }
		public Vector3 upVector 		{ get { return m_airMotion.upVector; } set { m_airMotion.upVector = value; } }
		public Vector3 velocity			{ get { return m_airMotion.velocity; } }
		public Vector3 angularVelocity	{ get { return m_airMotion.angularVelocity; } }

		public float biteResistance { get { return 0; } }
		public HoldPreyPoint[] holdPreyPoints { get{ return null; } }

		private Signals m_signals;


		protected virtual void Awake() {
			m_transform = transform;
			m_entity = GetComponent<IEntity>();	
			m_pilot = GetComponent<Pilot>();

			m_airMotion.Attach(this, m_entity, m_pilot);
			m_sensor.Attach(this, m_entity, m_pilot);

			m_signals = new Signals(this);
			m_signals.Init();

			m_signals.SetOnEnableTrigger(Signals.Type.Alert, SignalTriggers.OnAlert);
			m_signals.SetOnDisableTrigger(Signals.Type.Alert, SignalTriggers.OnIgnoreAll);

			m_signals.SetOnEnableTrigger(Signals.Type.Warning, SignalTriggers.OnWarning);	
			m_signals.SetOnDisableTrigger(Signals.Type.Warning, SignalTriggers.OnCalm);		

			m_signals.SetOnEnableTrigger(Signals.Type.Danger, SignalTriggers.OnDanger);
			m_signals.SetOnDisableTrigger(Signals.Type.Danger, SignalTriggers.OnSafe);

			m_signals.SetOnEnableTrigger(Signals.Type.Critical, SignalTriggers.OnCritical);

			m_signals.SetOnEnableTrigger(Signals.Type.Panic, SignalTriggers.OnPanic);
			m_signals.SetOnDisableTrigger(Signals.Type.Panic, SignalTriggers.OnRecoverFromPanic);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.OnDestroyed);
		}

		protected virtual void OnTriggerEnter(Collider _other) {
			if (_other.CompareTag("Player")) {				
				Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.EATEN);

				// Dispatch global event
				Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, m_transform, m_entity, reward);

				m_entity.Disable(true);
			}
		}

		public void Spawn(ISpawner _spawner) {
			m_signals.Init();

			if (InstanceManager.player != null)	{
				DragonPlayer player = InstanceManager.player;
				m_sensor.SetupEnemy(player.dragonEatBehaviour.mouth, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
			}

			m_airMotion.Init();
			m_sensor.Init();
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

		public void SetSignal(Signals.Type _signal, bool _activated) {
			m_signals.SetValue(_signal, _activated);
		}

		public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			m_signals.SetValue(_signal, _activated, ref _params);
		}

		public bool GetSignal(Signals.Type _signal) {
			if (m_signals != null) {
				return m_signals.GetValue(_signal);
			} else {
				return false;
			}
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		public void CustomUpdate() {
			if (!IsDead()) {				
				m_sensor.Update();
				m_airMotion.Update();

			}
		}

		public virtual void CustomFixedUpdate() {
			if (!IsDead()) {
				m_airMotion.FixedUpdate();
			}
		}

		//---------------------------------------------------------------

		public void OnTrigger(string _trigger, object[] _param = null) {}
		public void DisableSensor(float _seconds) {}
		public virtual void CheckCollisions(bool _value) {}
		public virtual void FaceDirection(bool _value) {}
		public bool HasCorpse() { return false; }
		public void ReceiveDamage(float _damage) {}


		public virtual void UseGravity(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }
		public virtual bool IsInFreeFall() { return false; }
		public bool IsDead(){ return false; }
		public bool IsDying(){ return false; }
		public bool IsFreezing(){ return false; }
        public bool IsStunned() { return false; }
        public bool IsInLove() { return false; }
        public bool IsBubbled() { return false; }

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
	}
}