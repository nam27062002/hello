using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Machine : MonoBehaviour, IMachine, ISpawnable {		
		/**************/
		/*			  */
		/**************/

		[SeparatorAttribute]
		[SerializeField] private bool m_enableMotion = true; // TODO: find a way to dynamically add this components
		[SerializeField] private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private Range m_railSeparation = new Range(0.5f, 1f);

		[SeparatorAttribute]
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

		private Dictionary<string, Signal> m_signals;

		private Group m_group; // this will be a reference


		private MachineEdible m_edible = new MachineEdible();
		private MachineInflammable m_inflammable = new MachineInflammable();


		private bool m_willPlaySpawnSound;
		private bool m_willPlayEatenSound;

		public Vector3 position { get { if (m_motion != null) return m_motion.position; else return transform.position; } }
		public Vector3 target	{ get { return m_pilot.target; } }
		public Vector3 direction { get { if (m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Vector3 upVector  { get { if (m_motion != null) return m_motion.upVector;  else return Vector3.up; } set { if (m_motion != null) m_motion.upVector = value; } }

		public Transform enemy { 
			get {
				if (m_sensor != null && (GetSignal(Signals.Warning.name) || GetSignal(Signals.Danger.name))) {
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

			m_signals = new Dictionary<string, Signal>();

			m_signals.Add(Signals.Leader.name, 			new Signals.Leader());
			m_signals.Add(Signals.Hungry.name, 			new Signals.Hungry());
			m_signals.Add(Signals.Alert.name, 			new Signals.Alert());
			m_signals.Add(Signals.Warning.name, 		new Signals.Warning());
			m_signals.Add(Signals.Danger.name, 			new Signals.Danger());
			m_signals.Add(Signals.Panic.name, 			new Signals.Panic());
			m_signals.Add(Signals.BackToHome.name,		new Signals.BackToHome());
			m_signals.Add(Signals.Burning.name, 		new Signals.Burning());
			m_signals.Add(Signals.Chewing.name, 		new Signals.Chewing());
			m_signals.Add(Signals.Destroyed.name, 		new Signals.Destroyed());
			m_signals.Add(Signals.CollisionTrigger.name,new Signals.CollisionTrigger());

			foreach(Signal s in m_signals.Values) {
				s.machine = this;
			}
		}

		void OnEnable() {
			if ( m_signals != null )
			foreach(KeyValuePair<string, Signal> p in m_signals) {
				p.Value.Init();
			}
		}

		void OnDisable() {
			LeaveGroup();
		}

		public void Spawn(Spawner _spawner) {
			m_motion.Attach(this, m_entity, m_pilot);
			m_motion.Init();

			m_sensor.Attach(this, m_entity, m_pilot);
			m_sensor.Init();

			m_edible.Attach(this, m_entity, m_pilot);
			m_edible.Init();

			m_inflammable.Attach(this, m_entity, m_pilot);
			m_inflammable.Init();

			if (m_collider != null) m_collider.enabled = true;

			m_willPlaySpawnSound = m_onSpawnSounds.Count > 0 && Random.Range(0, 100f) < m_onSpawnSoundProbability;
			m_willPlayEatenSound = m_onEatenSounds.Count > 0 && Random.Range(0, 100f) < m_onEatenSoundProbability;
		}

		public void OnTrigger(string _trigger) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger);
			}


			switch( _trigger )
			{
				case Signals.Destroyed.OnDestroyed:
				{
					m_viewControl.Die(m_signals[Signals.Chewing.name].value);
					if (m_collider != null) m_collider.enabled = false;
					m_entity.Disable(true);
				}break;
				case Signals.Burning.OnBurning:
				{
					m_viewControl.Burn();
					if (m_collider != null) 
						m_collider.enabled = false;
				}break;
			}
		}

		// Physics Collisions and Triggers

		void OnCollisionEnter(Collision _collision) {
			OnTrigger(Signals.Collided.OnCollisionEnter);
		}


		void OnTriggerEnter(Collider _other) {
			SetSignal(Signals.CollisionTrigger.name, true);
		}

		void OnTriggerExit(Collider _other) {
			SetSignal(Signals.CollisionTrigger.name, false);
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

		public void SetSignal(string _signal, bool _activated) {
			m_signals[_signal].Set(_activated);
		}

		public bool GetSignal(string _signal) {
			return m_signals[_signal].value;
		}

		public void StickToCollisions(bool _value) {
			if (m_motion != null) {
				m_motion.stickToGround = _value;
			}
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
			if (!IsDead()) {
				m_entity.Damage(_damage);
			}
		}

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals[Signals.Destroyed.name].value;
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
			m_edible.ReleaseHold();
		}

		public virtual bool Burn(float _damage, Transform _transform) {
			if (m_inflammable != null && !IsDead()) {
				if (!GetSignal(Signals.Burning.name)) {
					ReceiveDamage(_damage);
					if (m_entity.health <= 0) {
						m_inflammable.Burn(_transform);
					}
				}
				return true;
			}
			return false;
		}
	}
}