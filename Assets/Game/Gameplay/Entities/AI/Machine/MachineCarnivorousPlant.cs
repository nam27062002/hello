using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineCarnivorousPlant : MonoBehaviour, IMachine, ISpawnable {
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

		public Vector3 eye		{ get { return m_eye.position; } }
		public Vector3 target	{ get { return Vector3.zero; } }
		public Vector3 upVector { get { return m_upVector; } set {} }

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

		public Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		public Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		public Vector3 direction 		{ get { return Vector3.zero; } }
		public Vector3 groundDirection	{ get { return Vector3.right; } }
		public Vector3 velocity			{ get { return Vector3.zero; } }
		public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		public HoldPreyPoint[] holdPreyPoints { get{ return null; } }

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

			m_signals.SetOnEnableTrigger(Signals.Type.Alert, SignalTriggers.OnAlert);
			m_signals.SetOnDisableTrigger(Signals.Type.Alert, SignalTriggers.OnIgnoreAll);

			m_signals.SetOnEnableTrigger(Signals.Type.Warning, SignalTriggers.OnWarning);	
			m_signals.SetOnDisableTrigger(Signals.Type.Warning, SignalTriggers.OnCalm);		

			m_signals.SetOnEnableTrigger(Signals.Type.Danger, SignalTriggers.OnDanger);
			m_signals.SetOnDisableTrigger(Signals.Type.Danger, SignalTriggers.OnSafe);

			m_signals.SetOnEnableTrigger(Signals.Type.Critical, SignalTriggers.OnCritical);

			m_signals.SetOnEnableTrigger(Signals.Type.Panic, SignalTriggers.OnPanic);
			m_signals.SetOnDisableTrigger(Signals.Type.Panic, SignalTriggers.OnRecoverFromPanic);

			m_signals.SetOnEnableTrigger(Signals.Type.Burning, SignalTriggers.OnBurning);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.OnDestroyed);
		}



		public void Spawn(ISpawner _spawner) {
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

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (_trigger == SignalTriggers.OnDestroyed) {
				m_entity.Disable(true);
			} else if ( _trigger == SignalTriggers.OnBurning ){
				m_viewControl.Burn(m_inflammable.burningTime);
			}
		}

		public void SetSignal(Signals.Type _signal, bool _activated) {
			m_signals.SetValue(_signal, _activated);
		}

		public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			m_signals.SetValue(_signal, _activated, ref _params);
		}

		public bool GetSignal(Signals.Type _signal) {
			if (m_signals != null)
				return m_signals.GetValue(_signal);

			return false;
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		public void DisableSensor(float _seconds) {
			m_sensor.Disable(_seconds);
		}

		public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			if (m_entity.allowBurnable && m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(9999f);
					m_inflammable.Burn(_transform, _source, instant, fireColorType);
				}
				return true;
			}
			return false;
		}

		public bool Smash( IEntity.Type _source ) {
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

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		public bool IsDying() {
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}

		public void CustomUpdate() {
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


		public virtual bool CanBeBitten() {
			if (!enabled)
				return false;
			if ( IsDead() || IsDying() )
				return false;
			
			return true;
		}

		public float biteResistance { get { return m_edible.biteResistance; } }

		public void Bite() {
			if (!IsDead()) {
				m_edible.Bite();
			}
		}

		public bool HasCorpse() {
			if (m_viewControl != null) {
				return m_viewControl.HasCorpseAsset();
			}
			return false;
		}

		public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, IEntity.Type _source) {
			m_viewControl.Bite();
			m_edible.BeingSwallowed(_transform, _rewardsPlayer, _source);
		}

		public void EndSwallowed(Transform _transform){
			m_edible.EndSwallowed(_transform);
		}

		/**************************************************************************************************************/

		public virtual void CheckCollisions(bool _value) {}
		public virtual void FaceDirection(bool _value) {}
		public void ReceiveDamage(float _damage) {}

		public virtual void UseGravity(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }
		public virtual bool IsInFreeFall() { return false; }
		public bool IsFreezing(){ return false; }
        public bool IsStunned() { return false; }        
        public bool IsInLove() { return false; }

        public void CustomFixedUpdate(){}

		public void AddExternalForce(Vector3 force) {}
		public Quaternion GetDyingFixRot() { return Quaternion.identity; }
		public void SetVelocity(Vector3 _v) {}
		public void BiteAndHold() {}
		public void ReleaseHold() {}

		public void Drown() {}

		public void	EnterGroup(ref Group _group) {}
		public Group GetGroup() {return null;}
		public void LeaveGroup() {}

		public void EnterDevice(bool _isCage) {}
		public void LeaveDevice(bool _isCage) {}

	}
}