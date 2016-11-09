using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Game.Currents;

namespace AI {
	public class Machine : MonoBehaviour, IMachine, ISpawnable, MotionInterface {	
		protected static int m_groundMask;

		public enum RotateToMouthType
		{
			No,										// does not have any special rotation applied while being eaten
			TailFirst,								// rotates to be swallowed tail first - use with swallow shader
			HeadFirst,								// rotates to be swallowed head first - "
			ClosestFirst,							// rotates to swallow either head or tail first, whichever is closer - "
			Sideways,								// turns sideways - use when breaking into corpse chunks with centre chunk staying in mouth
		};

		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] protected MachineMotion m_motion = new MachineMotion();
		[SerializeField] private bool m_affectedByDragonTrample = false;

		[SerializeField] private bool m_enableSensor = true;
		[SerializeField] private MachineSensor m_sensor = new MachineSensor();

		[SeparatorAttribute("Sounds")]
		[SerializeField][Range(0f, 100f)] private float m_onSpawnSoundProbability = 40.0f;
		[SerializeField] private string m_onSpawnSound = "";

		[SerializeField][Range(0f, 100f)] private float m_onEatenSoundProbability = 50.0f;
		[SerializeField] private string m_onEatenSound = "";

		[SeparatorAttribute("Other")]
		[SerializeField] private RotateToMouthType m_rotateToMouth;
		public RotateToMouthType rotateToMouth{
			get{ return m_rotateToMouth; }
			set{ m_rotateToMouth = value; }
		}


		protected IEntity m_entity = null;
		protected Pilot m_pilot = null;
		protected ViewControl m_viewControl = null;
		protected Collider m_collider = null;

		private Signals m_signals;

		private Group m_group; // this will be a reference


		private MachineEdible m_edible = new MachineEdible();
		[SerializeField] private MachineInflammable m_inflammable = new MachineInflammable();


		private bool m_willPlaySpawnSound;
		private bool m_willPlayEatenSound;

		public Vector3 position { 	get { if (m_enableMotion && m_motion != null) return m_motion.position; else return transform.position; } 
									set { if (m_enableMotion && m_motion != null) m_motion.position = value; else transform.position = value; } }

		public Vector3 eye				{ get { if (m_enableSensor && m_sensor != null) return m_sensor.sensorPosition; else return transform.position; } }
		public Vector3 target			{ get { return m_pilot.target; } }
		public Vector3 direction 		{ get { if (m_enableMotion && m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Vector3 groundDirection	{ get { if (m_enableMotion && m_motion != null) return m_motion.groundDirection; else return Vector3.zero; } }
		public Vector3 upVector 		{ get { if (m_enableMotion && m_motion != null) return m_motion.upVector;  else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }
		public Vector3 velocity			{ get { if (m_enableMotion && m_motion != null) return m_motion.velocity; else return Vector3.zero;} }
		public Vector3 angularVelocity	{ get { if (m_enableMotion && m_motion != null) return m_motion.angularVelocity; else return Vector3.zero;} }

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
		public bool isPetTarget{ get{ return m_isPetTarget;} set { m_isPetTarget = value; } }


		// Currents
		private RegionManager 	m_regionManager;
		public Current			current { get; set; }
		private Vector3		m_externalForces;	// Mostly for currents

		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "Obstacle", "PreyOnlyCollisions");

			m_entity = GetComponent<IEntity>();
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControl>();
			m_collider = GetComponent<Collider>();

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

			m_externalForces = Vector3.zero;
		}

		void OnEnable() {
			if (m_signals!= null) 
				m_signals.Init();
		}

		void OnDisable() {
			LeaveGroup();
		}

		public virtual void Spawn(ISpawner _spawner) {			
			m_motion.Init();
			m_sensor.Init();
			m_edible.Init();
			m_inflammable.Init();

			if (m_collider != null) m_collider.enabled = true;

			m_willPlaySpawnSound = !string.IsNullOrEmpty(m_onSpawnSound) && Random.Range(0, 100f) < m_onSpawnSoundProbability;
			m_willPlayEatenSound = !string.IsNullOrEmpty(m_onEatenSound) && Random.Range(0, 100f) < m_onEatenSoundProbability;
		}

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger, _param);
			}

			if (_trigger == SignalTriggers.OnDestroyed) {
					m_viewControl.Die(m_signals.GetValue(Signals.Type.Chewing));
					if (m_motion != null) m_motion.Stop();
					if (m_collider != null) m_collider.enabled = false;
					m_entity.Disable(true);
			} else if (_trigger == SignalTriggers.OnBurning) {
					m_viewControl.Burn();
					if (m_motion != null) m_motion.Stop();
					if (m_collider != null) m_collider.enabled = false;
			} else if (_trigger == SignalTriggers.OnInvulnerable) {
				m_entity.allowEdible = false;
				m_entity.allowBurnable = false;
			} else if (_trigger == SignalTriggers.OnVulnerable) {
				m_entity.allowEdible = true;
				m_entity.allowBurnable = true;
			}

		}

		//-----------------------------------------------------------
		// Physics Collisions and Triggers
		void OnCollisionEnter(Collision _collision) {
			object[] _params = new object[1]{_collision};
			OnTrigger(SignalTriggers.OnCollisionEnter, _params);
			SetSignal(Signals.Type.Collision, true, _params);

			if (m_motion != null) {
				if (((1 << _collision.gameObject.layer) & m_groundMask) != 0) {
					m_motion.OnCollisionGroundEnter();
				}
			}
		}

		void OnCollisionExit(Collision _collision) {
			if (m_motion != null) {
				if (((1 << _collision.gameObject.layer) & m_groundMask) != 0) {
					m_motion.OnCollisionGroundExit();
				}
			}

			SetSignal(Signals.Type.Collision, false);
		}

		void OnTriggerEnter(Collider _other) {
			OnTriggerStay(_other);

			object[] _params = new object[1]{_other.gameObject};
			OnTrigger(SignalTriggers.OnTriggerEnter, _params);
			SetSignal(Signals.Type.Trigger, true, _params);


			if ( _other.tag == "Water" )
			{
				m_viewControl.EnterWater( _other, m_pilot.impulse );
				m_viewControl.StartSwimming();	
			}
			else if (_other.tag == "Space" )
			{
				m_viewControl.FlyToSpace();
			}
		}

		void OnTriggerExit(Collider _other) {
			OnTriggerStay(_other);

			SetSignal(Signals.Type.Trigger, false);
			object[] _params = new object[1]{_other.gameObject};
			OnTrigger(SignalTriggers.OnTriggerExit, _params);

			if ( _other.tag == "Water" )
			{
				m_viewControl.ExitWater( _other, m_pilot.impulse );
				m_viewControl.StopSwimming();	
			}
			else if (_other.tag == "Space" )
			{
				m_viewControl.ReturnFromSpace();
			}
		}

		void OnTriggerStay(Collider _other) {
			if (m_affectedByDragonTrample) {
				// lets check if dragon is trampling this entity
				if (!GetSignal(Signals.Type.FallDown) && _other.gameObject.tag == "Player") {
					// is in trample mode? - dragon has the mouth full
					DragonEatBehaviour dragonEat = InstanceManager.player.dragonEatBehaviour; 

					bool isEating	= dragonEat.IsEating();
					bool isLatching = dragonEat.IsLatching();
					bool isGrabbing = dragonEat.IsGrabbing();

					if (isEating || isLatching || isGrabbing) {
						Vector3 speed = InstanceManager.player.dragonMotion.velocity;
						m_motion.SetVelocity(speed * 10f);
						SetSignal(Signals.Type.FallDown, true);
					}
				}
			}
		}
		//-----------------------------------------------------------

		// Update is called once per frame
		protected virtual void Update() {
			if (!IsDead()) {
				if (m_willPlaySpawnSound) {
					if (m_entity.isOnScreen) {
						PlaySound(m_onSpawnSound);
						m_willPlaySpawnSound = false;
					}
				}

				if (m_enableMotion) {
					if(m_regionManager == null)
					{
						m_regionManager = RegionManager.Instance;
					}
					CheckForCurrents ();
					m_motion.externalVelocity = m_externalForces;
					m_motion.Update();
					m_externalForces = Vector3.zero;
				}
				if (m_enableSensor) m_sensor.Update();

				//forward special actions
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.A, m_pilot.IsActionPressed(Pilot.Action.Button_A));
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.B, m_pilot.IsActionPressed(Pilot.Action.Button_B));
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.C, m_pilot.IsActionPressed(Pilot.Action.Button_C));
			}
			m_inflammable.Update();
		}

		public void AddExternalForce( Vector3 force )
		{
			m_externalForces += force;
		}

		private void CheckForCurrents()
	    {
			// if the region manager is in place...
	        if(m_regionManager != null)
	        {
				// if it's not in a current...
				if(current == null)
	            {
					// ... and it's visible...
					if(m_entity.isOnScreen)
					{
						// we're not inside a current, check for entry
						current = m_regionManager.CheckIfObjIsInCurrent(gameObject);
						if(current != null)
						{
							// notify the machine that it's now in a current.
							// m_machine.EnteredInCurrent(current);
						}
					}
	            }
	            else
	            {
	                // we're already inside a current, check for exit
	                Vector3 pos = transform.position;
					//1.- Check if we are out of bounds of the current
					//2.- If we are no longer visible remove ourselves from the current as well ( most likely we have been killed by a mine which makes our renderers inactive )
					//(We dont want the camera to follow an invisible corpse)

					if(!m_entity.isOnScreen)
					{
						// if the object is no longer visible, remove it immediately
						current.splineForce.RemoveObject(gameObject, true);
						current = null;
					}
					else if(!current.Contains(pos.x, pos.y))
	                {
						if(current.splineForce != null)
						{
							// gently apply an exit force before leaving the current
							current.splineForce.RemoveObject(gameObject, false);
						}
						current = null;
	                }

					if(current == null )
					{
						// notify the machine that it's no more in a current.
						// m_machine.ExitedFromCurrent();
					}
				}
	        }	
		}	

		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params = null) {
			m_signals.SetValue(_signal, _activated, _params);
		}

		public bool GetSignal(Signals.Type _signal) {
			return m_signals.GetValue(_signal);
		}

		public object[] GetSignalParams(Signals.Type _signal) {
			return m_signals.GetParams(_signal);
		}

		public void UseGravity(bool _value) {
			if (m_motion != null) {
				m_motion.useGravity = _value;
			}
		}

		public void CheckCollisions(bool _value) {
			if (m_motion != null) {
				m_motion.checkCollisions = _value;
			}
		}

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
		public void LockInCage() {
			m_entity.allowEdible = false;
			SetSignal(Signals.Type.LockedInCage, true);
		}

		public void UnlockFromCage() {
			m_entity.allowEdible = true;
			SetSignal(Signals.Type.LockedInCage, false);
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

		public bool IsDying()
		{
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}


		public bool CanBeBitten()
		{
			if (!enabled)
				return false;
			if ( IsDead() || IsDying() )
				return false;
			if (m_isHolded)
				return false;
			if ( m_pilot.IsActionPressed(Pilot.Action.Latching) )
				return false;

			return true;
		}


		public float biteResistance { get { return m_edible.biteResistance; }}

		public void Bite() {
			if (m_edible != null && !IsDead()) {
				m_edible.Bite();
			}
		}

		public void BeingSwallowed(Transform _transform, bool _rewardsPlayer) {			
			if (m_willPlayEatenSound) {
				if (m_entity.isOnScreen) {
					PlaySound(m_onEatenSound);
					m_willPlayEatenSound = false;
				}
			}

			m_edible.BeingSwallowed(_transform, _rewardsPlayer);
		}

		public HoldPreyPoint[] holdPreyPoints { get{ return m_edible.holdPreyPoints; } }

		public void BiteAndHold() {
			m_isHolded = true;
			m_edible.BiteAndHold();
		}

		public void ReleaseHold() {
			m_isHolded = false;
			m_motion.position = transform.position;
			m_edible.ReleaseHold();
		}


		public void StartAttackTarget(Transform _transform)
		{
			m_motion.attackTarget = _transform;
			m_viewControl.StartAttackTarget();
		}

		public void StopAttackTarget()
		{
			m_motion.attackTarget = null;
			m_viewControl.StopAttackTarget();
		}

		public void StartEating()
		{
			m_viewControl.StartEating();
		}

		public void StopEating()
		{
			m_viewControl.StopEating();
		}

		// Get the local rot that this thing should try to rotate towards if it is set to
		// try to align to head-first etc.
		public virtual Quaternion GetDyingFixRot()
		{
			Quaternion result = Quaternion.identity;

			if(m_rotateToMouth == RotateToMouthType.No)
				return result;

			if(m_rotateToMouth == RotateToMouthType.Sideways)
			{
				float diff = Quaternion.Angle(transform.localRotation, Quaternion.AngleAxis(90.0f, Vector3.up));
				float rot = (diff > 90.0f) ? 270.0f : 90.0f;
				result = Quaternion.AngleAxis(rot, Vector3.up);
			}
			else
			{
				bool headFirst = (m_rotateToMouth == RotateToMouthType.HeadFirst);
				if(m_rotateToMouth == RotateToMouthType.ClosestFirst)
				{
					Transform trParent = transform.parent;	// null check on this to handle case of attacker being deleted or something
					if((trParent != null) &&  trParent.InverseTransformDirection(transform.right).x < 0.0f)
						headFirst = true;
				}
				result = headFirst ? Quaternion.AngleAxis(180.0f, Vector3.up) : Quaternion.identity;
			}

			result = result * Quaternion.AngleAxis(90f, Vector3.forward);

			return result;
		}


		public virtual bool Burn(float _damage, Transform _transform) {
			if (m_entity.allowBurnable && m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(_damage);
					if (m_entity.health <= 0) {
						m_inflammable.Burn(_transform);
					}
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
				m_sensor.OnDrawGizmosSelected(transform);
			}
		}
	}
}