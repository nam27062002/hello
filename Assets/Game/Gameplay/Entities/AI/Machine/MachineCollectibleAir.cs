using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCollectibleAir : IMachine {		
		UnityEngine.Events.UnityAction m_deactivateCallback;

		[SerializeField] private MC_MotionAir m_airMotion = new MC_MotionAir();
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		public MachineSensor sensor { get{ return m_sensor; } }

		private IEntity m_entity = null;
		private Pilot m_pilot = null;
		private Transform m_transform;


		override public Vector3 eye				{ get { return m_sensor.sensorPosition; } }
		override public Vector3 target			{ get { return m_pilot.target; } }
		override public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}
		public bool isPetTarget 		{ get { return false; } set {} }
		override public float lastFallDistance 	{ get { return 0f; } }
		public bool isKinematic 		{ get { return false; } set {} }

		override public Quaternion orientation 	{ get { return m_airMotion.orientation; } set { m_airMotion.orientation = value; } }
		override public Vector3 position			{ get { return m_airMotion.position; } set { m_airMotion.position = value; } }
		override public Vector3 direction 		{ get { return m_airMotion.direction; } }
		override public Vector3 groundDirection	{ get { return Vector3.right; } }
		override public Vector3 upVector 		{ get { return m_airMotion.upVector; } set { m_airMotion.upVector = value; } }
		override public Vector3 velocity			{ get { return m_airMotion.velocity; } }
		override public Vector3 angularVelocity	{ get { return m_airMotion.angularVelocity; } }

		override public float biteResistance { get { return 0; } }
		override public HoldPreyPoint[] holdPreyPoints { get{ return null; } }

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

		override public void Spawn(ISpawner _spawner) {
			m_signals.Init();

			if (InstanceManager.player != null)	{
				DragonPlayer player = InstanceManager.player;
				m_sensor.SetupEnemy(player.dragonEatBehaviour.mouth, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
			}

			m_airMotion.Init();
			m_sensor.Init();
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

		override public void SetSignal(Signals.Type _signal, bool _activated) {
			m_signals.SetValue(_signal, _activated);
		}

		override public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			m_signals.SetValue(_signal, _activated, ref _params);
		}

		override public bool GetSignal(Signals.Type _signal) {
			if (m_signals != null) {
				return m_signals.GetValue(_signal);
			} else {
				return false;
			}
		}

		override public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		override public void CustomUpdate() {
			if (!IsDead()) {				
				m_sensor.Update();
				m_airMotion.Update();

			}
		}

		override public void CustomFixedUpdate() {
			if (!IsDead()) {
				m_airMotion.FixedUpdate();
			}
		}

		//---------------------------------------------------------------

		override public void OnTrigger(string _trigger, object[] _param = null) {}
		override public void DisableSensor(float _seconds) {}
		override public void CheckCollisions(bool _value) {}
		override public void FaceDirection(bool _value) {}
		override public bool HasCorpse() { return false; }
		override public void ReceiveDamage(float _damage) {}


		override public void UseGravity(bool _value) { }
		override public bool IsFacingDirection() { return false; }
		override public bool IsInFreeFall() { return false; }
		override public bool IsDead(){ return false; }
		override public bool IsDying(){ return false; }
		public bool IsFreezing(){ return false; }
        override public bool IsStunned() { return false; }
        override public bool IsInLove() { return false; }
        override public bool IsBubbled() { return false; }

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
	}
}