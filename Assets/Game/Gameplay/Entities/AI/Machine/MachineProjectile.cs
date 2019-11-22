using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class MachineProjectile : IMachine {

		[SerializeField] private MachineEdible m_edible = new MachineEdible();
		[SerializeField] private MachineInflammable m_inflammable = new MachineInflammable();

		private Entity m_entity;
		private Projectile m_projectile;

		IEntity.DyingReason m_dyingReason = IEntity.DyingReason.OTHER;

		//---------------------------------------------------------------------------------
		override public Quaternion orientation 	{ get { return transform.rotation; } set { transform.rotation = value; } }

		override public Vector3 position { 	get { return transform.position;  } 
									set { transform.position = value; } 
		}

		override public Vector3 eye 				{ get { return m_projectile.position; } }
		override public Vector3 target			{ get { return m_projectile.target; } }
		override public Vector3 direction 		{ get { return m_projectile.direction; } }
		override public Vector3 groundDirection 	{ get { return m_projectile.direction; } } 
		override public Vector3 upVector  		{ get { return m_projectile.upVector; } set { } }
		override public Vector3 velocity			{ get { return m_projectile.velocity; } }
		override public Vector3 angularVelocity	{ get { return Vector3.zero; } }

		override public float lastFallDistance { get { return 0; } }

		override public bool isKinematic	{ get { return false; } set { } }
		override public Transform enemy  { get { return null; } }
		override public bool isPetTarget { get { return false;}  set { } }


		//---------------------------------------------------------------------------------

		// Use this for initialization
		void Awake() {
			m_entity = GetComponent<Entity>();
			m_projectile = GetComponent<Projectile>();

			m_edible.Attach(this, m_entity, null);
			m_inflammable.Attach(this, m_entity, null);
		}

		override public void Spawn(ISpawner _spawner) {
			m_dyingReason = IEntity.DyingReason.OTHER;
			m_edible.Init();
			m_inflammable.Init();
		}

		override public void Activate() {}
		override public void Deactivate( float duration, UnityEngine.Events.UnityAction _action) {}

		override public void OnTrigger(int _triggerHash, object[] _param = null) {}
			
		// Update is called once per frame
		override public void SetSignal(Signals.Type _signal, bool _activated) {
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
		override public void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params) {
			SetSignal(_signal, _activated);
		}
		override public bool GetSignal(Signals.Type _signal) { return false; }
		override public object[] GetSignalParams(Signals.Type _signal) { return null; }

		override public void DisableSensor(float _seconds) 	{}
		override public void UseGravity(bool _value) 		{}
		override public void CheckCollisions(bool _value)	{}
		override public void FaceDirection(bool _value) 		{}
		override public bool IsFacingDirection() 			{ return true; }
		override public bool IsInFreeFall() 			{ return false; }
		override public bool HasCorpse() 					{ return true; }

		override public void	EnterGroup(ref Group _group) 	{}
		override public Group GetGroup() 					{ return null; }
		override public void LeaveGroup() 					{}

		// External interactions
		override public void ReceiveDamage(float _damage) {}

		override public void EnterDevice(bool _isCage) 	{}
		override public void LeaveDevice(bool _isCage) 	{}

		// 
		override public bool IsDying() { return m_dyingReason != IEntity.DyingReason.OTHER; }
		override public bool IsDead() { return IsDying(); }
		public bool IsFreezing() { return false; }
        override public bool IsStunned() { return false; }
        override public bool IsInLove() { return false; }
        override public bool IsBubbled() { return false; }

        override public void Drown() {}

		// Being eaten
		override public bool CanBeBitten() {
			if (!enabled)					return false;
			if (IsDying())					return false;			

			return true;
		}

		override public float biteResistance { get { return m_edible.biteResistance; } }

		override public void Bite() {
			if (!IsDying()) {
                m_projectile.OnBite();
				m_edible.Bite();
			}
		}

		override public void BeginSwallowed(Transform _transform, bool _rewardPlayer, IEntity.Type _source, KillType _killType) {
			m_edible.BeingSwallowed(_transform, _rewardPlayer, _source, _killType); 
		}

		override public void EndSwallowed(Transform _transform) {
			m_edible.EndSwallowed(_transform);
		}

		override public HoldPreyPoint[] holdPreyPoints { get { return m_edible.holdPreyPoints; } }

		// Pojectiles can't be held
		override public void BiteAndHold() {}
		override public void ReleaseHold() {}

		override public Quaternion GetDyingFixRot() {
			return Quaternion.identity;
		}

		// Being burned
		override public bool Burn(Transform _transform, IEntity.Type _source, KillType _killType = KillType.BURNT, bool _instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED) {
			if (!IsDying()) {
				m_dyingReason = IEntity.DyingReason.BURNED;
				SetSignal(Signals.Type.Destroyed, true);
				//m_inflammable.Burn(_transform, _source, instant);
			}
			return false;
		}

		override public bool Smash( IEntity.Type _source ) {
			if ( !IsDead() && !IsDying() )
			{
				m_dyingReason = IEntity.DyingReason.DESTROYED;
				m_entity.onDieStatus.source = _source;
				m_entity.onDieStatus.reason = IEntity.DyingReason.DESTROYED;
				SetSignal(Signals.Type.Destroyed, true);
				Reward reward = m_entity.GetOnKillReward(IEntity.DyingReason.DESTROYED);
                Messenger.Broadcast<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, transform, m_entity, reward, KillType.EATEN);
				return true;
			}
			return false;
		}



		override public void SetVelocity(Vector3 _v) {}
		override public void AddExternalForce(Vector3 _f) {}

		override public void CustomUpdate(){}
		override public void CustomFixedUpdate(){}
        public override void CustomLateUpdate() { }
    }
}