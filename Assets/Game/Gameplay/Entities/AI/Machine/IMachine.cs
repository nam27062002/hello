﻿using UnityEngine;
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

		float lastFallDistance { get; }

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
		void ReceiveDamage(float _damage);

		void LockInCage();
		void UnlockFromCage();

		void Drown();

		bool CanBeBitten();
		float biteResistance { get; }
		HoldPreyPoint[] holdPreyPoints { get; }

		void Bite();
		void BeginSwallowed(Transform _transform, bool rewardPlayer);
		void EndSwallowed(Transform _transform);
		void BiteAndHold();
		void ReleaseHold();

		Quaternion GetDyingFixRot();

		bool Burn(Transform _transform);

		void SetVelocity(Vector3 _v);

		bool IsDead();
		bool IsDying();

	}
}