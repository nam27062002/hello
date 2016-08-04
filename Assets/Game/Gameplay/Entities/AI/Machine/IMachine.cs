using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine {
		//
		Vector3 position 	{ get; set; }
		Vector3 target		{ get; }
		Vector3 direction 	{ get; }
		Vector3 upVector	{ get; set; }
		Transform enemy 	{ get; } 

		// Monobehaviour methods
		T GetComponent<T>();
		T[] GetComponentsInChildren<T>();
		Transform transform { get; }

		// Internal connections
		void SetSignal(Signals.Type _signal, bool _activated);
		bool GetSignal(Signals.Type _signal);

		void StickToCollisions(bool _value);
		void FaceDirection(bool _value);
		bool IsFacingDirection();

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		// External interactions
		void Bite();
		void BeingSwallowed(Transform _transform);
		void BiteAndHold();
		bool Burn(float _damage, Transform _transform);
	}
}