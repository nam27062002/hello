using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Machine : MonoBehaviour, IMachine {		
		/**************/
		/*			  */
		/**************/

		private Entity m_entity = null;
		private Pilot m_pilot = null;
		private ViewControl m_viewControl;

		private Dictionary<string, Signal> m_signals;

		private Group m_group; // this will be a reference

		[SerializeField] private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private MachineSensor m_sensor = new MachineSensor();

		private MachineEdible m_edible = new MachineEdible();


		public Vector3 position { get { return transform.position; } }
		public Vector3 direction { get { if (m_motion != null) return m_motion.direction; else return Vector3.zero; } }
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

			m_signals = new Dictionary<string, Signal>();

			m_signals.Add(Signals.Leader.name, 		new Signals.Leader());
			m_signals.Add(Signals.Hungry.name, 		new Signals.Hungry());
			m_signals.Add(Signals.Alert.name, 		new Signals.Alert());
			m_signals.Add(Signals.Warning.name, 	new Signals.Warning());
			m_signals.Add(Signals.Danger.name, 		new Signals.Danger());
			m_signals.Add(Signals.Panic.name, 		new Signals.Panic());
			m_signals.Add(Signals.Burning.name, 	new Signals.Burning());
			m_signals.Add(Signals.Chewing.name, 	new Signals.Chewing());
			m_signals.Add(Signals.Destroyed.name, 	new Signals.Destroyed());

			foreach(Signal s in m_signals.Values) {
				s.machine = this;
			}
		}

		void Start() {
			m_motion.Attach(this, m_entity, m_pilot, m_viewControl);
			m_motion.Init();

			m_sensor.Attach(this, m_entity, m_pilot, m_viewControl);
			m_sensor.Init();

			m_edible.Attach(this, m_entity, m_pilot, m_viewControl);
			m_edible.Init();
		}

		public void OnTrigger(string _trigger) {
			if (m_pilot != null) {
				m_pilot.OnTrigger(_trigger);
			}

			if (Signals.Destroyed.OnDestroyed == _trigger) {
				m_pilot.enabled = false;
				if (m_group != null) m_group.Leave(this);
				GameObject.Destroy(gameObject);
			}
		}
		
		// Update is called once per frame
		void Update() {
			m_motion.Update();
			m_sensor.Update();
		}

		public void SetSignal(string _signal, bool _activated) {
			m_signals[_signal].Set(_activated);
		}

		public bool GetSignal(string _signal) {
			return m_signals[_signal].value;
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


		// External interactions
		public void ReceiveDamage(float _damage) {
			m_entity.Damage(_damage);
		}

		public bool IsDead() {
			return m_entity.health <= 0 || m_signals[Signals.Destroyed.name].value;
		}

		public float biteResistance { get { return m_edible.biteResistance; }}

		public void Bite() {
			if (m_edible != null) {
				m_edible.Bite();
			}
		}

		public void BeingSwallowed(Transform _transform) {
			if (m_edible != null) {
				m_edible.BeingSwallowed(_transform);
			}
		}

		public List<Transform> holdPreyPoints { get{ return m_edible.holdPreyPoints; } }

		public void BiteAndHold() {
			if (m_edible != null) {
				m_edible.BiteAndHold();
			}
		}

		public void ReleaseHold() {
			if (m_edible != null) {
				m_edible.ReleaseHold();
			}
		}

		public void Burn(float _damage, Transform _transform) {
			
		}
	}
}