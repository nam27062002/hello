using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine : IMotion {

		Vector3 	eye			{ get; }
		Vector3 	target		{ get; }
		Vector3 	upVector	{ get; set; }
		Transform 	enemy 		{ get; } 
		bool 		isPetTarget	{ get; set; }

		// Monobehaviour methods
		T GetComponent<T>();
		T[] GetComponentsInChildren<T>();
		Transform transform 	{ get; }
		GameObject gameObject 	{ get; }

		float lastFallDistance 	{ get; }
		bool isKinematic 		{ get; set; }
			
		//
		void Spawn(ISpawner _spawner);
		void Activate();
		void Deactivate(float duration, UnityEngine.Events.UnityAction _action);


		// Internal connections
		void SetSignal(Signals.Type _signal, bool _activated);
		void SetSignal(Signals.Type _signal, bool _activated, ref object[] _params);
		bool GetSignal(Signals.Type _signal);
		object[] GetSignalParams(Signals.Type _signal);

		void OnTrigger(string _trigger, object[] _param = null);

		void DisableSensor(float _seconds);

		void UseGravity(bool _value);
		void CheckCollisions(bool _value);
		void FaceDirection(bool _value);
		bool IsFacingDirection();
		bool IsInFreeFall();
		bool HasCorpse();

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		void ReceiveDamage(float _damage);

		void EnterDevice(bool _isCage);
		void LeaveDevice(bool _isCage);

		void Drown();

		bool CanBeBitten();
		float biteResistance { get; }
		HoldPreyPoint[] holdPreyPoints { get; }

		void Bite();
		void BeginSwallowed(Transform _transform, bool rewardPlayer, IEntity.Type _source);
		void EndSwallowed(Transform _transform);
		void BiteAndHold();
		void ReleaseHold();

		Quaternion GetDyingFixRot();

		bool Burn(Transform _transform, IEntity.Type _source, bool instant = false, FireColorSetupManager.FireColorType fireColorType = FireColorSetupManager.FireColorType.RED );

		bool Smash( IEntity.Type _source );

		void SetVelocity(Vector3 _v);
		void AddExternalForce(Vector3 _f);

		bool IsDead();
		bool IsDying();
        bool IsStunned();
        bool IsInLove();

		void CustomFixedUpdate();

	}
}