using UnityEngine;
using System;
using System.Collections.Generic;

namespace AI {
	[Serializable]
	public class MachineEdible : MachineComponent {
		//-----------------------------------------------
		public enum RotateToMouthType {
			No,				// does not have any special rotation applied while being eaten
			TailFirst,		// rotates to be swallowed tail first - use with swallow shader
			HeadFirst,		// rotates to be swallowed head first - "
			ClosestFirst,	// rotates to swallow either head or tail first, whichever is closer - "
			Sideways,		// turns sideways - use when breaking into corpse chunks with centre chunk staying in mouth
		};

		//-----------------------------------------------
		public override Type type { get { return Type.Edible; } }

		//-----------------------------------------------
		[SerializeField] private RotateToMouthType m_rotateToMouth;
		public RotateToMouthType rotateToMouth {
			get { return m_rotateToMouth; }
			set { m_rotateToMouth = value; }
		}

		//-----------------------------------------------
		//
		//-----------------------------------------------
		private Transform m_machineTransform;

		private float m_biteResistance = 1f;
		public float biteResistance { get { return m_biteResistance; }}

		private HoldPreyPoint[] m_holdPreyPoints = null;
		public HoldPreyPoint[] holdPreyPoints { get{ return m_holdPreyPoints; } }


		public MachineEdible() {}

		public override void Attach (IMachine _machine, IEntity _entity, Pilot _pilot){
			base.Attach (_machine, _entity, _pilot);

			m_machineTransform = m_machine.transform;

			m_biteResistance = m_entity.def.GetAsFloat("biteResistance");

			if (_pilot != null) {
				m_holdPreyPoints = m_pilot.transform.GetComponentsInChildren<HoldPreyPoint>();
			}
		}

		public override void Init() {}

		public Quaternion GetDyingFixRot() {
			Quaternion result = Quaternion.identity;

			if (m_rotateToMouth == RotateToMouthType.No)
				return result;

			if (m_rotateToMouth == RotateToMouthType.Sideways) {
				float diff = Quaternion.Angle(m_machineTransform.localRotation, Quaternion.AngleAxis(90.0f, Vector3.up));
				float rot = (diff > 90.0f) ? 270.0f : 90.0f;
				result = Quaternion.AngleAxis(rot, Vector3.up);
			} else {
				bool headFirst = (m_rotateToMouth == RotateToMouthType.HeadFirst);
				if (m_rotateToMouth == RotateToMouthType.ClosestFirst) {
					Transform trParent = m_machine.transform.parent;	// null check on this to handle case of attacker being deleted or something
					if ((trParent != null) && trParent.InverseTransformDirection(m_machineTransform.right).x < 0.0f)
						headFirst = true;
				}
				result = headFirst ? Quaternion.AngleAxis(180.0f, Vector3.up) : Quaternion.identity;
			}

			result = result * Quaternion.AngleAxis(90f, Vector3.forward);

			return result;
		}

		public void Bite() {
			m_machine.SetSignal(Signals.Type.Panic, true);
			m_machine.SetSignal(Signals.Type.Chewing, true);

			if (EntityManager.instance != null)
				EntityManager.instance.UnregisterEntity(m_entity as Entity);
		}

		public void BeingSwallowed(Transform _transform, bool _rewardsPlayer, bool _isPlayer) {			
			if (_rewardsPlayer) {
				// Get the reward to be given from the entity
				Reward reward = (m_entity as Entity).GetOnKillReward(false);
				if (!_isPlayer){
					reward.alcohol = 0;
					// Pets never harm player if they eat bad junk
					if (reward.health < 0)
					{
						reward.health = 0;
					}
				}

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