using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineProjectile : MonoBehaviour, IMachine, ISpawnable {

		[SerializeField] private MachineEdible m_edible = new MachineEdible();
		[SerializeField] private MachineInflammable m_inflammable = new MachineInflammable();

		private Entity m_entity;
		private Projectile m_projectile;

		IEntity.DyingReason m_dyingReason = IEntity.DyingReason.OTHER;

		//---------------------------------------------------------------------------------
		public virtual Quaternion orientation 	{ get { return transform.rotation; } set { transform.rotation = value; } }

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
			m_dyingReason = IEntity.DyingReason.OTHER;
			m_edible.Init();
			m_inflammable.Init();
		}

		public void Activate() {}
		public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {}

		public void OnTrigger(string _trigger, object[] _param = null) {}
			
		// Update is called once per frame
		public void SetSignal(Signals.Type _signal, bool _activated) {
			if (_signal == Signals.Type.Destroyed) {
				switch( m_dyingReason )
				{
					case IEntity.DyingReason.BURNED:
					{
						m_projectile.OnBurned();
					}break;
					case IEntity.DyingReason.DESTROYED:
					{
						m_projectile.OnDestoyed();
					}break;
					default:
					case IEntity.DyingReason.EATEN:
					{
						m_projectile.OnEaten();
					}break;

				}
				m_entity.Disable(true);
			}
		}
		public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			SetSignal(_signal, _activated);
		}
		public bool GetSignal(Signals.Type _signal) { return false; }
		public object[] GetSignalParams(Signals.Type _signal) { return null; }

		public void DisableSensor(float _seconds) 	{}
		public void UseGravity(bool _value) 		{}
		public void CheckCollisions(bool _value)	{}
		public void FaceDirection(bool _value) 		{}
		public bool IsFacingDirection() 			{ return true; }
		public virtual bool IsInFreeFall() 			{ return false; }
		public bool HasCorpse() 					{ return true; }

		public void	EnterGroup(ref Group _group) 	{}
		public Group GetGroup() 					{ return null; }
		public void LeaveGroup() 					{}

		// External interactions
		public void ReceiveDamage(float _damage) {}

		public void EnterDevice(bool _isCage) 	{}
		public void LeaveDevice(bool _isCage) 	{}

		// 
		public bool IsDying() { return m_dyingReason != IEntity.DyingReason.OTHER; }
		public bool IsDead() { return IsDying(); }
		public bool IsFreezing() { return false; }
        public bool IsStunned() { return false; }
        public bool IsInLove() { return false; }
        public bool IsBubbled() { return false; }

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
                m_projectile.OnBite();
				m_edible.Bite();
			}
		}

		public void BeginSwallowed(Transform _transform, bool _rewardPlayer, IEntity.Type _source) {
			m_edible.BeingSwallowed(_transform, _rewardPlayer, _source); 
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
		public bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			if (!IsDying()) {
				m_dyingReason = IEntity.DyingReason.BURNED;
				SetSignal(Signals.Type.Destroyed, true);
				//m_inflammable.Burn(_transform, _source, instant);
			}
			return false;
		}

		public bool Smash( IEntity.Type _source ) {
			if ( !IsDead() && !IsDying() )
			{
				m_dyingReason = IEntity.DyingReason.DESTROYED;
				m_entity.onDieStatus.source = _source;
				m_entity.onDieStatus.reason = IEntity.DyingReason.DESTROYED;
				SetSignal(Signals.Type.Destroyed, true);
				Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.DESTROYED);
				Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, transform, m_entity, reward);
				return true;
			}
			return false;
		}



		public void SetVelocity(Vector3 _v) {}
		public void AddExternalForce(Vector3 _f) {}

		public virtual void CustomUpdate(){}

		public virtual void CustomFixedUpdate(){}
	}
}