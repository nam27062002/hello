using UnityEngine;
using System.Collections;

namespace AI {
	public class Machine : IMachine, MonoBehaviour {
		/**************/
		/*			  */
		/**************/
		private enum Sensor {
			Leader = 0,
			Hungry,
			Danger,
			Scared,
			Panic,
			Burning,
			Eaten,
			Dead,
			Count
		};

		/**************/
		/*			  */
		/**************/

		private IPilot m_pilot = null;
		private bool[] m_sensors;

		private MachineMotion m_motion;

		// Use this for initialization
		void Start() {

			m_pilot = GetComponent<IPilot>();

			m_motion = new MachineMotion(this);
			m_motion.AttachPilot(m_pilot);
		}
		
		// Update is called once per frame
		void Update() {

			m_motion.Update();

		}
	}
}