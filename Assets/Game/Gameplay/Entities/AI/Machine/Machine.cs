using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Game.Currents;

namespace AI {
    public class Machine : IMachine, IAttacker {		
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_affectedByDragonTrample = false;

		[SeparatorAttribute("Components")]
		protected MC_Motion m_motion = null; // basic machine doesn't have a motion component

		[SerializeField] private bool m_enableSensor = true;
		[SerializeField] protected MachineSensor m_sensor = new MachineSensor();
		public MachineSensor sensor { get{ return m_sensor; } }

		[SerializeField] protected MachineEdible m_edible = new MachineEdible();
		[SerializeField] protected MachineInflammable m_inflammable = new MachineInflammable();

		[SeparatorAttribute("Sounds")]
		[SerializeField] private string m_onSpawnSound = "";
		// [SerializeField] private string m_onEatenSound = "";


		protected Transform m_transform;
		protected IEntity m_entity = null;
        public IEntity entity{ get{ return m_entity; } }
		protected Pilot m_pilot = null;
		protected ViewControl m_viewControl = null;
		public ViewControl view { get { return m_viewControl; } }
		protected Collider m_collider = null;

		private Signals m_signals;
        private int m_allowEdible;
        private bool allowEdible { 
                        get { return (m_allowEdible == 0 || IsStunned() || IsInLove() || IsBubbled()) && !GetSignal(Signals.Type.LockedInCage); } 
                        set { if (value) { m_allowEdible = Mathf.Max(0, m_allowEdible - 1); } else { m_allowEdible++; } } 
                    }

        private int m_allowBurnable;
        private bool allowBurnable { 
                        get { return m_allowBurnable == 0 || IsStunned() || IsInLove() || IsBubbled(); } 
                        set { if (value) { m_allowBurnable = Mathf.Max(0, m_allowBurnable - 1); } else { m_allowBurnable++; } } 
                    }


        private Group m_group; // this will be a reference

		public MachineEdible.RotateToMouthType rotateToMouth {
			get { return m_edible.rotateToMouth; }
			set { m_edible.rotateToMouth = value; }
		}

		private bool m_willPlaySpawnSound;

		override public Quaternion orientation 	{ get { return m_transform.rotation; } set { m_transform.rotation = value; } }
		override public Vector3 position		{ get { return m_transform.position; } set { m_transform.position = value; } }
		override public Vector3 direction 		{ get { return Vector3.zero; } }
		override public Vector3 groundDirection	{ get { return Vector3.right; } }
		override public Vector3 upVector 		{ get { return Vector3.up; } set {} }
		override public Vector3 velocity		{ get { return Vector3.zero; } }
		override public Vector3 angularVelocity	{ get { return Vector3.zero; } }
		override public float lastFallDistance 	{ get { return 0f; } }
		public virtual bool isKinematic 		{ get { return false; } set { } }

		override public Vector3 eye						{ get { if (m_enableSensor) return m_sensor.sensorPosition; else return m_transform.position; } }
		override public Vector3 target					{ get { return m_pilot.target; } }


		override public Transform enemy { 
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

		private Vector3		m_externalForces;	// Mostly for currents

		protected float m_stunned = 0;
        protected float m_inLove = 0;
        protected bool m_bubbled = false;

        private object[] m_collisionParams;
		private object[] m_triggerParams;

		// Activating
		UnityEngine.Events.UnityAction m_deactivateCallback;
		//---------------------------------------------------------------------------------



		// Use this for initialization
		protected virtual void Awake() {
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

			m_signals.SetOnEnableTrigger(Signals.Type.Leader, SignalTriggers.onLeaderPromoted);
			m_signals.SetOnDisableTrigger(Signals.Type.Leader, SignalTriggers.onLeaderDemoted);

			m_signals.SetOnEnableTrigger(Signals.Type.Hungry, SignalTriggers.onIsHungry);	
			m_signals.SetOnDisableTrigger(Signals.Type.Hungry, SignalTriggers.onNotHungry);

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

			m_signals.SetOnEnableTrigger(Signals.Type.Chewing, SignalTriggers.onChewing);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.onDestroyed);

			m_signals.SetOnEnableTrigger(Signals.Type.FallDown, SignalTriggers.onFallDown);
			m_signals.SetOnDisableTrigger(Signals.Type.FallDown, SignalTriggers.OnGround);

			m_signals.SetOnEnableTrigger(Signals.Type.LockedInCage, SignalTriggers.onLockedInCage);
			m_signals.SetOnDisableTrigger(Signals.Type.LockedInCage, SignalTriggers.onUnlockedFromCage);

			m_signals.SetOnEnableTrigger(Signals.Type.Invulnerable, SignalTriggers.onInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.Invulnerable, SignalTriggers.onVulnerable);

			m_signals.SetOnEnableTrigger(Signals.Type.InvulnerableBite, SignalTriggers.onInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.InvulnerableBite, SignalTriggers.onVulnerable);

			m_signals.SetOnEnableTrigger(Signals.Type.InvulnerableFire, SignalTriggers.onInvulnerable);
			m_signals.SetOnDisableTrigger(Signals.Type.InvulnerableFire, SignalTriggers.onVulnerable);

            m_signals.SetOnEnableTrigger(Signals.Type.InWater, SignalTriggers.onWaterEnter);
            m_signals.SetOnDisableTrigger(Signals.Type.InWater, SignalTriggers.onWaterExit);

            m_collisionParams = new object[1];
			m_triggerParams = new object[1];

			m_externalForces = Vector3.zero;
		}

		void OnDisable() {
			LeaveGroup();
		}

		override public void Spawn(ISpawner _spawner) {
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

            m_allowEdible = 0;
            m_allowBurnable = 0;

            if (m_collider != null)
				m_collider.enabled = true;

			m_willPlaySpawnSound = !string.IsNullOrEmpty(m_onSpawnSound);
		}

		override public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {
			gameObject.SetActive(false);
			m_deactivateCallback = _action;
			Invoke("Activate", duration);
		}

		override public void Activate() {
			gameObject.SetActive(true);
			if (m_deactivateCallback != null)
				m_deactivateCallback();
		}

		override public void OnTrigger(int _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger, _param);
			}

			if (_trigger == SignalTriggers.onDestroyed) {
				m_viewControl.Die(m_signals.GetValue(Signals.Type.Chewing), m_signals.GetValue(Signals.Type.Burning));
				if (m_motion != null) m_motion.Stop();
				if (m_collider != null) m_collider.enabled = false;
				m_entity.Disable(true);
			} else if (_trigger == SignalTriggers.onBurning) {
				m_viewControl.Burn(m_inflammable.burningTime, m_inflammable.burnedColor);
				if (m_motion != null) m_motion.Stop();
				if (m_collider != null) m_collider.enabled = false;
			} else if (_trigger == SignalTriggers.onInvulnerable || _trigger == SignalTriggers.onVulnerable) {
				allowEdible = !(m_signals.GetValue(Signals.Type.Invulnerable) || m_signals.GetValue(Signals.Type.InvulnerableBite));
				allowBurnable = !(m_signals.GetValue(Signals.Type.Invulnerable) || m_signals.GetValue(Signals.Type.InvulnerableFire));
			}
		}

		//-----------------------------------------------------------
		// Physics Collisions and Triggers
		protected virtual void OnCollisionEnter(Collision _collision) {
			m_collisionParams[0] = _collision;
			OnTrigger(SignalTriggers.onCollisionEnter, m_collisionParams);
			SetSignal(Signals.Type.Collision, true, ref m_collisionParams);

			if (m_motion != null) {
                if (((1 << _collision.collider.gameObject.layer) & GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) != 0) {
					m_motion.OnCollisionGroundEnter(_collision);
				}
			}
		}

		protected virtual void OnCollisionStay(Collision _collision) {
			if (m_motion != null) {
				if (((1 << _collision.collider.gameObject.layer) & GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) != 0) {
					m_motion.OnCollisionGroundStay(_collision);
				}
			}
		}

		protected virtual void OnCollisionExit(Collision _collision) {
			if (m_motion != null) {
				if (((1 << _collision.collider.gameObject.layer) & GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) != 0) {
					m_motion.OnCollisionGroundExit(_collision);
				}
			}

			SetSignal(Signals.Type.Collision, false);
		}

		protected virtual void OnTriggerEnter(Collider _other) {
			OnTriggerStay(_other);

			m_triggerParams[0] = _other.gameObject;
			OnTrigger(SignalTriggers.onTriggerEnter, m_triggerParams);
			SetSignal(Signals.Type.Trigger, true, ref m_triggerParams);

			if (_other.CompareTag("Water")) {
				SetSignal(Signals.Type.InWater, true);
				m_viewControl.EnterWater( _other, m_pilot.impulse );
				m_viewControl.StartSwimming();
			} else if (_other.CompareTag("Space")) {
				m_viewControl.FlyToSpace();
			}
		}

		protected virtual void OnTriggerExit(Collider _other) {
			OnTriggerStay(_other);

			SetSignal(Signals.Type.Trigger, false);
            m_triggerParams[0] = _other.gameObject;
			OnTrigger(SignalTriggers.onTriggerExit, m_triggerParams);

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
		override public void CustomUpdate() {            
            if (!IsDead()) {
                CheckStun();
                CheckInLove();
                
                if (m_stunned <= 0 && !m_bubbled) {
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
                        m_viewControl.Scared(m_pilot.IsActionPressed(Pilot.Action.Scared));

                        m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.A, m_pilot.IsActionPressed(Pilot.Action.Button_A));
                        m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.B, m_pilot.IsActionPressed(Pilot.Action.Button_B));
                        m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.C, m_pilot.IsActionPressed(Pilot.Action.Button_C));

                        m_viewControl.ShowExclamationMark(m_pilot.IsActionPressed(Pilot.Action.ExclamationMark));
                    }
                }
            }
            m_inflammable.Update();
        }

		override public void CustomFixedUpdate() {
            if (!IsDead()) {
				if (m_motion != null) {

					m_motion.externalVelocity = m_externalForces;
					m_externalForces = Vector3.zero;

                    if (m_stunned <= 0 && !m_bubbled) {
                        m_motion.FixedUpdate();
                    }
				}
			}
        }

		public override void CustomLateUpdate() {
            if (!IsDead()) {
				if (m_motion != null) {
					m_motion.LateUpdate();
				}
			}
        }

		override public void AddExternalForce(Vector3 force) {
			m_externalForces += force;
		}

		public void CheckStun() {
			if (m_stunned > 0) {
				m_stunned -= Time.deltaTime;
                if ( m_pilot != null )
				    m_pilot.SetStunned(m_stunned > 0);
				m_viewControl.SetStunned(m_stunned > 0);
			}
		}

		public void Stun(float _stunTime) {
			m_stunned = Mathf.Max( _stunTime, m_stunned);
            if (m_stunned > 0) {
                if (m_pilot != null)  m_pilot.Stop();
                if (m_motion != null) m_motion.Stop();
            }
		}

        public void Bubbled(bool _active) {
            m_bubbled = _active;
            if (m_bubbled) {
                if (m_pilot != null) m_pilot.Stop();
                if (m_motion != null) m_motion.Stop();
            }
            m_viewControl.SetBubbled(m_bubbled);
        }

        public virtual void CheckInLove() {
            if (m_inLove > 0) {
                m_inLove -= Time.deltaTime;
                if (m_inLove <= 0) {
                    SetSignal(Signals.Type.InLove, false);
                    m_viewControl.SetInLove(false);
                }
            }
        }
        
        public virtual void InLove( float _inLoveDuration ) {
            m_inLove = Mathf.Max( _inLoveDuration, m_inLove);
            if (m_inLove > 0) {
                if (m_pilot != null) m_pilot.Stop();
                SetSignal(Signals.Type.InLove, true);

                SetSignal(Signals.Type.Invulnerable, false);
                SetSignal(Signals.Type.InvulnerableBite, false);
                SetSignal(Signals.Type.InvulnerableFire, false);

                m_viewControl.SetInLove(true);
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
			if (m_enableSensor) {
				m_sensor.Disable(_seconds);
			}
		}

		override public void UseGravity(bool _value) { }
		override public void CheckCollisions(bool _value) { }
		override public void FaceDirection(bool _value) { }
		override public bool IsFacingDirection() { return false; }
		override public bool IsInFreeFall() { 
			if (m_motion != null) {
				return m_motion.IsInFreeFall();
			} else {
				return false; 
			}
		}

		override public bool HasCorpse() {
			if (m_viewControl != null) {
				return m_viewControl.HasCorpseAsset();
			}
			return false;
		}

		// Group membership -> for collective behaviours
		override public void	EnterGroup(ref Group _group) {
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
			if ( !string.IsNullOrEmpty(_clip) )
				AudioController.Play(_clip, m_transform.position);
		}

		// External interactions
		override public void EnterDevice(bool _isCage) {
			allowEdible = !_isCage;
			SetSignal(Signals.Type.LockedInCage, true);
		}

		override public void LeaveDevice(bool _isCage) {
			allowEdible = true;
			SetSignal(Signals.Type.LockedInCage, false);
		}

		public void ReceiveHit() {
			m_viewControl.Hit();
		}

		override public void ReceiveDamage(float _damage) {
			if (!IsDead()) {
				m_entity.Damage(_damage);
				if (IsDead()) {
					if (m_motion != null) m_motion.Stop();
				}
			}
		}

		override public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		override public bool IsDying() {
			return GetSignal(AI.Signals.Type.Chewing) || GetSignal(AI.Signals.Type.Burning);
		}

        override public bool IsStunned() {
            return m_stunned > 0;
        }

        override public bool IsInLove() {
            return m_inLove > 0;
        }

        override public bool IsBubbled() {
            return m_bubbled;
        }

		override public bool CanBeBitten() {
			if (!enabled)
				return false;
			if ( IsDead() || IsDying() )
				return false;
            if (!allowEdible)
                return false;
			if (m_isHolded)
				return false;
			if (m_pilot != null && m_pilot.IsActionPressed(Pilot.Action.Latching))
				return false;

			return true;
		}

		override public void Drown() {
			SetSignal(Signals.Type.Destroyed, true);
		}

        override public bool Smash( IEntity.Type _source ) {
			if ( !IsDead() && !IsDying() && allowEdible)
			{
                if (m_bubbled) {
                    BubbledEntitySystem.RemoveEntity(m_entity);
                }

				SetSignal(Signals.Type.Destroyed, true);
				if ( !m_viewControl.HasCorpseAsset() )
					m_viewControl.SpawnEatenParticlesAt( m_transform );

				m_entity.onDieStatus.source = _source;
				m_entity.onDieStatus.reason = IEntity.DyingReason.DESTROYED;
				
                Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.DESTROYED);
				Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, m_transform, m_entity, reward);

                if ( _source == IEntity.Type.PLAYER )
                    InstanceManager.timeScaleController.HitStop();
				
                return true;
			}

			return false;
		}

		override public float biteResistance { get { return m_edible.biteResistance; } }

		override public void Bite() {
			if (!IsDead() && allowEdible) {
                m_edible.Bite();
				m_viewControl.Bite(m_transform);
			}
		}

		override public void BeginSwallowed(Transform _transform, bool _rewardsPlayer, IEntity.Type _source) {
            if (allowEdible) {
                m_viewControl.BeginSwallowed(_transform);
                m_edible.BeingSwallowed(_transform, _rewardsPlayer, _source);
            }
		}

		override public void EndSwallowed(Transform _transform){
			m_edible.EndSwallowed(_transform);
		}

		override public HoldPreyPoint[] holdPreyPoints { get{ return m_edible.holdPreyPoints; } }

		override public void BiteAndHold() {
            if (allowEdible) {
                m_isHolded = true;
                m_edible.BiteAndHold();
            }
		}

		override public void ReleaseHold() {
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
		override public Quaternion GetDyingFixRot() {
			return m_edible.GetDyingFixRot();
		}

		override public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			if (allowBurnable && m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Type.Burning)) {
					ReceiveDamage(9999f);
					m_inflammable.Burn(_transform, _source, instant, fireColorType);
				}
				return true;
			}
			return false;
		}

		override public void SetVelocity(Vector3 _v) {
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