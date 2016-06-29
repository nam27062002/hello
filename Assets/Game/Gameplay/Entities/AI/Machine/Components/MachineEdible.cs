﻿using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineEdible : MachineComponent {

		private float m_biteResistance = 1f;
		public float biteResistance { get { return m_biteResistance; }}

		private List<Transform> m_holdPreyPoints = new List<Transform>();
		public List<Transform> holdPreyPoints { get{ return m_holdPreyPoints; } }



		public MachineEdible() {}

		public override void Init() {
			m_biteResistance = m_entity.def.GetAsFloat("biteResistance");

			HoldPreyPoint[] holdPoints = m_machine.transform.GetComponentsInChildren<HoldPreyPoint>();
			if (holdPoints != null) {
				for (int i = 0;i<holdPoints.Length; i++) {
					m_holdPreyPoints.Add(holdPoints[i].transform);
				}
			}
		}

		public void Bite() {
			m_machine.SetSignal(Signals.Panic.name, true);
			Debug.Log(m_machine.name + " : I've been bitten!! >_<");

			//TODO: play sound
		}

		public void BeingSwallowed(Transform _transform) {			
			// Get the reward to be given from the entity
			Reward reward = m_entity.GetOnKillReward(false);

			// Dispatch global event
			Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, m_machine.transform, reward);

			m_viewControl.SpawnEatenParticlesAt(_transform);

			m_machine.SetSignal(Signals.Destroyed.name, true);
		}

		public void BiteAndHold() {
			m_machine.SetSignal(Signals.Panic.name, true);
		}

		public void ReleaseHold() {
			m_machine.SetSignal(Signals.Panic.name, false);
		}

		public override void Update() {}
	}
}