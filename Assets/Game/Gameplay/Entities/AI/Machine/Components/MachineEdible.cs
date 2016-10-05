using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineEdible : MachineComponent {

		private float m_biteResistance = 1f;
		public float biteResistance { get { return m_biteResistance; }}

		private List<Transform> m_holdPreyPoints = new List<Transform>();
		public List<Transform> holdPreyPoints { get{ return m_holdPreyPoints; } }

		private ViewControl m_viewControl;


		public MachineEdible() {}

		public override void Attach (IMachine _machine, IEntity _entity, Pilot _pilot){
			base.Attach (_machine, _entity, _pilot);

			m_viewControl = m_machine.GetComponent<ViewControl>();
			m_biteResistance = (m_entity as Entity).def.GetAsFloat("biteResistance");
			HoldPreyPoint[] holdPoints = m_pilot.transform.GetComponentsInChildren<HoldPreyPoint>();
			if (holdPoints != null) {
				for (int i = 0;i<holdPoints.Length; i++) {
					m_holdPreyPoints.Add(holdPoints[i].transform);
				}
			}
		}

		public override void Init() {
			m_machine.SetSignal(Signals.Type.Destroyed, false);
		}

		public void Bite() {
			m_machine.SetSignal(Signals.Type.Panic, true);
			m_machine.SetSignal(Signals.Type.Chewing, true);

			if (EntityManager.instance != null)
				EntityManager.instance.Unregister(m_entity as Entity);
		}

		public void BeingSwallowed(Transform _transform, bool _rewardsPlayer) {			
			if ( _rewardsPlayer ){
				// Get the reward to be given from the entity
				Reward reward = (m_entity as Entity).GetOnKillReward(false);

				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, m_pilot.transform, reward);
			}

			m_viewControl.SpawnEatenParticlesAt(_transform);

			m_machine.SetSignal(Signals.Type.Destroyed, true);
		}

		public void BiteAndHold() {
			m_machine.SetSignal(Signals.Type.Panic, true);
		}

		public void ReleaseHold() {
			m_machine.SetSignal(Signals.Type.Panic, false);
		}

		public override void Update() {}
	}
}