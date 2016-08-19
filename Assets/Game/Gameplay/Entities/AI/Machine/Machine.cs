using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Machine : MonoBehaviour, IMachine, ISpawnable {		
		/**************/
		/*			  */
		/**************/
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private Range m_railSeparation = new Range(0.5f, 1f);

		[SerializeField] private bool m_enableSensor = true;
		[SerializeField] private MachineSensor m_sensor = new MachineSensor();

		[SeparatorAttribute("Sounds")]
		[SerializeField][Range(0f, 100f)] private float m_onSpawnSoundProbability = 40.0f;
		[SerializeField] private List<string> m_onSpawnSounds = new List<string>();

		[SerializeField][Range(0f, 100f)] private float m_onEatenSoundProbability = 50.0f;
		[SerializeField] private List<string> m_onEatenSounds = new List<string>();


		private Entity m_entity = null;
		private Pilot m_pilot = null;
		private ViewControl m_viewControl = null;
		private Collider m_collider = null;

		private Signals m_signals;

		private Group m_group; // this will be a reference


		private MachineEdible m_edible = new MachineEdible();
		private MachineInflammable m_inflammable = new MachineInflammable();


		private bool m_willPlaySpawnSound;
		private bool m_willPlayEatenSound;

		public Vector3 position { 	get { if (m_enableMotion && m_motion != null) return m_motion.position; else return transform.position; } 
									set { if (m_enableMotion && m_motion != null) m_motion.position = value; else transform.position = value; } 
								}

		public Vector3 target	 { 	get { return m_pilot.target; } }
		public Vector3 direction { 	get { if (m_enableMotion && m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Vector3 upVector  { 	get { if (m_enableMotion && m_motion != null) return m_motion.upVector;  else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }

		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Type.Warning) || GetSignal(Signals.Type.Danger))) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}

		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_entity = GetComponent<Entity>();
			m_pilot = GetComponent<Pilot>();
			m_viewControl = GetComponent<ViewControl>();
			m_collider = GetComponent<Collider>();

			m_motion.Attach(this, m_entity, m_pilot);
			m_sensor.Attach(this, m_entity, m_pilot);
			m_edible.Attach(this, m_entity, m_pilot);
			m_inflammable.Attach(this, m_entity, m_pilot);

			m_signals = new Signals(this);

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

			m_signals.SetOnEnableTrigger(Signals.Type.Panic, SignalTriggers.OnPanic);
			m_signals.SetOnDisableTrigger(Signals.Type.Panic, SignalTriggers.OnRecoverFromPanic);

			m_signals.SetOnEnableTrigger(Signals.Type.BackToHome, SignalTriggers.OnOutsideArea);
			m_signals.SetOnDisableTrigger(Signals.Type.BackToHome, SignalTriggers.OnBackAtHome);

			m_signals.SetOnEnableTrigger(Signals.Type.Burning, SignalTriggers.OnBurning);

			m_signals.SetOnEnableTrigger(Signals.Type.Chewing, SignalTriggers.OnChewing);

			m_signals.SetOnEnableTrigger(Signals.Type.Destroyed, SignalTriggers.OnDestroyed);

			m_signals.SetOnEnableTrigger(Signals.Type.FallDown, SignalTriggers.OnFallDown);
			m_signals.SetOnDisableTrigger(Signals.Type.FallDown, SignalTriggers.OnGround);
		}

		void OnEnable() {
			if (m_signals != null)
				m_signals.Init();
		}

		void OnDisable() {
			LeaveGroup();
		}

		public void Spawn(ISpawner _spawner) {
			m_motion.Init();
			m_sensor.Init();
			m_edible.Init();
			m_inflammable.Init();

			if (m_collider != null) m_collider.enabled = true;

			m_willPlaySpawnSound = m_onSpawnSounds.Count > 0 && Random.Range(0, 100f) < m_onSpawnSoundProbability;
			m_willPlayEatenSound = m_onEatenSounds.Count > 0 && Random.Range(0, 100f) < m_onEatenSoundProbability;
		}

		public void OnTrigger(string _trigger, object[] _param = null) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger, _param);
			}

			if (_trigger == SignalTriggers.OnDestroyed) {
					m_viewControl.Die(m_signals.GetValue(Signals.Type.Chewing));
					if (m_collider != null) m_collider.enabled = false;
					m_entity.Disable(true);
			} else if (_trigger == SignalTriggers.OnBurning) {
					m_viewControl.Burn();
					if (m_collider != null) m_collider.enabled = false;
			}
		}

		// Physics Collisions and Triggers
		void OnCollisionEnter(Collision _collision) {
			object[] _params = new object[1]{_collision.gameObject};
			OnTrigger(SignalTriggers.OnCollisionEnter, _params);
		}


		void OnTriggerEnter(Collider _other) {
			object[] _params = new object[1]{_other.gameObject};
			OnTrigger(SignalTriggers.OnTriggerEnter, _params);
			SetSignal(Signals.Type.Trigger, true);
		}

		void OnTriggerExit(Collider _other) {
			SetSignal(Signals.Type.Trigger, false);
		}
		//

		// Update is called once per frame
		void Update() {
			if (!IsDead()) {
				if (m_willPlaySpawnSound) {
					if (m_entity.isOnScreen) {
						PlaySound(m_onSpawnSounds[Random.Range(0, m_onSpawnSounds.Count)]);
						m_willPlaySpawnSound = false;
					}
				}

				if (m_enableMotion) m_motion.Update();
				if (m_enableSensor) m_sensor.Update();

				//forward special actions
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.A, m_pilot.IsActionPressed(Pilot.Action.Button_A));
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.B, m_pilot.IsActionPressed(Pilot.Action.Button_B));
				m_viewControl.SpecialAnimation(ViewControl.SpecialAnims.C, m_pilot.IsActionPressed(Pilot.Action.Button_C));
			}
			m_inflammable.Update();
		}

		public void SetSignal(Signals.Type _signal, bool _activated) {
			m_signals.SetValue(_signal, _activated);
		}

		public bool GetSignal(Signals.Type _signal) {
			return m_signals.GetValue(_signal);
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

		public void SetRail(uint _rail, uint _total) {
			if (m_motion != null) {
				if (_total > 1) {
					float railSeparation = m_railSeparation.GetRandom();
					m_motion.zOffset = (_rail * railSeparation) - (railSeparation * (_total / 2));
				} else {
					m_motion.zOffset = 0f;
				}
			}
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
			AudioManager.instance.PlayClip(_clip);
		}

		// External interactions
		public void ReceiveDamage(float _damage) {
			_damage = 0f;
			if (!IsDead()) {
				m_entity.Damage(_damage);
			}
		}

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals.GetValue(Signals.Type.Destroyed);
		}

		public float biteResistance { get { return m_edible.biteResistance; }}

		public void Bite() {
			if (m_edible != null && !IsDead()) {
				m_edible.Bite();
			}
		}

		public void BeingSwallowed(Transform _transform) {			
			if (m_willPlayEatenSound) {
				if (m_entity.isOnScreen) {
					PlaySound(m_onEatenSounds[Random.Range(0, m_onEatenSounds.Count)]);
					m_willPlayEatenSound = false;
				}
			}

			m_edible.BeingSwallowed(_transform);
		}

		public List<Transform> holdPreyPoints { get{ return m_edible.holdPreyPoints; } }

		public void BiteAndHold() {
			m_edible.BiteAndHold();
		}

		public void ReleaseHold() {
			m_motion.position = transform.position;
			m_edible.ReleaseHold();
		}

		public virtual bool Burn(float _damage, Transform _transform) {
			if (m_inflammable != null && !IsDead()) {
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