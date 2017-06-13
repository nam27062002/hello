using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Game.Currents;

namespace AI {
	public class Machine : MonoBehaviour, IMachine, ISpawnable, IAttacker, IMotion {	
		public static int GROUND_MASK;
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_affectedByDragonTrample = false;

		[SeparatorAttribute("Components")]
		protected MC_Motion m_motion = null; // basic machine doesn't have a motion component
		[SerializeField] private bool m_enableSensor = true;
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		public MachineSensor sensor
		{
			get{ return m_sensor; }
		}
		[SerializeField] protected MachineEdible m_edible = new MachineEdible();
		[SerializeField] protected MachineInflammable m_inflammable = new MachineInflammable();

		[SeparatorAttribute("Sounds")]
		[SerializeField] private string m_onSpawnSound = "";
		// [SerializeField] private string m_onEatenSound = "";


		protected Transform m_transform;
		protected IEntity m_entity = null;
		protected Pilot m_pilot = null;
		protected ViewControl m_viewControl = null;
		public ViewControl view { get { return m_viewControl; } }
		protected Collider m_collider = null;

		private Signals m_signals;

		private Group m_group; // this will be a reference

		public MachineEdible.RotateToMouthType rotateToMouth {
			get { return m_edible.rotateToMouth; }
			set { m_edible.rotateToMouth = value; }
		}

		private bool m_willPlaySpawnSound;

		public virtual Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		public virtual Vector3 position			{ get { return m_transform.position; } set { m_transform.position = value; } }
		public virtual Vector3 direction 		{ get { return Vector3.zero; } }
		public virtual Vector3 groundDirection	{ get { return Vector3.right; } }
		public virtual Vector3 upVector 		{ get { return Vector3.up; } set {} }
		public virtual Vector3 velocity			{ get { return Vector3.zero; } }
		public virtual Vector3 angularVelocity	{ get { return Vector3.zero; } }
		public virtual float lastFallDistance 	{ get { return 0f; } }
		public virtual bool isKinematic 		{ get { return false; } set { } }

		public Vector3 eye						{ get { if (m_enableSensor) return m_sensor.sensorPosition; else return m_transform.position; } }
		public Vector3 target					{ get { return m_pilot.target; } }


		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}

		bool m_isHolded = false;	// if machine being holded
		bool m_isPetTarget = false;
		public bool isPetTarget { get { return m_isPetTarget; } set { m_isPetTarget = value; } }


		// Currents
		private RegionManager 	m_regionManager;
		public Current			current { get; set; }
		private bool m_checkCurrents = false;
		public bool checkCurrents{ get{return m_checkCurrents;} set{ m_checkCurrents = value; } }
		private Vector3		m_externalForces;	// Mostly for currents

		// Freezing
		private bool m_freezing = false;
		public bool freezing
		{
			get{ return m_freezing; }
		}
		private float m_freezingMultiplier = 1;


		// Activating
		UnityEngine.Events.UnityAction m_deactivateCallback;
		//---------------------------------------------------------------------------------



		// Use this for initialization
		protected virtual void Awake() {
			GROUND_MASK = LayerMask.GetMask("Ground", "GroundVisible", "Obstacle", "PreyOnlyCollisions");

			m_transform = transform;

			m_entity = GetComponent<IEntity>();
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControl>();
			m_collider = GetComponent<Collider>();

			if (m_motion != null) 
				m_motion.Attach(this, m_entity, m_pilot);

			m_sensor.Attach(this, m_entity, m_pilot);
			m_edible.Attach(this, m_entity, m_pilot);
			m_inflammable.Attach(this, m_entity, m_pilot);

			m_signals = new Signals(this);
			m_signals.Init();

			m_signals.SetOnEnableTrigger(Signals.Type.Leader, SignalTriggers.OnLeaderPromoted);
			m_signals.SetOnDisableTrigger(Signals.Type.Leader, SignalTriggers.OnLeaderDemoted);

			m_signals.SetOnEnableTrigger(Signals.Type.Hungry, SignalTriggers.OnIsHungry);	
			m_signals.SetOnDisableTrigger(Signals.Type.Hungry, SignalTriggers.OnNotHungry);

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

			m_signals.SetOnEnableTrigger(Signals.Type.Chewing, SignalTriggers.OnChewing);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.OnDestroyed);

			m_signals.SetOnEnableTrigger(Signals.Type.FallDown, SignalTriggers.OnFallDown);
			m_signals.SetOnDisableTrigger(Signals.Type.FallDown, SignalTriggers.OnGround);

			m_signals.SetOnEnableTrigger(Signals.Type.LockedInCage, SignalTriggers.OnLockedInCage);
			m_signals.SetOnDisableTrigger(Signals.Type.LockedInCage, SignalTriggers.OnUnlockedFromCage);

			m_signals.SetOnEnableTrigger(Signals.Type.Invulnerable, SignalTriggers.OnInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.Invulnerable, SignalTriggers.OnVulnerable);

			m_signals.SetOnEnableTrigger(Signals.Type.InvulnerableBite, SignalTriggers.OnInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.InvulnerableBite, SignalTriggers.OnVulnerable);

			m_signals.SetOnEnableTrigger(Signals.Type.InvulnerableFire, SignalTriggers.OnInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.InvulnerableFire, SignalTriggers.OnVulnerable);

			m_externalForces = Vector3.zero;
		}

		void OnEnable() {

		}

		void OnDisable() {
			LeaveGroup();
		}

		public virtual void Spawn(ISpawner _spawner) {
			if (m_signals != null) 
				m_signals.Init();

			if (m_motion != null) 
				m_motion.Init();

			if (m_enableSensor) {
				m_sensor.Init();
				if (InstanceManager.player != null)	{
					DragonPlayer player = InstanceManager.player;
					m_sensor.SetupEnemy(player.dragonEatBehaviour.mouth, player.dragonEatBehaviour.eatDistanceSqr, player.dragonMotion.hitBounds);
				}
			}

			m_edible.Init();
			m_inflammable.Init();

			if (m_collider != null)
				m_collider.enabled = true;

			m_willPlaySpawnSound = !string.IsNullOrEmpty(m_onSpawnSound);

			if (_spawner != null) {
				m_checkCurrents = _spawner.SpawnersCheckCurrents();
			}
		}

		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {
			gameObject.SetActive(false);
			m_deactivateCallback = _action;
			Invoke("Activate", duration);
		}

		public void Activate() {
			gameObject.SetActive(true);
			if (m_deactivateCallback != null)
				m_deactivateCallback();
		}

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger, _param);
			}

			if (_trigger == SignalTriggers.OnDestroyed) {
				m_viewControl.Die(m_signals.GetValue(Signals.Type.Chewing), m_signals.GetValue(Signals.Type.Burning));
				if (m_motion != null) m_motion.Stop();
				if (m_collider != null) m_collider.enabled = false;
				m_entity.Disable(true);
			} else if (_trigger == SignalTriggers.OnBurning) {
				m_viewControl.Burn(m_inflammable.burningTime);
				if (m_motion != null) m_motion.Stop();
				if (m_collider != null) m_collider.enabled = false;
			} else if (_trigger == SignalTriggers.OnInvulnerable || _trigger == SignalTriggers.OnVulnerable) {
				m_entity.allowEdible = !(m_signals.GetValue(Signals.Type.Invulnerable) || m_signals.GetValue(Signals.Type.InvulnerableBite));
				m_entity.allowBurnable = !(m_signals.GetValue(Signals.Type.Invulnerable) || m_signals.GetValue(Signals.Type.InvulnerableFire));
			}
		}

		//-----------------------------------------------------------
		// Physics Collisions and Triggers
		protected virtual void OnCollisionEnter(Collision _collision) {
			object[] _params = new object[1]{_collision};
			OnTrigger(SignalTriggers.OnCollisionEnter, _params);
			SetSignal(Signals.Type.Collision, true, _params);

			if (m_motion != null) {
				if (((1 << _collision.collider.gameObject.layer) & GROUND_MASK) != 0) {
					m_motion.OnCollisionGroundEnter(_collision);
				}
			}
		}

		void OnCollisionStay(Collision _collision) {
			if (m_motion != null) {
				if (((1 << _collision.collider.gameObject.layer) & GROUND_MASK) != 0) {
					m_motion.OnCollisionGroundStay(_collision);
				}
			}
		}

		void OnCollisionExit(Collision _collision) {
			if (m_motion != null) {
				if (((1 << _collision.collider.gameObject.layer) & GROUND_MASK) != 0) {
					m_motion.OnCollisionGroundExit(_collision);
				}
			}

			SetSignal(Signals.Type.Collision, false);
		}

		void OnTriggerEnter(Collider _other) {
			OnTriggerStay(_other);

			object[] _params = new object[1]{_other.gameObject};
			OnTrigger(SignalTriggers.OnTriggerEnter, _params);
			SetSignal(Signals.Type.Trigger, true, _params);

			if (_other.CompareTag("Water")) {
				SetSignal(Signals.Type.InWater, true);
				m_viewControl.EnterWater( _other, m_pilot.impulse );
				m_viewControl.StartSwimming();
			} else if (_other.CompareTag("Space")) {
				m_viewControl.FlyToSpace();
			}
		}

		void OnTriggerExit(Collider _other) {
			OnTriggerStay(_other);

			SetSignal(Signals.Type.Trigger, false);
			object[] _params = new object[1]{_other.gameObject};
			OnTrigger(SignalTriggers.OnTriggerExit, _params);

			if (_other.CompareTag("Water")) {
				SetSignal(Signals.Type.InWater, false);
				m_viewControl.ExitWater( _other, m_pilot.impulse );
				m_viewControl.StopSwimming();	
			} else if (_other.CompareTag("Space")) {
				m_viewControl.ReturnFromSpace();
			}
		}

		void OnTriggerStay(Collider _other) {
			if (m_motion != null && m_affectedByDragonTrample) {
				// lets check if dragon is trampling this entity
				if (!GetSignal(Signals.Type.FallDown) && 
					!GetSignal(Signals.Type.Latched) &&
					_other.gameObject.CompareTag("Player")) {
					//----------------------------------------------------------------------------------

					// is in trample mode? - dragon has the mouth full
					DragonEatBehaviour dragonEat = InstanceManager.player.dragonEatBehaviour; 

					bool isEating	= dragonEat.IsEating();
					bool isLatching = dragonEat.IsLatching();
					bool isGrabbing = dragonEat.IsGrabbing();

					if (isEating || isLatching || isGrabbing) {
						Vector3 speed = InstanceManager.player.dragonMotion.velocity;
						m_motion.SetVelocity(speed);
						SetSignal(Signals.Type.FallDown, true);					
					}
				}
			}
		}
		//-----------------------------------------------------------

		// Update is called once per frame
		public virtual void CustomUpdate() {
			if (!IsDead()) {
				if (m_willPlaySpawnSound) {
					if (m_entity.isOnScreen) {
						PlaySound(m_onSpawnSound);
						m_willPlaySpawnSound = false;
					}
				}

				if (m_enableSensor) m_sensor.Update();
				if (m_motion != null) m_motion.Update();

				//forward special actions
				if (m_pilot != null) {
					m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.A, m_pilot.IsActionPressed(Pilot.Action.Button_A));
					m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.B, m_pilot.IsActionPressed(Pilot.Action.Button_B));
					m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.C, m_pilot.IsActionPressed(Pilot.Action.Button_C));

					m_viewControl.ShowExclamationMark(m_pilot.IsActionPressed(Pilot.Action.ExclamationMark));
				}
			}
			m_inflammable.Update();
			if (m_checkCurrents)
				CheckForCurrents();
		}

		public virtual void CustomFixedUpdate() {
			if (!IsDead()) {
				if (m_motion != null) {
					if (m_regionManager == null) {
						m_regionManager = RegionManager.Instance;
					}
					CheckFreeze();

					m_motion.externalVelocity = m_externalForces;
					m_externalForces = Vector3.zero;

					m_motion.FixedUpdate();
				}
			}
		}

		protected virtual void LateUpdate() {
			if (!IsDead()) {
				if (m_motion != null) {
					m_motion.LateUpdate();
				}
			}
		}

		public void AddExternalForce(Vector3 force) {
			m_externalForces += force;
		}

		private void CheckForCurrents() {
			// if the region manager is in place...
			if (m_regionManager != null) {
				// if it's not in a current...
				if (current == null) {
					// ... and it's visible...
					if (m_entity.isOnScreen) {
						// we're not inside a current, check for entry
						current = m_regionManager.CheckIfObjIsInCurrent(gameObject);
						if (current != null) {
							// notify the machine that it's now in a current.
							// m_machine.EnteredInCurrent(current);
						}
					}
				} else {
					// we're already inside a current, check for exit
					Vector3 pos = m_transform.position;
					//1.- Check if we are out of bounds of the current
					//2.- If we are no longer visible remove ourselves from the current as well ( most likely we have been killed by a mine which makes our renderers inactive )
					//(We dont want the camera to follow an invisible corpse)

					if (!m_entity.isOnScreen) {
						// if the object is no longer visible, remove it immediately
						if (current.splineForce != null) {
							current.splineForce.RemoveObject(gameObject, true);
						}
						current = null;
					} else if(!current.Contains(pos.x, pos.y)) {
						if (current.splineForce != null) {
							// gently apply an exit force before leaving the current
							current.splineForce.RemoveObject(gameObject, false);
						}
						current = null;
					}

					if (current == null) {
						// notify the machine that it's no more in a current.
						// m_machine.ExitedFromCurrent();
					}
				}
			}	
		}	

		public void CheckFreeze() {
			// can freeze?
			if (m_entity.circleArea != null) {
				m_freezing = FreezingObjectsRegistry.instance.Overlaps((CircleAreaBounds)m_entity.circleArea.bounds);
				if (m_freezing) {
					m_freezingMultiplier -= Time.deltaTime * FreezingObjectsRegistry.m_freezinSpeed;
				} else {
					m_freezingMultiplier += Time.deltaTime * FreezingObjectsRegistry.m_defrostSpeed;
				}
				m_freezingMultiplier = Mathf.Clamp( m_freezingMultiplier, FreezingObjectsRegistry.m_minFreezeSpeedMultiplier, 1.0f);
				m_pilot.SetFreezeFactor(m_freezingMultiplier);

				float freezingLevel = (1.0f - m_freezingMultiplier) / (1.0f - FreezingObjectsRegistry.m_minFreezeSpeedMultiplier);

				m_viewControl.Freezing(freezingLevel);
			}
		}

		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params = null) {
			m_signals.SetValue(_signal, _activated, _params);
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
			if (m_enableSensor) {
				m_sensor.Disable(_seconds);
			}
		}

		public virtual void UseGravity(bool _value) { }
		public virtual void CheckCollisions(bool _value) { }
		public virtual void FaceDirection(bool _value) { }
		public virtual bool IsFacingDirection() { return false; }

		public bool HasCorpse() {
			if (m_viewControl != null) {
				return m_viewControl.HasCorpseAsset();
			}
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
			if ( !string.IsNullOrEmpty(_clip) )
				AudioController.Play(_clip, m_transform.position);
		}

		// External interactions
		public void EnterDevice(bool _isCage) {
			m_entity.allowEdible = !_isCage;
			SetSignal(Signals.Type.LockedInCage, true);
		}

		public void LeaveDevice(bool _isCage) {
			m_entity.allowEdible = true;
			SetSignal(Signals.Type.LockedInCage, false);
		}

		public void ReceiveHit() {
			m_viewControl.Hit();
		}

		public void ReceiveDamage(float _damage) {
			if (!IsDead()) {
				m_entity.Damage(_damage);
				if (IsDead()) {
					if (m_motion != null) m_motion.Stop();
				}
			}
		}

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		public bool IsDying() {
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}

		public bool IsFreezing() {
			return m_freezing;
		}


		public virtual bool CanBeBitten() {
			if (!enabled)
				return false;
			if ( IsDead() || IsDying() )
				return false;
			if (m_isHolded)
				return false;
			if (m_pilot != null && m_pilot.IsActionPressed(Pilot.Action.Latching))
				return false;

			return true;
		}

		public void Drown() {
			SetSignal(Signals.Type.Destroyed, true);
		}

		public bool CanBeSmashed() {
			return CanBeBitten() && m_entity.CanBeSmashed();
		}

		public void Smashed() {
			SetSignal(Signals.Type.Destroyed, true);
		}

		public float biteResistance { get { return m_edible.biteResistance; } }

		public void Bite() {
			if (!IsDead()) {
				m_edible.Bite();
				m_viewControl.Bite(m_transform);
			}
		}

		public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, bool _isPlayer) {
			m_viewControl.BeginSwallowed(_transform);
			m_edible.BeingSwallowed(_transform, _rewardsPlayer, _isPlayer);
		}

		public void EndSwallowed(Transform _transform){
			m_edible.EndSwallowed(_transform);
		}

		public HoldPreyPoint[] holdPreyPoints { get{ return m_edible.holdPreyPoints; } }

		public void BiteAndHold() {
			m_isHolded = true;
			m_edible.BiteAndHold();
		}

		public void ReleaseHold() {
			m_isHolded = false;
			if ( m_motion != null )
				m_motion.position = m_transform.position;
			m_edible.ReleaseHold();

			OnReleaseHold();
		}

		protected virtual void OnReleaseHold() { }

		public void StartAttackTarget(Transform _transform) {			
			m_motion.attackTarget = _transform;
			m_viewControl.StartAttackTarget();
		}

		public void StopAttackTarget() {
			m_motion.attackTarget = null;
			m_viewControl.StopAttackTarget();
		}

		public void StartEating() {
			m_viewControl.StartEating();
		}

		public void StopEating() {
			m_viewControl.StopEating();
		}

		// Get the local rot that this thing should try to rotate towards if it is set to
		// try to align to head-first etc.
		public Quaternion GetDyingFixRot() {
			return m_edible.GetDyingFixRot();
		}

		public virtual bool Burn(Transform _transform) {
			if (m_entity.allowBurnable && m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(9999f);
					m_inflammable.Burn(_transform);
				}
				return true;
			}
			return false;
		}

		public void SetVelocity(Vector3 _v) {
			if (m_motion != null) {
				m_motion.SetVelocity(_v);
			}
		}

		// Debug
		void OnDrawGizmosSelected() {
			if (m_sensor != null) {
				if (m_transform == null) {
					m_transform = transform;
				}
				m_sensor.OnDrawGizmosSelected(m_transform);
			}
		}
	}
}