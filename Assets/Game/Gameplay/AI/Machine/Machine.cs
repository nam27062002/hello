using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Machine : MonoBehaviour, IMachine {		
		/**************/
		/*			  */
		/**************/

		private Pilot m_pilot = null;
		private Dictionary<string, Signal> m_signals;

		private Group m_group; // this will be a reference

		private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private MachineSensor m_sensor = new MachineSensor();

		public Vector3 position { get { return transform.position; } }
		public Vector3 direction { get { if (m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Machine enemy { 
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
			m_pilot = GetComponent<Pilot>();

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
				s.pilot = m_pilot;
			}

			m_motion.AttacheMachine(this);
			m_motion.AttachPilot(m_pilot);
			m_motion.Init();

			m_sensor.AttacheMachine(this);
			m_sensor.AttachPilot(m_pilot);
			m_sensor.Init();
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
		public void Bite() {}
		public void BiteAndHold() {}
		public void Burn() {}
	}
}