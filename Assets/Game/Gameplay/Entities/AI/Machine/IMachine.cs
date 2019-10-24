using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public abstract class IMachine : ISpawnable, IMotion {

		public abstract Quaternion orientation 	{ get; set; }
		public abstract Vector3 position 		{ get; set; }
		public abstract Vector3 direction 		{ get; }
		public abstract Vector3 groundDirection { get; }
		public abstract Vector3 velocity 		{ get; }
		public abstract Vector3 angularVelocity { get; }

		public abstract Vector3 	eye			{ get; }
		public abstract Vector3 	target		{ get; }
		public abstract Vector3 	upVector	{ get; set; }
		public abstract Transform 	enemy 		{ get; } 
		public abstract bool 	isPetTarget	{ get; set; }

		public abstract float lastFallDistance 	{ get; }
		public abstract bool isKinematic 		{ get; set; }
			
		//
		public abstract void Activate();
		public abstract void Deactivate(float duration, UnityEngine.Events.UnityAction _action);


		// Internal connections
		public abstract void SetSignal(Signals.Type _signal, bool _activated);
		public abstract void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params);
		public abstract bool GetSignal(Signals.Type _signal);
		public abstract object[] GetSignalParams(Signals.Type _signal);

		public abstract void OnTrigger(int _triggerHash, object[] _param = null);

		public abstract void DisableSensor(float _seconds);

		public abstract void UseGravity(bool _value);
		public abstract void CheckCollisions(bool _value);
		public abstract void FaceDirection(bool _value);
		public abstract bool IsFacingDirection();
		public abstract bool IsInFreeFall();
		public abstract bool HasCorpse();

		// Group membership -> for collective behaviours
		public abstract void	EnterGroup(ref Group _group);
		public abstract Group 	GetGroup();
		public abstract void	LeaveGroup();

		public abstract void ReceiveDamage(float _damage);

		public abstract void EnterDevice(bool _isCage);
		public abstract void LeaveDevice(bool _isCage);

		public abstract void Drown();

		public abstract bool CanBeBitten();
		public abstract float biteResistance { get; }
		public abstract HoldPreyPoint[] holdPreyPoints { get; }

		public abstract void Bite();
		public abstract void BeginSwallowed(Transform _transform, bool rewardPlayer, IEntity.Type _source, KillType _killType);
		public abstract void EndSwallowed(Transform _transform);
		public abstract void BiteAndHold();
		public abstract void ReleaseHold();

		public abstract Quaternion GetDyingFixRot();

		public abstract bool Burn(Transform _transform, IEntity.Type _source, KillType _killType = KillType.BURNT, bool _instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED );

		public abstract bool Smash( IEntity.Type _source );

		public abstract void SetVelocity(Vector3 _v);
		public abstract void AddExternalForce(Vector3 _f);

		public abstract bool IsDead();
		public abstract bool IsDying();
        public abstract bool IsStunned();
        public abstract bool IsInLove();
        public abstract bool IsBubbled();

		public abstract void CustomFixedUpdate();
        public abstract void CustomLateUpdate();
    }
}