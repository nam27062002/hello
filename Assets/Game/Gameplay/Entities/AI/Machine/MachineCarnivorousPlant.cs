using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCarnivorousPlant : IMachine {
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		[SerializeField] protected MachineInflammable m_inflammable = new MachineInflammable();
		[SerializeField] protected MachineEdible m_edible = new MachineEdible();

		UnityEngine.Events.UnityAction m_deactivateCallback;

		private Pilot m_pilot = null;
		private ViewControlCarnivorousPlant m_viewControl = null;
		private IEntity m_entity = null;
		private Transform m_transform;
		private Transform m_eye; // for aiming purpose

        private RaycastHit[] m_raycastHits;
        private RaycastHit[] m_hitResults;
        private bool[] m_hasHit;

        private Vector3 m_upVector;

		private Signals m_signals;

		override public Vector3 eye		{ get { return m_eye.position; } }
		override public Vector3 target	{ get { return Vector3.zero; } }
		override public Vector3 upVector { get { return m_upVector; } set {} }

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

		override public Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		override public Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		override public Vector3 direction 		{ get { return Vector3.zero; } }
		override public Vector3 groundDirection	{ get { return Vector3.right; } }
		override public Vector3 velocity			{ get { return Vector3.zero; } }
		override public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		override public HoldPreyPoint[] holdPreyPoints { get{ return null; } }

		protected void Awake() {
			m_transform = transform;
			m_eye = m_transform.Find("eye");
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControlCarnivorousPlant>();
			m_entity = GetComponent<IEntity>();	

			m_sensor.Attach(this, m_entity, m_pilot);
			m_edible.Attach(this, m_entity, m_pilot);
			m_inflammable.Attach(this, m_entity, m_pilot);

            m_raycastHits = new RaycastHit[255];
            m_hitResults = new RaycastHit[4];
            m_hasHit = new bool[4]; 

			m_signals = new Signals(this);
			m_signals.Init();

			m_signals.SetOnEnableTrigger(Signals.Type.Alert, SignalTriggers.onAlert);
			m_signals.SetOnDisableTrigger(Signals.Type.Alert, SignalTriggers.onIgnoreAll);

			m_signals.SetOnEnableTrigger(Signals.Type.Warning, SignalTriggers.onWarning);	
			m_signals.SetOnDisableTrigger(Signals.Type.Warning, SignalTriggers.onCalm);		

			m_signals.SetOnEnableTrigger(Signals.Type.Danger, SignalTriggers.onDanger);
			m_signals.SetOnDisableTrigger(Signals.Type.Danger, SignalTriggers.onSafe);

			m_signals.SetOnEnableTrigger(Signals.Type.Critical, SignalTriggers.onCritical);

			m_signals.SetOnEnableTrigger(Signals.Type.Panic, SignalTriggers.onPanic);
			m_signals.SetOnDisableTrigger(Signals.Type.Panic, SignalTriggers.onRecoverFromPanic);

			m_signals.SetOnEnableTrigger(Signals.Type.Burning, SignalTriggers.onBurning);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.onDestroyed);
		}



		override public void Spawn(ISpawner _spawner) {
			m_signals.Init();
			m_sensor.Init();
			m_edible.Init();
			m_inflammable.Init();

			if (InstanceManager.player != null)	{
				DragonPlayer player = InstanceManager.player;
				HoldPreyPoint t = player.holdPreyPoints[player.holdPreyPoints.Length - 1];
				m_sensor.SetupEnemy(t.transform, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
			}

			m_upVector = Vector3.up;
			FindUpVector();
			m_transform.rotation = Quaternion.LookRotation(Vector3.forward, m_upVector);
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

		override public void OnTrigger(int _triggerHash, object[] _param = null) {
			if (_triggerHash == SignalTriggers.onDestroyed) {
				m_entity.Disable(true);
			} else if (_triggerHash == SignalTriggers.onBurning ){
				m_viewControl.Burn(m_inflammable.burningTime);
			}
		}

		override public void SetSignal(Signals.Type _signal, bool _activated) {
			m_signals.SetValue(_signal, _activated);
		}

		override public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			m_signals.SetValue(_signal, _activated, ref _params);
		}

		override public bool GetSignal(Signals.Type _signal) {
			if (m_signals != null)
				return m_signals.GetValue(_signal);

			return false;
		}

		override public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		override public void DisableSensor(float _seconds) {
			m_sensor.Disable(_seconds);
		}

		override public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			if (m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(9999f);
					m_inflammable.Burn(_transform, _source, instant, fireColorType);
				}
				return true;
			}
			return false;
		}

		override public bool Smash( IEntity.Type _source ) {
			if ( !IsDead() && !IsDying() )
			{
				SetSignal(Signals.Type.Destroyed, true);
				m_entity.onDieStatus.source = _source;
				m_entity.onDieStatus.reason = IEntity.DyingReason.DESTROYED;
				Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.DESTROYED);
				Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, m_transform, m_entity, reward);
				return true;
			}
			return false;
		}

		override public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		override public bool IsDying() {
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}

		override public void CustomUpdate() {
			m_inflammable.Update();

			if (IsDying() || IsDead()) {
				m_viewControl.Attack(false);
			} else {
				m_sensor.Update();

				if (m_pilot.IsActionPressed(Pilot.Action.Attack)) {
					UpdateAim();
					m_viewControl.Attack(true);
				} else {
					m_viewControl.Attack(false);
				}
			}
		}

		private void UpdateAim() {			
			Transform target = m_sensor.enemy;
			if (target != null) {
				Vector3 targetDir = target.position - m_eye.position;
				targetDir.z = 0f;

				targetDir.Normalize();
				Vector3 cross = Vector3.Cross(targetDir, m_upVector);
				float aim = cross.z;

				// blend between attack directions
				m_viewControl.Aim(aim);
			}
		}

		private void FindUpVector() {			
            Ray ray = new Ray();
            ray.origin = position;

            for (int i = 0; i < 4; i++) {
                m_hasHit[i] = false;
            }

            //down
            ray.direction = GameConstants.Vector3.down;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[0] = m_raycastHits[0]; m_hasHit[0] = true; }

            //up
            ray.direction = GameConstants.Vector3.up;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[1] = m_raycastHits[0]; m_hasHit[1] = true; }

            //right
            ray.direction = GameConstants.Vector3.right;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[2] = m_raycastHits[0]; m_hasHit[2] = true; }

            //left
            ray.direction = GameConstants.Vector3.left;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[3] = m_raycastHits[0]; m_hasHit[3] = true; }

			float d = 99999f;
			for (int i = 0; i < 4; i++) {
                if (m_hasHit[i]) {
                    if (m_hitResults[i].distance < d) {
                        d = m_hitResults[i].distance;

                        m_upVector = m_hitResults[i].normal;
                        position = m_hitResults[i].point;
					}
				}
			}
		}


		override public bool CanBeBitten() {
			if (!enabled)
				return false;
			if ( IsDead() || IsDying() )
				return false;
			
			return true;
		}

		override public float biteResistance { get { return m_edible.biteResistance; } }

		override public void Bite() {
			if (!IsDead()) {
				m_edible.Bite();
			}
		}

		override public bool HasCorpse() {
			if (m_viewControl != null) {
				return m_viewControl.HasCorpseAsset();
			}
			return false;
		}

		override public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, IEntity.Type _source) {
			m_viewControl.Bite();
			m_edible.BeingSwallowed(_transform, _rewardsPlayer, _source);
		}

		override public void EndSwallowed(Transform _transform){
			m_edible.EndSwallowed(_transform);
		}

		/**************************************************************************************************************/

		override public void CheckCollisions(bool _value) {}
		override public void FaceDirection(bool _value) {}
		override public void ReceiveDamage(float _damage) {}

		override public void UseGravity(bool _value) { }
		override public bool IsFacingDirection() { return false; }
		override public bool IsInFreeFall() { return false; }
		public bool IsFreezing(){ return false; }
        override public bool IsStunned() { return false; }        
        override public bool IsInLove() { return false; }
        override public bool IsBubbled() { return false; }

        override public void CustomFixedUpdate(){}

		override public void AddExternalForce(Vector3 force) {}
		override public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		override public void SetVelocity(Vector3 _v) {}
		override public void BiteAndHold() {}
		override public void ReleaseHold() {}

		override public void Drown() {}

		override public void	EnterGroup(ref Group _group) {}
		override public Group GetGroup() {return null;}
		override public void LeaveGroup() {}

		override public void EnterDevice(bool _isCage) {}
		override public void LeaveDevice(bool _isCage) {}

	}
}