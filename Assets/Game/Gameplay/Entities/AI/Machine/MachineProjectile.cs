using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineProjectile : MonoBehaviour, IMachine, ISpawnable {

		[SerializeField] private MachineEdible m_edible = new MachineEdible();
		[SerializeField] private MachineInflammable m_inflammable = new MachineInflammable();

		private Entity m_entity;
		private Projectile m_projectile;

		private bool m_beingEaten;
		private bool m_beingBurned;


		//---------------------------------------------------------------------------------
		public Vector3 position { 	get { return transform.position;  } 
									set { transform.position = value; } 
		}

		public Vector3 eye 				{ get { return m_projectile.position; } }
		public Vector3 target			{ get { return m_projectile.target; } }
		public Vector3 direction 		{ get { return m_projectile.direction; } }
		public Vector3 groundDirection 	{ get { return m_projectile.direction; } } 
		public Vector3 upVector  		{ get { return m_projectile.upVector; } set { } }
		public Vector3 velocity			{ get { return m_projectile.velocity; } }
		public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		public float lastFallDistance { get { return 0; } }

		public bool isKinematic	{ get { return false; } set { } }
		public Transform enemy  { get { return null; }  set { } }
		public bool isPetTarget { get { return false;}  set { } }


		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_entity = GetComponent<Entity>();
			m_projectile = GetComponent<Projectile>();

			m_edible.Attach(this, m_entity, null);
			m_inflammable.Attach(this, m_entity, null);
		}

		public void Spawn(ISpawner _spawner) {
			m_beingEaten = false;
			m_edible.Init();

			m_beingBurned = false;
			m_inflammable.Init();
		}

		public void Activate() {}
		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {}

		public void OnTrigger(string _trigger, object[] _param = null) {}
			
		// Update is called once per frame
		public void SetSignal(Signals.Type _signal, bool _activated, object[] _params) {
			if (_signal == Signals.Type.Destroyed) {
				m_projectile.OnEaten();
				m_entity.Disable(true);
			}
		}
		public bool GetSignal(Signals.Type _signal) { return false; }
		public object[] GetSignalParams(Signals.Type _signal) { return null; }

		public void DisableSensor(float _seconds) 	{}
		public void UseGravity(bool _value) 		{}
		public void CheckCollisions(bool _value)	{}
		public void FaceDirection(bool _value) 		{}
		public bool IsFacingDirection() 			{ return true; }
		public bool HasCorpse() 					{ return true; }

		public void	EnterGroup(ref Group _group) 	{}
		public Group GetGroup() 					{ return null; }
		public void LeaveGroup() 					{}

		// External interactions
		public void ReceiveDamage(float _damage) {}

		public void EnterDevice(bool _isCage) 	{}
		public void LeaveDevice(bool _isCage) 	{}

		// 
		public bool IsDying() { return m_beingEaten || m_beingBurned; }
		public bool IsDead() { return IsDying(); }
		public bool IsFreezing() { return false; }

		public void Drown() {}

		// Being eaten
		public bool CanBeBitten() {
			if (!enabled)					return false;
			if (IsDying())					return false;			

			return true;
		}

		public float biteResistance { get { return m_edible.biteResistance; } }

		public void Bite() {
			if (!IsDying()) {
				m_beingEaten = true;
				m_edible.Bite();
			}
		}

		public void BeginSwallowed(Transform _transform, bool _rewardPlayer, bool _isPlayer) {
			m_edible.BeingSwallowed(_transform, _rewardPlayer, _isPlayer); 
		}

		public void EndSwallowed(Transform _transform) {
			m_edible.EndSwallowed(_transform);
		}

		public HoldPreyPoint[] holdPreyPoints { get { return m_edible.holdPreyPoints; } }

		// Pojectiles can't be held
		public void BiteAndHold() {}
		public void ReleaseHold() {}

		public Quaternion GetDyingFixRot() {
			return Quaternion.identity;
		}

		// Being burned
		public bool Burn(Transform _transform) {
			if (!IsDying()) {
				m_beingBurned = true;
				m_inflammable.Burn(_transform);
			}
			return false;
		}

		public void SetVelocity(Vector3 _v) {}
		public void AddExternalForce(Vector3 _f) {}

		public virtual void CustomUpdate(){}

		public virtual void CustomFixedUpdate(){}
	}
}