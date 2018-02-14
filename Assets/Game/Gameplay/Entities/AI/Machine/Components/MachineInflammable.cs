﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI {
	[Serializable]
	public class MachineInflammable : MachineComponent {

		public override Type type { get { return Type.Inflammable; } }

		//-----------------------------------------------
		// Constants
		//-----------------------------------------------
		enum State {
			Idle = 0,
			Burning,
			Burned,
			Ashes
		};


		//-----------------------------------------------
		//
		//-----------------------------------------------
		[SerializeField] private float m_burningTime = 0f;
		[SerializeField] private bool m_canBeDissolved = true;
		[SerializeField] private List<Renderer> m_ashExceptions = new List<Renderer>();

		private float m_actualBurningTime = 0;
		public float burningTime { get { return m_actualBurningTime; } }

		//-----------------------------------------------
		//
		//-----------------------------------------------
		private List<Renderer> m_renderers;

		private float m_timer;
		private State m_state;
		private State m_nextState;


		//-----------------------------------------------
		public MachineInflammable() {}

		public override void Init() {
			// Renderers And Materials
			if (m_renderers == null) {
				m_renderers = new List<Renderer>();
				Renderer[] renderers = m_machine.GetComponentsInChildren<Renderer>();

				if (renderers.Length > 0) {
					for(int i = 0; i < renderers.Length; i++) {
						Renderer renderer = renderers[i];
						if (!m_ashExceptions.Contains(renderer)) {
							m_renderers.Add(renderer);
						}
					}
				}
			}

			for( int i = 0; i < m_ashExceptions.Count; i++ )
				m_ashExceptions[i].enabled = true;

			m_state = State.Idle;
			m_nextState = State.Idle;
		}

		public List<Renderer> GetBurnableRenderers() {			
			return m_renderers;
		}

		public void Burn(Transform _transform, IEntity.Type _source, bool instant = false) {
			// raise flags
			m_machine.SetSignal(Signals.Type.Burning, true);
			m_machine.SetSignal(Signals.Type.Panic, true);

			// Initialize some death info
			m_entity.onDieStatus.source = _source;
			m_entity.onDieStatus.isInFreeFall = m_machine.IsInFreeFall();
			m_entity.onDieStatus.isPressed_ActionA = m_pilot.IsActionPressed(Pilot.Action.Button_A);
			m_entity.onDieStatus.isPressed_ActionB = m_pilot.IsActionPressed(Pilot.Action.Button_B);
			m_entity.onDieStatus.isPressed_ActionC = m_pilot.IsActionPressed(Pilot.Action.Button_C);

			if (m_pilot != null)
				m_pilot.OnDie();

			// reward
			Reward reward = m_entity.GetOnKillReward(true);
			Messenger.Broadcast<Transform, Reward>(MessengerEvents.ENTITY_BURNED, m_machine.transform, reward);

			if ( instant )
			{
				m_actualBurningTime = 0;
			}
			else
			{
				m_actualBurningTime = m_burningTime;
			}
			m_nextState = State.Burning;
		}

		public void Burned() {
			m_nextState = State.Ashes;
		}

		//------------------------------------------------------------------------------------------------------------------------
		public override void Update() {
			if (m_state != m_nextState) {
				ChangeState();
			}

			if (m_state == State.Burning) {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					m_timer = 0f;
					if (m_canBeDissolved) {
						m_nextState = State.Burned;
					} else {
						m_nextState = State.Ashes;
					}
				}
			}
		}

		private void ChangeState() {
			switch(m_nextState) {
				case State.Burning:
					m_timer = m_actualBurningTime;
					break;

				case State.Burned:					
					for (int i = 0; i < m_ashExceptions.Count; ++i) {
						m_ashExceptions[i].enabled = false;
					}
					MachineInflammableManager.Add(this);
					break;

				case State.Ashes:
					m_machine.SetSignal(Signals.Type.Destroyed, true);
					break;
			}

			m_state = m_nextState;
		}
	}
}