using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public interface IMachine {
		//
		Vector3 position { get; }
		Vector3 direction { get; }

		// Internal connections
		void SetSignal(string _signal, bool _activated);
		bool GetSignal(string _signal);

		// Group membership -> for collective behaviours
		void	EnterGroup(ref Group _group);
		Group 	GetGroup();
		void	LeaveGroup();

		// External interactions
		void Bite();
		void BeingSwallowed(Transform _transform);
		void BiteAndHold();
		void Burn(float _damage, Transform _transform);
	}
}