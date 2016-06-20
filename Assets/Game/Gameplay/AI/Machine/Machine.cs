using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class Machine : MonoBehaviour, IMachine {
		/**************/
		/*			  */
		/**************/
		public enum Signal {
			Leader = 0,
			Hungry, 	// this machine is hungry and it'll search for preys using the Eater machine component
			Alert,  	// this machine will use it's sensor to detect the player. Later we can extend this to detect all king of enemies
			Warning, 	// enemy detected nearby
			Danger, 	// enemy is too close
			Panic,		// this machine is unable to perform actions
			Burning, 	// a fire is touching this machine
			Chewing,	// something is chewing this machine
			Destroyed,	// 
			Count
		};

		/**************/
		/*			  */
		/**************/

		private Pilot m_pilot = null;
		private bool[] m_signals;

		private Group m_group; // this will be a reference

		private MachineMotion m_motion = new MachineMotion();
		[SerializeField] private MachineSensor m_sensor = new MachineSensor();

		public Vector3 position { get { return transform.position; } }
		public Vector3 direction { get { if (m_motion != null) return m_motion.direction; else return Vector3.zero; } }
		public Machine enemy { 
			get {
				if (m_sensor != null && (m_signals[(int)Signal.Warning] || m_signals[(int)Signal.Danger])) {
					return m_sensor.enemy;
				} else {
					return null;
				}
			}
		}

		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_signals = new bool[(int)Signal.Count];

			m_pilot = GetComponent<Pilot>();

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

		public void SetSignal(Signal _signal, bool _activated) {
			m_signals[(int)_signal] = _activated;
		}

		public bool GetSignal(Signal _signal) {
			return m_signals[(int)_signal];
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