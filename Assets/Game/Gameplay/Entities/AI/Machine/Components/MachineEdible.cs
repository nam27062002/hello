using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
	[Serializable]
	public class MachineEdible : MachineComponent {

		public override Type type { get { return Type.Edible; } }

		//-----------------------------------------------
		//
		//-----------------------------------------------
		private float m_biteResistance = 1f;
		public float biteResistance { get { return m_biteResistance; }}

		private HoldPreyPoint[] m_holdPreyPoints = null;
		public HoldPreyPoint[] holdPreyPoints { get{ return m_holdPreyPoints; } }


		public MachineEdible() {}

		public override void Attach (IMachine _machine, IEntity _entity, Pilot _pilot){
			base.Attach (_machine, _entity, _pilot);

			m_biteResistance = m_entity.def.GetAsFloat("biteResistance");

			if (_pilot != null) {
				m_holdPreyPoints = m_pilot.transform.GetComponentsInChildren<HoldPreyPoint>();
			}
		}

		public override void Init() {}

		public void Bite() {
			m_machine.SetSignal(Signals.Type.Panic, true);
			m_machine.SetSignal(Signals.Type.Chewing, true);

			if (EntityManager.instance != null)
				EntityManager.instance.UnregisterEntity(m_entity as Entity);
		}

		public void BeingSwallowed(Transform _transform, bool _rewardsPlayer) {			
			if (_rewardsPlayer) {
				// Get the reward to be given from the entity
				Reward reward = (m_entity as Entity).GetOnKillReward(false);
				reward.alcohol = 0;
				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, m_machine.transform, reward);
			}
		}

		public void EndSwallowed( Transform _transform ){
			m_machine.SetSignal(Signals.Type.Destroyed, true);
		}

		public void BiteAndHold() {
			m_machine.SetSignal(Signals.Type.Panic, true);
			m_machine.SetSignal(Signals.Type.Latched, true);
		}

		public void ReleaseHold() {
			m_machine.SetSignal(Signals.Type.Panic, false);
			m_machine.SetSignal(Signals.Type.Latched, false);
		}

		public override void Update() {}
	}
}