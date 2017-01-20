using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine : MotionInterface {

		Vector3 eye			{ get; }
		Vector3 target		{ get; }
		Vector3 upVector	{ get; set; }
		Transform enemy 	{ get; set; } 

		// Monobehaviour methods
		T GetComponent<T>();
		T[] GetComponentsInChildren<T>();
		Transform transform { get; }

		// Internal connections
		void SetSignal(Signals.Type _signal, bool _activated, object[] _params = null);
		bool GetSignal(Signals.Type _signal);
		object[] GetSignalParams(Signals.Type _signal);

		void DisableSensor(float _seconds);

		void UseGravity(bool _value);
		void CheckCollisions(bool _value);
		void FaceDirection(bool _value);
		bool IsFacingDirection();
		bool HasCorpse();

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		// External interactions
		void LockInCage();
		void UnlockFromCage();

		void Drown();
		void Bite();
		void BeginSwallowed(Transform _transform, bool rewardPlayer);
		void BiteAndHold();
		bool Burn(Transform _transform);

		void SetVelocity(Vector3 _v);

		bool IsDying();

	}
}